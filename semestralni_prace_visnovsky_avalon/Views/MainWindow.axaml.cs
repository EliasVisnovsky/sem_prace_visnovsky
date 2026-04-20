using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using semestralni_prace_visnovsky_avalon.Views;
using semestralni_prace_visnovsky_avalon.ViewModels;

namespace semestralni_prace_visnovsky_avalon
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel();

            this.DataContext = _vm;

            _vm.RequestOpenDetail = OtevriDetailOkna;

        }
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = _vm.GetSelectedAccount();

            var addWindow = new AddTransactionWindow(_vm.Manager, selectedAccount);
            var result = await addWindow.ShowDialog<bool>(this);

            if(result == true)
            {
                _vm.UpdateEverything();
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            _vm.Manager.SaveData();
            this.Close();
        }

        private async void OtevriDetailOkna(Transaction transaction)
        {
            var detailWindow = new TransactionDetailWindow(_vm.Manager, transaction);

            await detailWindow.ShowDialog(this);

            _vm.UpdateEverything();
        }
    }

}