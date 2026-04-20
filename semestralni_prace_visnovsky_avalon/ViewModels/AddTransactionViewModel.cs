using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Avalonia.Controls.Primitives;

namespace semestralni_prace_visnovsky_avalon.ViewModels
{
    public partial class AddTransactionViewModel : ObservableObject
    {
        /*general*/
        private FinanceManager _manager;
        private Account _activeAccount;
        private Action<bool> _closeWindowAction;

        [ObservableProperty]
        private string _amountText;

        [ObservableProperty]
        private string _categoryName;

        [ObservableProperty]
        private string _noteText;

        [ObservableProperty]
        private DateTime? _transactionDate = DateTime.Now;

        [ObservableProperty]
        private string _selectedColor;

        [ObservableProperty]
        private bool _isColorPickerVisible = true;

        public ObservableCollection<string> ExistingCategories { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> PaletteColors { get; } = new ObservableCollection<string>
        {
            "#EF4444",
            "#F97316",
            "#10B981",
            "#3B82F6",
            "#8B5CF6",
            "#6366F1",
            "#F43F5E", 
            "#84CC16" 
        };

        public AddTransactionViewModel(FinanceManager manager, Account account, Action<bool> closeAction)
        {
            _manager = manager;
            _activeAccount = account;
            _closeWindowAction = closeAction;

            foreach (var c in _manager.Categories)
            {
                ExistingCategories.Add(c.Name);
            }

            SelectedColor = PaletteColors[0];
        }

        /*function for button*/
        [RelayCommand]
        private void SetToday()
        {
            TransactionDate = DateTime.Now;
        }

        /*function for button*/
        [RelayCommand]
        private void SetYesterday()
        {
            TransactionDate = DateTime.Now.AddDays(-1);
        }

        /*function for text change, toggles the color picker visibility - hides it if the category already exists, shows it for new ones*/
        partial void OnCategoryNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                IsColorPickerVisible = true;
                return;
            }

            bool exists = false;
            foreach(var cat in ExistingCategories)
            {
                if(cat.ToLower() == value.ToLower())
                {
                    exists = true;
                    break;
                }
            }

            IsColorPickerVisible = !exists;
        }

        [RelayCommand]
        private void Cancel()
        {
            _closeWindowAction(false);
        }

        /*saves the transaction and auto creates the category if its new*/
        [RelayCommand]
        private void Save()
        {
            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                return;
            }

            string catName = string.IsNullOrEmpty(CategoryName) ? "Bez kategorie" : CategoryName;

            Category category = null;
            foreach (var c in _manager.Categories)
            {
                if (c.Name.ToLower() == catName.ToLower())
                {
                    category = c;
                    break;
                }
            }

            if (category == null)
            {
                string colorToUse = (catName == "Bez kategorie") ? "#27272A" : SelectedColor;

                category = new Category
                {
                    Id = _manager.Categories.Count + 1,
                    Name = catName,
                    Color = colorToUse
                };

                _manager.Categories.Add(category);
            }

            DateTime selectedDate = TransactionDate.HasValue ? TransactionDate.Value : DateTime.Now;

            Transaction newTransaction = new Transaction
            {
                Id = _manager.Transactions.Count + 1,
                Amount = amount,
                DateAndTime = selectedDate,
                Note = NoteText,
                Category = category,
                Account = _activeAccount
            };

            _manager.Transactions.Add(newTransaction);
            _activeAccount.CurrentBalance += amount;

            _closeWindowAction(true);
        }
    }
}
