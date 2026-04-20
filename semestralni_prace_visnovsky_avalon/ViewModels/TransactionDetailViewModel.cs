using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace semestralni_prace_visnovsky_avalon.ViewModels
{
    public partial class TransactionDetailViewModel : ObservableObject
    {
        /*general*/
        private FinanceManager _manager;
        private Transaction _originalTransaction;
        private Action _closeAction;

        [ObservableProperty]
        private string _amountText;
        [ObservableProperty] 
        private string _categoryName;
        [ObservableProperty] 
        private string _noteText;

        public TransactionDetailViewModel(FinanceManager manager, Transaction transaction, Action closeAction)
        {
            _manager = manager;
            _originalTransaction = transaction;
            _closeAction = closeAction;

            AmountText = transaction.Amount.ToString();
            CategoryName = transaction.Category.Name;
            NoteText = transaction.Note;
        }

        /*rewrites the amount and note, closes the window*/
        [RelayCommand]
        public void Save()
        {
            if(!decimal.TryParse(AmountText, out decimal newAmount))
            {
                return;
            }

            _originalTransaction.Account.CurrentBalance -= _originalTransaction.Amount;
            _originalTransaction.Account.CurrentBalance += newAmount;

            _originalTransaction.Amount = newAmount;
            _originalTransaction.Note = NoteText;

            _manager.SaveData();
            _closeAction();
        }

        /*deletes the transaction and closes the window*/
        [RelayCommand]
        private void Delete()
        {
            _originalTransaction.Account.CurrentBalance -= _originalTransaction.Amount;

            _manager.Transactions.Remove(_originalTransaction);
            _manager.SaveData();

            _closeAction();
        }

        /*cancels and closes the window*/
        [RelayCommand]
        private void Cancel()
        {
            _closeAction();
        }
    }
}
