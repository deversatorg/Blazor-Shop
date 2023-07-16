using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Product
{
    public class Order : IEntity<int>
    {
        #region Properties

        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public OrderStatus Status { get; set; }

        public double TotalPrice { get; set; }

        public string Comment { get; set; }

        #endregion

        #region Navigation Properties

        [InverseProperty("Order")]
        public virtual ICollection<ProductInOrder> Products { get; set; }

        [InverseProperty("Orders")]
        public virtual ApplicationUser User { get; set; }

        #endregion 
    }
}
