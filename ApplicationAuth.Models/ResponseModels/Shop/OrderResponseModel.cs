using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.Models.ResponseModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Order
{
    public class OrderResponseModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public double TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; }
        public List<ProductInOrderResponseModel> Products { get; set; }
    }
}
