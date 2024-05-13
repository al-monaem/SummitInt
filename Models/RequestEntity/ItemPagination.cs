using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestEntity
{
    public class ItemPagination
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public int TotalPage { get; set; }
        public int TotalItem { get; set; }
    }
}
