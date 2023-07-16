using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.RequestModels.Product
{
    public class ProductRequestModel
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }

    }
}
