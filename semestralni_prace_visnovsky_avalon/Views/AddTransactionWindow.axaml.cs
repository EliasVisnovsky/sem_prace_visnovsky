using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using semestralni_prace_visnovsky_avalon.ViewModels;
using System;
using System.Collections.Generic;

namespace semestralni_prace_visnovsky_avalon;

public partial class AddTransactionWindow : Window
{
    public AddTransactionWindow()
    {
        InitializeComponent();

    }

    // builds the window UI and wires up the ViewModel with required data and a close action
    public AddTransactionWindow(FinanceManager manager, Account currentAccount) : this()
    {
        this.DataContext = new AddTransactionViewModel(manager, currentAccount, Zavrit);
    }
    private void Zavrit(bool result)
    {
        this.Close(result);
    }
}
