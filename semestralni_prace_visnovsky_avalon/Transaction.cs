using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

public class Transaction
{
    public int Id { get; set; }
    public DateTime DateAndTime { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; }

    public Account Account { get; set; }
    public Category Category { get; set; }

    public string AmountColor
    {
        get
        {
            if (Amount >= 0)
            {
                return "#10B981";
            }
            else
            {
                return "#EF4444";
            }
        }
    }

    public string FormattedAmount
    {
        get
        {
            if (Amount >= 0)
            {
                return "+" + Amount.ToString("N0") + " Kč";
            }
            else
            {
                return Amount.ToString("N0") + " Kč";
            }
        }
    }


    public bool IsIncome 
    { 
        get 
        { 
            return Amount >= 0; 
        } 
    }

    public string MonthGroup 
    { 
        get 
        { 
            return DateAndTime.ToString("MMMM yyyy"); 
        } 
    }
}