using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Common.Utilities;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Product;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels.Shop;
using ApplicationAuth.Models.ResponseModels.Order;
using ApplicationAuth.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _env;
        
        private int? _userId = null;

        public OrderService(IUnitOfWork unitOfWork, 
                            IMapper mapper, 
                            IHttpContextAccessor httpContextAccessor,
                            IHostingEnvironment env)
        {
            StripeConfiguration.ApiKey = "sk_test_51LTUiIJa5jvq82MZdCu4gwZdNOEQ9yKtLiCrwPlEflxrJuPeKk9OIrYLnjo7NyQzdSshcCK8gGmzXTdReLIwBB0a0001Od76KC";
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _env = env;


            var context = httpContextAccessor.HttpContext;

            if (context?.User != null)
            {

                try
                {
                    _userId = context.User.GetUserId();
                }
                catch
                {
                    _userId = null;
                }
            }
        }

        public async Task<OrderResponseModel> Create(IEnumerable<ProductInOrderRequestModel> model)
        {
            var productsInOrder = new List<ProductInOrder>();

            var products = _unitOfWork.Repository<Domain.Entities.Product.Product>()
                                        .Get(x => model.Select(w => w.ProductId).Contains(x.Id)).ToList();

            products.ForEach(x => 
            {
                productsInOrder.Add(new ProductInOrder
                {
                    ProductId = x.Id,
                    Amount = model.FirstOrDefault(w => w.ProductId == x.Id).Amount,
                });
            });

            var order = new Domain.Entities.Product.Order()
            {
                UserId = _userId.Value,
                CreatedDate = DateTime.UtcNow,
                Status = OrderStatus.Created,
                Comment = "",
                Products = productsInOrder,
            };

            _unitOfWork.Repository<Domain.Entities.Product.Order>().Insert(order);
            _unitOfWork.SaveChanges();

            var response = _mapper.Map<OrderResponseModel>(order);
            return response;
        }

        public async Task<OrderResponseModel> GetById(int id)
        {
            var order = _unitOfWork.Repository<Domain.Entities.Product.Order>().Get(x => x.Id == id).FirstOrDefault();

            if (order == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "invalid orderId", "order does not exist or invalid orderId");

            var response = _mapper.Map<OrderResponseModel>(order);
            return response;
        }

        public async Task<string> Payment(int orderId)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.Id == _userId.Value)
                                                                .Include(w => w.Orders)
                                                                    .ThenInclude(w => w.Products)
                                                                    .ThenInclude(w => w.Product)
                                                                .FirstOrDefault();

            if (user == null)
                throw new CustomException(System.Net.HttpStatusCode.Unauthorized, "user", "user session had been expired");

            var order = user.Orders.FirstOrDefault(x => x.Id == orderId);

            if (order == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "orderId", "order does not exist or invalid orderId");


            var lineItems = new List<SessionLineItemOptions>();
            foreach (var item in order.Products)
            {
                lineItems.Add(new SessionLineItemOptions() 
                {
                    PriceData = new SessionLineItemPriceDataOptions() 
                    {
                        UnitAmountDecimal = (decimal?)(item.Product.Price * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions() 
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        }
                    },
                    Quantity = item.Amount,
                });
            }
            var domain = "https://localhost:7206";

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "/success",
                CancelUrl = domain + "/cancel",
                CustomerEmail = order.User.Email,
                //AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return session.Url;

        }

        public async Task<string> Payment(IEnumerable<ProductInOrderRequestModel> model)
        {
            var productsInOrder = new List<ProductInOrder>();

            var products = _unitOfWork.Repository<Domain.Entities.Product.Product>()
                                        .Get(x => model.Select(w => w.ProductId).Contains(x.Id))
                                        .Include(w => w.Image)
                                        .ToList();

            products.ForEach(x =>
            {
                productsInOrder.Add(new ProductInOrder
                {
                    Product = x,
                    Amount = model.FirstOrDefault(w => w.ProductId == x.Id).Amount,
                });
            });

            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in productsInOrder)
            {
                lineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmountDecimal = (decimal?)(item.Product.Price * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                            Images = new List<string>() { new Uri(item.Product.Image.Path).AbsolutePath },
                        }
                    },
                    Quantity = item.Amount,
                });
            }
            var domain = "https://localhost:7206";

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "/success",
                CancelUrl = domain + "/cancel",
                //AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return session.Url;
        }

    }
}
