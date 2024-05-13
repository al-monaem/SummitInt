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
        [StringLength(20)]
        [DisplayName("Category Name")]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //public int CreatedBy { get; set; }
        //public int UpdatedBy { get; set; }

        public virtual List<Item> Items { get; set; }
    }
}
