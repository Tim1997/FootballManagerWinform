using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Models
{
    public class Receipt
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
    }
}
