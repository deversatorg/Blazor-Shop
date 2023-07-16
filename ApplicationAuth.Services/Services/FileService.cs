using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.FIlesDetails;
using ApplicationAuth.Services.Interfaces;
using Braintree;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostingEnvironment _env;

        public FileService(IUnitOfWork unitOfWork,
                            IHostingEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        public async Task<FileDetails> PostFileAsync(IFormFile fileData)
        {
            var uploadResult = new FileDetails();
            var untrustedFileName = fileData.FileName;
            uploadResult.FileName = untrustedFileName;
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);

            var trustedFileNameForStorage = Path.GetRandomFileName();
            var path = Path.Combine(_env.ContentRootPath, "Resources", "ProductsPhotos", trustedFileNameForStorage);

            await using FileStream fs = new(path, FileMode.Create);
            await fileData.CopyToAsync(fs);

            uploadResult.StoredFileName = trustedFileNameForStorage;
            uploadResult.FileType = fileData.ContentType;
            uploadResult.Path = path;

            _unitOfWork.Repository<FileDetails>().Insert(uploadResult);
            _unitOfWork.SaveChanges();

            return uploadResult;
        }

        public async Task<IEnumerable<FileDetails>> PostMultiFileAsync(List<IFormFile> fileData)
        {
            var resposne = new List<FileDetails>();

            foreach (var file in fileData)
            {
                var uploadResult = new FileDetails();
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);

                var trustedFileNameForStorage = Path.GetRandomFileName();
                var path = Path.Combine(_env.ContentRootPath, "Resources", "ProductsPhotos", trustedFileNameForStorage);

                await using FileStream fs = new(path, FileMode.Create);
                await file.CopyToAsync(fs);

                uploadResult.StoredFileName = trustedFileNameForStorage;
                uploadResult.FileType = file.ContentType;
                uploadResult.Path = path;

                _unitOfWork.Repository<FileDetails>().Insert(uploadResult);

                resposne.Add(uploadResult);
            }

            _unitOfWork.SaveChanges();

            return resposne;
        }

        public async Task<byte[]> GetFileByIdAsync(int Id)
        {

            var file = _unitOfWork.Repository<FileDetails>().Get(x => x.Id == Id).FirstOrDefault();

            if (file == null)
                throw new CustomException(HttpStatusCode.BadRequest, "file id", "invalid file id or file does not exist");

            var response = File.ReadAllBytes(file.Path);

            if(response.Length == 0)
                throw new CustomException(HttpStatusCode.NoContent, "file", "file invalid or empty or deleted");

            return response;

        }

        public byte[] GetFileById(int Id)
        {

            var file = _unitOfWork.Repository<FileDetails>().Get(x => x.Id == Id).FirstOrDefault();

            if (file == null)
                throw new CustomException(HttpStatusCode.BadRequest, "file id", "invalid file id or file does not exist");

            var response = File.ReadAllBytes(file.Path);

            if (response.Length == 0)
                throw new CustomException(HttpStatusCode.NoContent, "file", "file invalid or empty or deleted");

            return response;

        }

        public async Task CopyStream(Stream stream, string downloadPath)
        {
            using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }

}
