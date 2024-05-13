using Models.RequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ResponseEntity
{
    public class DataResponse<T>
    {
        public List<T> List { get; set; }
        public ItemPagination Pagination { get; set; }
    }
}
