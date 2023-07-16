using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.FIlesDetails;
using ApplicationAuth.Domain.Entities.Product;
using ApplicationAuth.Models.RequestModels.Product;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper = null;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public ProductService(IMapper mapper, 
                              IUnitOfWork unitOfWork,
                              IFileService fileService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<ProductResponseModel> ChangeStatusById(int id)
        {
            var product = _unitOfWork.Repository<Product>().GetById(id);

            if (product == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "invalid productId", "invalid product Id");

            switch (product.InStock)
            {
                case true:
                    product.InStock = false;
                    break;
                case false:
                    product.InStock = true;
                    break;
            }
            _unitOfWork.Repository<Product>().Update(product);
            _unitOfWork.SaveChanges();

            var response = _mapper.Map<ProductResponseModel>(product);
            return response;
        }

        public async Task<ProductResponseModel> Add(ProductRequestModel model)
        {
            var product = _unitOfWork.Repository<Product>().Get(x => x.Name == model.Name)
                                                            .Include(w => w.Image)
                                                            .FirstOrDefault();

            if (product != null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "product name", "invalid product name or product with such name already exists");

            var imageDetails = await _fileService.PostFileAsync(model.Image);

            product = new Product()
            {
                InStock = true,
                Name = model.Name,
                Image = imageDetails,
                Price = model.Price,
                Description = model.Description,
                LastUpdate = DateTime.UtcNow
            };

            _unitOfWork.Repository<Product>().Insert(product);
            _unitOfWork.SaveChanges();

            var response = _mapper.Map<ProductResponseModel>(product);
            return response;
        }

        public async Task<string> DeleteById(int id)
        {
            var product = _unitOfWork.Repository<Product>().Get(x => x.Id == id)
                                                            .Include(w => w.Image)
                                                            .FirstOrDefault();

            if (product == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "id", "invalid product id");

            _unitOfWork.Repository<Product>().Delete(product);
            _unitOfWork.SaveChanges();
            return $"Id:{id}| product deleted";
        }

        public async Task<ProductResponseModel> Edit(ProductRequestModel model, int id)
        {
            var product = _unitOfWork.Repository<Product>().Get(x => x.Id == id)
                                                            .Include(w => w.Image)
                                                            .FirstOrDefault();

            if (product == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "id", "invalid product id");

            var imageDetails = await _fileService.PostFileAsync(model.Image);

            product.Name = model.Name;
            product.Image = imageDetails; 
            product.Description = model.Description;
            product.Price = model.Price;

            _unitOfWork.Repository<Product>().Update(product);
            _unitOfWork.SaveChanges();

            var response = _mapper.Map<ProductResponseModel>(product);
            return response;
        }

        public async Task<IEnumerable<SmallProductResponseModel>> GetAll()
        {
            var products = _unitOfWork.Repository<Product>().Get(x => x.Id > 0)
                                                        .Include(w => w.Image)
                                                        .ToList();

            var response = _mapper.Map<IEnumerable<SmallProductResponseModel>>(products);

            return response;
        }

        public async Task<ProductResponseModel> GetById(int id)
        {
            var product = _unitOfWork.Repository<Product>().Get(x => x.Id == id)
                                                            .Include(w => w.Image)
                                                            .FirstOrDefault();

            if (product == null)
                throw new CustomException(System.Net.HttpStatusCode.BadRequest, "id", "invalid product id");

            var response = _mapper.Map<ProductResponseModel>(product);
            return response;
        }
    }
}
