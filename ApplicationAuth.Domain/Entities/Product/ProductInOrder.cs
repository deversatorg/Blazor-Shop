using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Product
{
    public class ProductInOrder : IEntity<int>
    {
        #region Properties
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public int Amount { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Product Product { get; set; }

        [InverseProperty("Products")]
        public virtual Order Order { get; set; }

        #endregion
    }
}
