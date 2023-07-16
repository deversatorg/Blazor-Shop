using ApplicationAuth.Common.Constants;
using ApplicationAuth.Controllers.API;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using ApplicationAuth.Models.RequestModels.Shop;
using ApplicationAuth.Server.Helpers.Attributes;
using ApplicationAuth.Models.ResponseModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ApplicationAuth.Server.Controllers.API
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Validate]
    public class OrderController : _BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IStringLocalizer<ErrorsResource> errorsLocalizer,
                                IOrderService orderService) : base(errorsLocalizer)
        {
            _orderService = orderService;
        }

        // POST api/v1/order
        /// <summary>
        /// Create an order
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/v1/order
        ///     [   
        ///         {
        ///             "productId" : 123,
        ///             "amount" : 12
        ///         },
        ///         {
        ///             "productId" : 321,
        ///             "amount" : 1
        ///         }
        ///     ]
        ///
        /// </remarks>
        /// <returns>HTTP 200 with OrderResponseModel or HTTP 500 with error message</returns>
        /// <response code="200">Order created successful</response>
        /// <response code="500">Internal server error</response>  
        [HttpPost("")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<OrderResponseModel>))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> Create(IEnumerable<ProductInOrderRequestModel> model)
        {
            var response = await _orderService.Create(model);

            return Json(new JsonResponse<OrderResponseModel>(response));
        }

        // GET api/v1/order/{id}
        /// <summary>
        /// Pay for the order
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/v1/order/123
        ///     
        ///
        /// </remarks>
        /// <returns>HTTP 200 with OrderResponseModel or HTTP 500 with error message</returns>
        /// <response code="200">Products returned successful</response>
        /// <response code="500">Internal server error</response>  
        [HttpGet("payment/{id}")]
        [PreventSpam(Name = "Payment")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<string>))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> Payment([FromRoute] int id)
        {
            var response = await _orderService.Payment(id);

            return Json(new JsonResponse<string>(response));
        }

        // GET api/v1/order
        /// <summary>
        /// Payment without creating a order (shoud be used for unauthorized session)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/v1/order
        ///     
        ///
        /// </remarks>
        /// <returns>HTTP 200 with Stripe checkout url or HTTP 500 with error message</returns>
        /// <response code="200">Products returned successful</response>
        /// <response code="500">Internal server error</response>  
        [AllowAnonymous]
        [HttpPost("paymentfromcart")]
        [PreventSpam(Name = "Payment")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<ProductResponseModel>))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> AnonymousPayment([FromBody] IEnumerable<ProductInOrderRequestModel> model)
        {
            var response = await _orderService.Payment(model);

            return Json(new JsonResponse<string>(response));
        }

    }
}
