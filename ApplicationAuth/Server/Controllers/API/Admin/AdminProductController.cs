using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Controllers.API;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.RequestModels.Product;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Server.Helpers.Attributes;
using ApplicationAuth.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace ApplicationAuth.Server.Controllers.API.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admins)]
    [Validate]
    public class AdminProductController : _BaseApiController
    {
        private readonly IProductService _productService;

        public AdminProductController(IStringLocalizer<ErrorsResource> errorsLocalizer,
                                      IProductService productService) : base(errorsLocalizer)
        {
            _productService = productService;
        }

        // POST api/v1/adminproduct
        /// <summary>
        /// Create new product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/v1/adminproduct
        ///
        /// </remarks>
        /// <returns>HTTP 200 with PostResponseModel or HTTP 401, 500 with error message</returns>
        /// <response code="200">Product created successful</response>
        /// <response code="401">Unauthorized</response>   
        /// <response code="500">Internal server error</response>  
        [HttpPost]
        [PreventSpam(Name = "CreateProduct", Seconds = 2)]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> Create([FromForm] ProductRequestModel model)
        {
            var resposne = await _productService.Add(model);

            return Json(new JsonResponse<ProductResponseModel>(resposne));
        }


        // POST api/v1/adminproduct/edit/{id}
        /// <summary>
        /// Edit a product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/v1/adminproduct/edit/123
        ///
        /// </remarks>
        /// <returns>HTTP 200 with PostResponseModel or HTTP 400, 401, 500 with error message</returns>
        /// <response code="200">Product edited successful</response>
        /// <response code="400">If the product is not found</response>
        /// <response code="401">Unauthorized</response>   
        /// <response code="500">Internal server error</response>  
        [HttpPost("edit/{id}")]
        [PreventSpam(Name = "EditProduct")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.BadRequest, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> Edit([FromBody] ProductRequestModel model, [FromRoute] int id/*, [FromForm] IFormFile image*/)
        {
            var resposne = await _productService.Edit(model, id);

            return Json(new JsonResponse<ProductResponseModel>(resposne));
        }


        // DELETE api/v1/adminproduct/{id}
        /// <summary>
        /// Delete a product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/adminproduct/123
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 400, 401, 500 with error message</returns>
        /// <response code="200">Product deleted successful</response>
        /// <response code="400">If the product is not found</response>
        /// <response code="401">Unauthorized</response>   
        /// <response code="500">Internal server error</response>  
        [HttpDelete("{id}")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.BadRequest, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var resposne = await _productService.DeleteById(id);

            return Json(new JsonResponse<MessageResponseModel>(new(resposne)));
        }
    }
}
