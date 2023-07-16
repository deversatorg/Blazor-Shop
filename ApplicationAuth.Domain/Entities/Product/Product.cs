using ApplicationAuth.Domain.Entities.FIlesDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Product
{
    public class Product : IEntity<int>
    {
        #region Properties
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public double Price { get; set; }

        [DefaultValue(true)]        
        public bool InStock { get; set; }
        
        public DateTime LastUpdate { get; set; }

        #endregion

        #region Navigation Properties

        public virtual FileDetails Image { get; set; }

        #endregion
    }
}
