using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia4.Models.Dto.request
{
    public class Product_Warehouse
    {
        [Required]
        public int IdProduct { get; set; }

        [Required]
        public int IdWarehouse { get; set; }

        [Required]
        [Range(0,999999,
                ErrorMessage = "Amount cannot be less than 0")]
        public int Amount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
