using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace semestralni_prace_visnovsky_avalon.Models
{
    public class CategoryStat
    {
        /*for the mainview progress bar*/
        /*general*/
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal ValueAmount { get; set; }
        public string FormattedAmount { get; set; }
        public double Percentage { get; set; }
    }
}
