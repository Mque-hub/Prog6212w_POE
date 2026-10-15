using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class EnrolmentDto
    {
        public int EnrolmentId {  get; set; }
        public int AccountId { get; set; }
        public int CategoryId {  get; set; }
        public string EnrolmentType { get; set; }
    }
}
