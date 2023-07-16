using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Product
{
    public class ProductResponseModel
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool InStock { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
