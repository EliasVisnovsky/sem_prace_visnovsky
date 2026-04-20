using CommunityToolkit.Mvvm.ComponentModel; 
using CommunityToolkit.Mvvm.Input;
using semestralni_prace_visnovsky_avalon.Models;
using semestralni_prace_visnovsky_avalon.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace semestralni_prace_visnovsky_avalon
{
    public partial class MainViewModel : ObservableObject
    {
        /*general*/
        private FinanceManager _manager;

        [ObservableProperty]
        private string _balanceText = "0 Kč";

        [ObservableProperty]
        private string _selectedAccountName;

        [ObservableProperty]
        private bool _isAllTimeChecked;

        [ObservableProperty]
        private string _filterCategoryName;

        [ObservableProperty]
        private DateTime? _filterDateFrom;

        [ObservableProperty]
        private DateTime? _filterDateTo;

        [ObservableProperty]
        private string _filterMinAmountStr;

        [ObservableProperty]
        private string _filterMaxAmountStr;

        [ObservableProperty]
        private int _filterSortOrderIndex = 0;

        [ObservableProperty]
        private ExtendedViewModel _extendedVm;


        public ObservableCollection<string> AccountNames { get; } = new ObservableCollection<string>();
        public ObservableCollection<Transaction> FilteredTransactions { get; } = new ObservableCollection<Transaction>();
        public ObservableCollection<CategoryStat> CategoryStats { get; } = new ObservableCollection<CategoryStat>();
        public ObservableCollection<string> AllCategories { get; } = new ObservableCollection<string>();

        public FinanceManager Manager
        {
            get { return _manager; }
        }

        public Action<Transaction> RequestOpenDetail { get; set; }


        /*automatically triggered when the user selects a different account - refreshes balances, stats, and filters*/
        partial void OnSelectedAccountNameChanged(string value)
        {
            UpdateEverything();
        }

        /*automatically triggered when the all time filter is toggled - refreshes the transaction list to apply or remove the 7-day limit*/
        partial void OnIsAllTimeCheckedChanged(bool value)
        {
            UpdateEverything();
        }


        public MainViewModel()
        {
            _manager = new FinanceManager();

            ExtendedVm = new ExtendedViewModel(_manager);

            foreach (var acc in _manager.Accounts)
            {
                AccountNames.Add(acc.Name);
            }

            if (AccountNames.Count > 0)
            {
                SelectedAccountName = AccountNames[0];
            }

            foreach (var cat in _manager.Categories)
            {
                AllCategories.Add(cat.Name);
            }

            
        }

        /*validates the input, checks for duplicates, and creates a new account with a zero balance if the name is unique*/
        [RelayCommand]
        private void AddAccount()
        {
            if (string.IsNullOrWhiteSpace(_selectedAccountName))
            {
                return;
            }

            bool exists = false;
            foreach (var a in _manager.Accounts)
            {
                if (a.Name.ToLower() == _selectedAccountName.ToLower())
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                _manager.Accounts.Add(new Account
                {
                    Id = _manager.Accounts.Count + 1,
                    Name = SelectedAccountName,
                    CurrentBalance = 0
                });

                AccountNames.Add(SelectedAccountName);
                _manager.SaveData();
            }
        }

        /*updates the displayed balance, applies current filters, recalculates category statistics, triggers extended calculations for the selected account*/
        public void UpdateEverything()
        {
            Account foundAccount = null;
            foreach (var acc in _manager.Accounts)
            {
                if (acc.Name.ToLower() == SelectedAccountName.ToLower())
                {
                    foundAccount = acc;
                    break;
                }
            }

            if (foundAccount != null)
            {
                BalanceText = foundAccount.CurrentBalance.ToString("N0") + " Kč";
            }
            else
            {
                BalanceText = "0 Kč";
            }

            ApplyFilter();

            UpdateStatistics();

            if (ExtendedVm != null)
            {
                ExtendedVm.CurrentAccountName = SelectedAccountName;
                ExtendedVm.CalculateSavings();
                ExtendedVm.CalculateExpenses();
            }
        }

        /*selects account and if not, it selects the first one or nothing*/
        public Account GetSelectedAccount()
        {
            foreach (var acc in _manager.Accounts)
            {
                if (acc.Name == SelectedAccountName)
                {
                    return acc;
                }
            }
            return _manager.Accounts.Count > 0 ? _manager.Accounts[0] : null;
        }

        /*calculates 30-day expense statistics grouped by category and updates the UI percentages*/
        public void UpdateStatistics()
        {
            CategoryStats.Clear();
            decimal totalExpenses = 0;
            List<CategoryStat> tempStats = new List<CategoryStat>();

            DateTime posledniMesic = DateTime.Now.AddDays(-30);

            foreach (var t in _manager.Transactions)
            {

                if (t.Account.Name == SelectedAccountName && t.Amount < 0 && t.DateAndTime >= posledniMesic)
                {
                    decimal absAmount = Math.Abs(t.Amount);
                    totalExpenses += absAmount;

                    CategoryStat found = null;
                    foreach (var s in tempStats)
                    {
                        if (s.Name == t.Category.Name)
                        {
                            found = s;
                            break;
                        }
                    }

                    if (found != null)
                    {
                        found.ValueAmount += absAmount;
                    }
                    else
                    {
                        tempStats.Add(new CategoryStat
                        {
                            Name = t.Category.Name,
                            Color = t.Category.Color,
                            ValueAmount = absAmount
                        });
                    }
                }
            }

            foreach (var s in tempStats)
            {
                if (totalExpenses > 0)
                {
                    s.Percentage = (double)(s.ValueAmount / totalExpenses) * 100;
                }
                s.FormattedAmount = s.ValueAmount.ToString("N0") + " Kč";
                CategoryStats.Add(s);
            }
        }

        /*filters the transaction list based on active criteria, applies the selected sorting order, updates the UI*/
        [RelayCommand]
        private void ApplyFilter()
        {
            FilteredTransactions.Clear();
            DateTime limit7Days = DateTime.Now.Date.AddDays(-7);

            List<Transaction> tempTrans = new List<Transaction>();

            decimal? minAmount = null;
            if (decimal.TryParse(FilterMinAmountStr, out decimal min))
            {
                minAmount = min;
            }

            decimal? maxAmount = null;
            if (decimal.TryParse(FilterMaxAmountStr, out decimal max))
            {
                maxAmount = max;
            }

            foreach (var t in _manager.Transactions)
            {
                if (t.Account.Name != SelectedAccountName)
                {
                    continue;
                }

                if (!IsAllTimeChecked && t.DateAndTime.Date < limit7Days)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(FilterCategoryName))
                {
                    if(t.Category.Name.ToLower() != FilterCategoryName.ToLower())
                    {
                        continue;
                    }
                }

                if(FilterDateFrom.HasValue && t.DateAndTime.Date < FilterDateFrom.Value.Date)
                {
                    continue;
                }

                if (FilterDateTo.HasValue && t.DateAndTime.Date > FilterDateTo.Value.Date)
                {
                    continue;
                }

                decimal actualAmount = t.Amount;

                if (minAmount.HasValue && actualAmount <= minAmount.Value)
                {
                    continue;
                }

                if (maxAmount.HasValue && actualAmount >= maxAmount.Value)
                {
                    continue;
                }

                tempTrans.Add(t);
            }

            tempTrans.Sort((a, b) =>
            {
                if (FilterSortOrderIndex == 0)
                {
                    return b.DateAndTime.CompareTo(a.DateAndTime);
                }

                if (FilterSortOrderIndex == 1)
                {
                    return a.DateAndTime.CompareTo(b.DateAndTime);
                }

                if (FilterSortOrderIndex == 2)
                {
                    return Math.Abs(b.Amount).CompareTo(Math.Abs(a.Amount));
                }

                if (FilterSortOrderIndex == 3)
                {
                    return Math.Abs(a.Amount).CompareTo(Math.Abs(b.Amount));
                }

                return 0;
            });

            foreach(var t in tempTrans)
            {
                FilteredTransactions.Add(t);
            }
        }

        /*resets all of the filters*/
        [RelayCommand]
        private void ResetFilter()
        {
            FilterCategoryName = null;
            FilterDateFrom = null;
            FilterDateTo = null;
            FilterMinAmountStr = string.Empty;
            FilterMaxAmountStr = string.Empty;
            FilterSortOrderIndex = 0;

            UpdateEverything();
        }

        /*Opens the detail window*/
        [RelayCommand]
        private void OpenDetail(Transaction clickedTransaction)
        {
            if(clickedTransaction != null)
            {
                RequestOpenDetail.Invoke(clickedTransaction);
            }
        }

        /*finds the account to delete and removes it from list*/
        [RelayCommand]
        private void DeleteAccount()
        {
            if (string.IsNullOrEmpty(SelectedAccountName))
            {
                return;
            }

            Account accToRemove = null;
            foreach (var a in _manager.Accounts)
            {
                if (a.Name.ToLower() == SelectedAccountName.ToLower())
                {
                    accToRemove = a;
                    break; 
                }
            }

            if (accToRemove != null)
            {
                _manager.Accounts.Remove(accToRemove);
            }

            AccountNames.Remove(SelectedAccountName);
            _manager.SaveData();

            SelectedAccountName = AccountNames.Count > 0 ? AccountNames[0] : "";
        }
    }
}
