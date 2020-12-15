using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace COMP2084BeerStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string SKU { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string ProductName { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")] // display in currency format
        [Range(0.01, 999999)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Alcohol %")]
        [Range(0, 100)]
        public float AlcoholContent { get; set; }

        [Required]
        [Range(1, 999999)]
        public int Volume { get; set; }
        public string Photo { get; set; }

        // parent object reference
        public Category Category { get; set; }

        // child ref
        public List<OrderDetail> OrderDetails { get; set; }
        public List<Cart> Carts { get; set; }
    }
}
