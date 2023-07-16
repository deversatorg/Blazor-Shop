using ApplicationAuth.Common.Constants;
using ApplicationAuth.Controllers.API;
using ApplicationAuth.Models.RequestModels.Product;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApplicationAuth.Server.Controllers.API
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class ProductController : _BaseApiController
    {
        private readonly IProductService _productService;
        public ProductController(IStringLocalizer<ErrorsResource> errorsLocalizer, 
                                 IProductService productService) : base(errorsLocalizer)
        {
            _productService = productService;
        }

        // GET api/v1/product
        /// <summary>
        /// Get all products
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/v1/product
        ///
        /// </remarks>
        /// <returns>HTTP 200 with array of SmallProducrResponseModel or HTTP 500 with error message</returns>
        /// <response code="200">Products returned successful</response>
        /// <response code="500">Internal server error</response>  
        [HttpGet]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> GetProducts()
        {
            var resposne = await _productService.GetAll();

            return Json(new JsonResponse<IEnumerable<SmallProductResponseModel>>(resposne));
        }

        // GET api/v1/product/{id}
        /// <summary>
        /// Get product by id 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/v1/product/123
        ///
        /// </remarks>
        /// <returns>HTTP 200 with ProductResponseModel or HTTP 500 with error message</returns>
        /// <response code="200">Product returned successful</response>
        /// <response code="400">If the product is not found</response>
        /// <response code="500">Internal server error</response>  
        [HttpGet("{id}")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.BadRequest, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            var resposne = await _productService.GetById(id);

            return Json(new JsonResponse<ProductResponseModel>(resposne));
        }
    }
}
