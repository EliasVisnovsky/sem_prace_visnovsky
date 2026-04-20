using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class Transaction : ObservableObject
{
    /*general*/
    public int Id { get; set; }
    public DateTime DateAndTime { get; set; }
    public string Note { get; set; }
    public Account Account { get; set; }
    public Category Category { get; set; }

    /*Ttransaction edit - It shouts on axaml and changes the color and other thing immediately*/
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmountColor))]
    [NotifyPropertyChangedFor(nameof(FormattedAmount))]
    [NotifyPropertyChangedFor(nameof(IsIncome))]
    private decimal _amount;

    /*changes color to red or green*/
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

    /*formats the number and adds + to the positive number*/
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