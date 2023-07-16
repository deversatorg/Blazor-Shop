using ApplicationAuth.Domain.Entities.FIlesDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IFileService
    {
        public Task<FileDetails> PostFileAsync(IFormFile fileData);

        public Task<IEnumerable<FileDetails>> PostMultiFileAsync(List<IFormFile> fileData);

        public Task<byte[]> GetFileByIdAsync(int id);

        byte[] GetFileById(int id);
    }
}
