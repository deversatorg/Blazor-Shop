using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NuGet.Common;
using Swashbuckle.AspNetCore.Filters;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ApplicationAuth.DAL;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.DAL.Repository;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.Common.Utilities;
using ApplicationAuth.DAL.UnitOfWork;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Services;
using ApplicationAuth.Services.StartApp;
using ApplicationAuth.Server.Helpers.SwaggerFilters;
using ApplicationAuth.Common.Constants;
using System.IdentityModel.Tokens.Jwt;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Server.Helpers;
using ApplicationAuth.DAL.Migrations;
using NLog.Layouts;
using NLog.Targets;
using NLog;
using Smart.Blazor;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Connection"));
    options.EnableSensitiveDataLogging(false);
});


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#=";
}).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
{
    o.Name = "Default";
    o.TokenLifespan = TimeSpan.FromHours(12);
});

builder.Services.AddCors();

// Add services to the container.
#region Register services

#region Blazor
// Add Smart UI for Blazor.  
builder.Services.AddSmart();
#endregion

#region Basis services

builder.Services.AddScoped<IDataContext>(provider => provider.GetService<DataContext>());
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IHashUtility, HashUtility>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFileService, FileService>();

#endregion

var config = new AutoMapper.MapperConfiguration(cfg =>
{
    cfg.AddProfile(new AutoMapperProfileConfiguration());
});

builder.Services.AddSingleton(config.CreateMapper());

#endregion

var appBasePath = System.IO.Directory.GetCurrentDirectory();
NLog.GlobalDiagnosticsContext.Set("appbasepath", appBasePath);

// NLog: setup the logger first to catch all errors
var logger = NLog.Web.NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
try
{
    foreach (FileTarget target in LogManager.Configuration.AllTargets)
    {
        target.FileName = appBasePath + "/" + ((SimpleLayout)target.FileName).OriginalText;
    }

    LogManager.ReconfigExistingLoggers();

    logger.Debug("init main");

    var configbuilder = new ConfigurationBuilder()
    .SetBasePath(appBasePath)
    .AddJsonFile("appsettings.json");

    var cfg = configbuilder.Build();
    using var servicesProvider = builder.Services.BuildServiceProvider();
    try
    {
        var context = servicesProvider.GetRequiredService<DataContext>();
        DbInitializer.Initialize(context, cfg, servicesProvider);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "An error occurred while seeding the database.");
    }

}
catch (Exception ex)
{
    //NLog: catch setup errors
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

builder.Services
    .AddDetection()
    .AddCoreServices()
    .AddRequiredPlatformServices();

builder.Services.AddMiniProfiler(opt =>
{
    opt.RouteBasePath = "/profiler";
}).AddEntityFramework();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddVersionedApiExplorer(
    options =>
    {
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddApiVersioning(o =>
{
    o.ReportApiVersions = true;
    o.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddMvc(options =>
{
    // Allow use optional parameters in actions
    options.AllowEmptyInputInBodyModelBinding = true;
    options.EnableEndpointRouting = false;
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
})
.ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true)
.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

if (!builder.Environment.IsProduction())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.EnableAnnotations();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            In = ParameterLocation.Header,
            Description = "Access token",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OrderActionsBy(x => x.ActionDescriptor.DisplayName);

        // resolve the IApiVersionDescriptionProvider service
        // note: that we have to build a temporary service provider here because one has not been created yet
        var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        // add a custom operation filter which sets default values

        // integrate xml comments
        options.IncludeXmlComments(XmlCommentsFilePath());
        options.IgnoreObsoleteActions();

        options.OperationFilter<DefaultValues>();
        options.OperationFilter<SecurityRequirementsOperationFilter>("Bearer");

        // for deep linking
        options.CustomOperationIds(e => $"{e.HttpMethod}_{e.RelativePath.Replace('/', '-').ToLower()}");
    });

    // instead of options.DescribeAllEnumsAsStrings()
    builder.Services.AddSwaggerGenNewtonsoftSupport();
}

var sp = builder.Services.BuildServiceProvider();
var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidateActor = false,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
        {
            var jwt = securityToken as JwtSecurityToken;

            if (!notBefore.HasValue || !expires.HasValue || DateTime.Compare(expires.Value, DateTime.UtcNow) <= 0)
            {
                return false;
            }

            if (jwt == null)
                return false;

            var isRefresStr = jwt.Claims.FirstOrDefault(t => t.Type == "isRefresh")?.Value;

            if (isRefresStr == null)
                return false;

            var isRefresh = Convert.ToBoolean(isRefresStr);

            if (!isRefresh)
            {
                try
                {
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var hash = scope.ServiceProvider.GetService<IHashUtility>().GetHash(jwt.RawData);
                        return scope.ServiceProvider.GetService<IRepository<UserToken>>().Find(t => t.AccessTokenHash == hash && t.IsActive) != null;
                    }
                }
                catch (Exception ex)
                {
                    var logger = sp.GetService<ILogger<Program>>();
                    logger.LogError(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + ": Exception occured in token validator. Exception message: " + ex.Message + ". InnerException: " + ex.InnerException?.Message);
                    return false;
                }
            }

            return false;
        },
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddRouting();
builder.Services.AddMemoryCache();

