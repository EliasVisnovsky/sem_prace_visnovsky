using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace semestralni_prace_visnovsky_avalon.Models
{
    public partial class RecurringPayment : ObservableObject
    {
        /*general*/
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; }
        public int LastProcessedMonth { get; set; }
        public int LastProcessedYear { get; set; }
        public string Note { get; set; }
        public Category Category { get; set; }
        public string AccountName { get; set; }
    }
}
