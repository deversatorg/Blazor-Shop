using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.FIlesDetails
{
    public class FileDetails : IEntity<int>
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string StoredFileName { get; set; }

        public string Path { get; set; }

        public string FileType { get; set; }

    }
}
