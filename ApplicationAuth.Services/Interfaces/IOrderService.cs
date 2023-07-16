using ApplicationAuth.Models.RequestModels.Shop;
using ApplicationAuth.Models.ResponseModels.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseModel> Create(IEnumerable<ProductInOrderRequestModel> model);

        Task<OrderResponseModel> GetById(int id);

        Task<string> Payment(int orderId);

        //Unauthtorized payment
        Task<string> Payment(IEnumerable<ProductInOrderRequestModel> model);
    }
}
