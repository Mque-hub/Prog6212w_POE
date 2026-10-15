using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class CategoryDto
    {
        public int CategoryID {get; set;}
        public int EventID {get; set;}
        public string CategoryName {get; set;}
        public int MinAge {get; set;}
        public int MaxAge {get; set;}
    }
}
