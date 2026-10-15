using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class EnrolmentRequest
    {
        public int CategoryId { get; set; }
        public string EnrolmentType { get; set; }
    }
}
