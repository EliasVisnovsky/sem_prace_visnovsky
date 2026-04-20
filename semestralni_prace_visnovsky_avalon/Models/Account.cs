using System;
using System.Collections.Generic;
using System.Text;

public class Account
{

    /*general*/
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal CurrentBalance { get; set; }

    /*saving goal bar*/
    public decimal SavingsGoal { get; set; }
    public DateTime? SavingsDeadline { get; set; }

    /*spending limit*/
    public decimal ExpenseLimit { get; set; }
    public DateTime? ExpenseDeadline { get; set; }
}
