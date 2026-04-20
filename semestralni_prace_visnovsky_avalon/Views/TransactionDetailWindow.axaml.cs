using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using semestralni_prace_visnovsky_avalon.ViewModels;

namespace semestralni_prace_visnovsky_avalon.Views
{
    public partial class TransactionDetailWindow : Window
    {
        public TransactionDetailWindow()
        {
            InitializeComponent();
        }

        // builds the window UI and wires up the ViewModel with required data and a close action
        public TransactionDetailWindow(FinanceManager manager, Transaction t) : this()
        {
            this.DataContext = new TransactionDetailViewModel(manager, t, Zavrit);
        }
        private void Zavrit()
        {
            this.Close();
        }
    }
}
