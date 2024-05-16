using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Model
{
    public class Category
    {
        [Key]
        [DisplayName("Category ID")]
        [Required]
        public int Id { get; set; }

        [Required, NotNull]
        [StringLength(50)]
        [DisplayName("Category Name")]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual List<Item>? Items { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }
        [ForeignKey(nameof(UpdatedBy))]
        public virtual User? UpdatedByUser { get; set; }
    }
}
