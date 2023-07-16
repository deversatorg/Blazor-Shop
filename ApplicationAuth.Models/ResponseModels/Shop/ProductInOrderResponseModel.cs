using ApplicationAuth.Models.ResponseModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Shop
{
    public class ProductInOrderResponseModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public SmallProductResponseModel Product { get; set; }

        public int Amount { get; set; }

    }
}