var app = builder.Build();

var configuration = builder.Configuration;
var env = builder.Environment;

app.UseDefaultFiles();

var cultures = configuration.GetSection("SupportedCultures").Get<string[]>();

var supportedCultures = new List<CultureInfo>();

foreach (var culture in cultures)
{
    supportedCultures.Add(new CultureInfo(culture));
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

#region Cookie Auth

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies[".AspNetCore.Application.Id"];
    if (!string.IsNullOrEmpty(token))
        context.Request.Headers.Add("Authorization", "Bearer " + token);

    await next();
});

#endregion

app.UseMiniProfiler();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor,

        // IIS is also tagging a X-Forwarded-For header on, so we need to increase this limit, 
        // otherwise the X-Forwarded-For we are passing along from the browser will be ignored
        ForwardLimit = 2
    });
}

if (!Directory.Exists("Logs"))
{
    Directory.CreateDirectory("Logs");
}

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(5)
};

if (!env.IsProduction())
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            //swagger.Host = httpReq.Host.Value;

            var ampersand = "&amp;";

            foreach (var path in swagger.Paths)
            {
                if (path.Value.Operations.Any(x => x.Key == OperationType.Get && x.Value.Deprecated))
                    path.Value.Operations.First(x => x.Key == OperationType.Get).Value.Description = path.Value.Operations.First(x => x.Key == OperationType.Get).Value.Description.Replace(ampersand, "&");

                if (path.Value.Operations.Any(x => x.Key == OperationType.Delete && x.Value?.Description != null))
                    path.Value.Operations.First(x => x.Key == OperationType.Delete).Value.Description = path.Value.Operations.First(x => x.Key == OperationType.Delete).Value.Description.Replace(ampersand, "&");
            }

            var paths = swagger.Paths.ToDictionary(p => p.Key, p => p.Value);
            foreach (KeyValuePair<string, OpenApiPathItem> path in paths)
            {
                swagger.Paths.Remove(path.Key);
                swagger.Paths.Add(path.Key.ToLowerInvariant(), path.Value);
            }
        });
    });

    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(options =>
    {
        options.IndexStream = () => System.IO.File.OpenRead("Views/Swagger/swagger-ui.html");
        options.InjectStylesheet("/Swagger/swagger-ui.style.css");

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());

        options.EnableFilter();

        // for deep linking
        options.EnableDeepLinking();
        options.DisplayOperationId();
    });

    app.UseReDoc(c =>
    {
        c.RoutePrefix = "docs";
        c.SpecUrl("/swagger/v1/swagger.json");
        c.ExpandResponses("200");
        c.RequiredPropsFirst();
    });
}

app.UseCors(builder =>
{
    if (env.IsDevelopment())
        builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();

    if (env.IsStaging())
        builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();

    if (env.IsProduction())
        builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
});

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.UseWebAssemblyDebugging();
app.UseRouting();

#region Error handler

// Different middleware for api and ui requests  
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    var localizer = sp.GetService<IStringLocalizer<ErrorsResource>>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalErrorHandling");

    // Exception handler - show exception data in api response
    appBuilder.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandler = async context =>
        {
            var errorModel = new ErrorResponseModel(localizer);
            var result = new ContentResult();

            var exception = context.Features.Get<IExceptionHandlerPathFeature>();

            if (exception.Error is CustomException)
            {
                var ex = (CustomException)exception.Error;

                result = errorModel.Error(ex);
            }
            else
            {
                var message = exception.Error.InnerException?.Message ?? exception.Error.Message;
                logger.LogError($"{exception.Path} - {message}");

                errorModel.AddError("general", message);
                result = errorModel.InternalServerError(env.IsDevelopment() ? exception.Error.StackTrace : null);
            }

            context.Response.StatusCode = result.StatusCode.Value;
            context.Response.ContentType = result.ContentType;

            await context.Response.WriteAsync(result.Content);
        }
    });

    // Handles responses with status codes (correctly executed requests, without any exceptions)
    appBuilder.UseStatusCodePages(async context =>
    {
        var errorResponse = ErrorHelper.GetError(localizer, context.HttpContext.Response.StatusCode);

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings { Formatting = Formatting.Indented }));
    });
});

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseExceptionHandler("/Error");
    appBuilder.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
});

#endregion

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
});

app.Run();





static string XmlCommentsFilePath()
{

    var basePath = PlatformServices.Default.Application.ApplicationBasePath;
    var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
    return System.IO.Path.Combine(basePath, fileName);

}

static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
{
    var info = new OpenApiInfo()
    {
        Title = $"Shop API {description.ApiVersion}",
        Version = description.ApiVersion.ToString(),
        Description = "The Shop application with Swagger and API versioning."
    };

    if (description.IsDeprecated)
    {
        info.Description += " This API version has been deprecated.";
    }

    return info;
}

string Encode(string input, byte[] key)
{
    HMACSHA256 myhmacsha = new HMACSHA256(key);
    byte[] byteArray = Encoding.UTF8.GetBytes(input);
    MemoryStream stream = new MemoryStream(byteArray);
    byte[] hashValue = myhmacsha.ComputeHash(stream);
    return Base64UrlEncoder.Encode(hashValue);
}
