using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class Currency
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Symbol { get; set; }
        public decimal ConversionRateToUSD { get; set; }
        public bool IsDefaultCurrency { get; set; }
    }
}
