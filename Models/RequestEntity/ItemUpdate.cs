using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestEntity
{
    public class ItemUpdate: Item
    {
        public List<string?> ExistingImages { get; set; }
    }
}
