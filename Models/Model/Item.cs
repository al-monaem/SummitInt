using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Model
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength =3)]
        public required string ItemName { get; set; }

        [ValidateNever]
        public string? ItemDescription { get; set; }

        [Required]
        [StringLength(50, MinimumLength =2)]
        public required string ItemUnit { get; set; }

        [Required, NotNull]
        [StringLength(50, MinimumLength =1)]
        public required string ItemQuantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required, NotNull]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }
        public virtual List<ItemImage>? ItemImages { get; set; }
    }
}
