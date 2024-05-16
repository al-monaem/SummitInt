using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ResponseEntity
{
    public class ItemDetails
    {
        public Item? Item { get; set; }
        public List<Item>? SimilarItems { get; set; }
    }
}
