using ApplicationAuth.Common.Attributes;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Test;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Server.Helpers.Attributes;
using ApplicationAuth.Services.Interfaces;
using Mandrill.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Controllers.API
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Validate]
    public class TestController : _BaseApiController
    {
        private readonly ILogger<TestController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;

        public TestController(IStringLocalizer<ErrorsResource> localizer,
            ILogger<TestController> logger,
            IUnitOfWork unitOfWork,
            IJWTService jwtService,
            IUserService userService,
            IServiceProvider serviceProvider
            )
            : base(localizer)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userService = userService;
        }

        /// <summary>
        /// For Swagger UI
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("authorize")]
        public async Task<IActionResult> AuthorizeWithoutCredentials([FromBody] ShortAuthorizationRequestModel model)
        {
            IQueryable<ApplicationUser> users = null;

            if (model.Id.HasValue)
                users = _unitOfWork.Repository<ApplicationUser>().Get(x => x.Id == model.Id);
            else if (!string.IsNullOrEmpty(model.UserName))
                users = _unitOfWork.Repository<ApplicationUser>().Get(x => x.UserName == model.UserName);

            var user = await users.Include(x => x.Profile).FirstOrDefaultAsync();

            if (user == null)
            {
                Errors.AddError("", "User is not found");
                return Errors.Error(HttpStatusCode.NotFound);
            }

            var result = await _jwtService.BuildLoginResponse(user);

            return Json(new JsonResponse<LoginResponseModel>(result));
        }

        // DELETE api/v1/test/DeleteAccount
        /// <summary>
        /// Hard delete user from db
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/test/DeleteAccount?userid=1
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 40X, 500 with message error</returns>
        [HttpDelete("DeleteAccount")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<MessageResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidData, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> DeleteAccount([FromQuery][ValidateId] int userId)
        {
            await _userService.HardDeleteUser(userId);
            return Json(new JsonResponse<MessageResponseModel>(new("User has been deleted")));
        }

    }
}