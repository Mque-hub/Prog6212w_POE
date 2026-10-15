using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class CategoryRequest
    {
        public int EventID { get; set; }
        public string CategoryName { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
    }
}
