using ApplicationAuth.Models.RequestModels.Product;
using ApplicationAuth.Models.ResponseModels.Product;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseModel> Add(ProductRequestModel model);
        Task<IEnumerable<SmallProductResponseModel>> GetAll();
        Task<ProductResponseModel> GetById(int id);
        Task<ProductResponseModel> Edit(ProductRequestModel model, int id);
        Task<ProductResponseModel> ChangeStatusById(int id);
        Task<string> DeleteById(int id);
    }
}
