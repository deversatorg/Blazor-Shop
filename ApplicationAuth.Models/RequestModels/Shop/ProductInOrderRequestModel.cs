using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.RequestModels.Shop
{
    public class ProductInOrderRequestModel
    {
        public int ProductId { get; set; }

        public int Amount { get; set; }
    }
}
