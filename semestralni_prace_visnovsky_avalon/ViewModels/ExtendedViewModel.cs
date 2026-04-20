using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using semestralni_prace_visnovsky_avalon.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semestralni_prace_visnovsky_avalon.ViewModels
{
    public partial class ExtendedViewModel : ObservableObject
    {

        /*general*/
        private FinanceManager _manager;

        [ObservableProperty] private string _currentAccountName = "";

        /*saving general*/
        [ObservableProperty] private string _savingsGoalText = "5000";
        [ObservableProperty] private decimal _currentSaved = 0;
        [ObservableProperty] private double _savingsProgress = 0;
        [ObservableProperty] private string _savingsMessage = "";
        [ObservableProperty] private string _savingsColor = "#EAB308";
        [ObservableProperty] private DateTime? _savingsDeadline = DateTime.Now.AddMonths(1);

        /*expense limit general*/
        [ObservableProperty] private string _expenseLimitText = "5000";
        [ObservableProperty] private decimal _currentSpent = 0;
        [ObservableProperty] private double _expenseProgress = 0;
        [ObservableProperty] private string _expenseMessage = "";
        [ObservableProperty] private string _expenseColor = "#10B981";
        [ObservableProperty] private DateTime? _expenseDeadline = DateTime.Now.AddMonths(1);

        /*recurring general*/
        [ObservableProperty] private string _recAmountText = "";
        [ObservableProperty] private string _recNote = "";
        [ObservableProperty] private Category? _selectedRecCategory;
        [ObservableProperty] private DateTime? _recDate = DateTime.Now;
        public ObservableCollection<Category> AllCategories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<RecurringPayment> CurrentRecurringPayments { get; } = new ObservableCollection<RecurringPayment>();

        /*piechart general*/
        [ObservableProperty] private DateTime? _pieDateFrom = DateTime.Now.Date.AddDays(-14);
        [ObservableProperty] private DateTime? _pieDateTo = DateTime.Now.Date;

        /*data source for the pie chart, auto-updates the UI when modified*/
        public ObservableCollection<ISeries> PieSeries { get; set; } = new ObservableCollection<ISeries>();

        partial void OnPieDateFromChanged(DateTime? value)
        {
            CalculatePieChart();
        }

        partial void OnPieDateToChanged(DateTime? value)
        {
            CalculatePieChart();
        }

        public ExtendedViewModel(FinanceManager manager)
        {
            _manager = manager;
            CalculateEverything();
        }

        /*Automatically triggered, updates everything*/
        partial void OnCurrentAccountNameChanged(string value)
        {
            LoadCategories();
            LoadLimitsFromAccount(); 
            CalculateSavings();
            CalculateExpenses();
            UpdateRecurringList();
            CalculatePieChart();   
        }


        private bool _isUpdatingFromCode = false;

        /*loads custom savings and expense limits for the active account, applies default values if no customizations exist*/
        private void LoadLimitsFromAccount()
        {
            Account acc = null;
            foreach (var a in _manager.Accounts)
            {
                if (a.Name.ToLower() == CurrentAccountName.ToLower())
                {
                    acc = a;
                    break; 
                }
            }

            if (acc != null)
            {
                _isUpdatingFromCode = true;

                SavingsGoalText = acc.SavingsGoal > 0 ? acc.SavingsGoal.ToString() : "5000";
                SavingsDeadline = acc.SavingsDeadline != null ? acc.SavingsDeadline : DateTime.Now.AddMonths(1);
                ExpenseLimitText = acc.ExpenseLimit > 0 ? acc.ExpenseLimit.ToString() : "5000";
                ExpenseDeadline = acc.ExpenseDeadline != null ? acc.ExpenseDeadline : DateTime.Now.AddMonths(1);

                _isUpdatingFromCode = false;
            }
        }

        /*Automatically triggered when the user changes the savings goal, updates and saves the new limit to the active account, then recalculates savings progress, ignored if updated by me*/
        partial void OnSavingsGoalTextChanged(string value)
        {
            if (!_isUpdatingFromCode)
            {

                Account acc = null;
                foreach (var a in _manager.Accounts)
                {
                    if (a.Name.ToLower() == CurrentAccountName.ToLower())
                    {
                        acc = a;
                        break;
                    }
                }

                if (acc != null && decimal.TryParse(value, out decimal val))
                {
                    acc.SavingsGoal = val;
                    _manager.SaveData();
                }
            }
            CalculateSavings();
        }

        /*automatically triggered when the user changes the savings deadline, updates and saves the new date to the active account, then recalculates the progress. Ignored if updated by me*/
        partial void OnSavingsDeadlineChanged(DateTime? value)
        {
            if (!_isUpdatingFromCode)
            {
                Account acc = null;
                foreach (var a in _manager.Accounts)
                {
                    if (a.Name.ToLower() == CurrentAccountName.ToLower())
                    {
                        acc = a;
                        break;
                    }
                }

                if (acc != null) 
                { 
                    acc.SavingsDeadline = value; 
                    _manager.SaveData(); 
                }
            }

            CalculateSavings();
        }

        /*automatically triggered when the user changes the expense limit, updates and saves the new limit to the active account, then recalculates the expense progress, ignored if updated by me.*/
        partial void OnExpenseLimitTextChanged(string value)
        {
            if (!_isUpdatingFromCode)
            {
                Account acc = null;
                foreach (var a in _manager.Accounts)
                {
                    if (a.Name.ToLower() == CurrentAccountName.ToLower())
                    {
                        acc = a;
                        break;
                    }
                }

                if (acc != null && decimal.TryParse(value, out decimal val))
                {
                    acc.ExpenseLimit = val;
                    _manager.SaveData();
                }
            }
            CalculateExpenses();
        }

        /*automatically triggered when the user changes the expense deadline, updates and saves the new deadline to the active account, then recalculates the expense progress, ignored if updated by me.*/
        partial void OnExpenseDeadlineChanged(DateTime? value)
        {
            if (!_isUpdatingFromCode)
            {
                Account acc = null;
                foreach (var a in _manager.Accounts)
                {
                    if (a.Name.ToLower() == CurrentAccountName.ToLower())
                    {
                        acc = a;
                        break;
                    }
                }

                if (acc != null) 
                { 
                    acc.ExpenseDeadline = value; 
                    _manager.SaveData(); 
                }
            }
            CalculateExpenses();
        }

        /*updates everything*/
        private void CalculateEverything()
        {
            LoadCategories();
            LoadLimitsFromAccount();
            CalculateSavings();
            CalculateExpenses();
            UpdateRecurringList();
            CalculatePieChart();
        }

        /*starts counting from beginning of the current month, calculates the current months savings by subtracting expenses from income, updates the UI progress bar and status messages and color indicators based on the users goal and deadline*/
        public void CalculateSavings()
        {
            DateTime zacatekMesice = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            decimal prijmy = 0;
            decimal vydaje = 0;

            foreach (var t in _manager.Transactions)
            {
                if (t.Account.Name == CurrentAccountName && t.DateAndTime >= zacatekMesice)
                {
                    if (t.Amount > 0)
                    {
                        prijmy += t.Amount;
                    }
                    else
                    {
                        vydaje += Math.Abs(t.Amount);
                    }
                }
            }

            CurrentSaved = prijmy - vydaje;
            if (CurrentSaved < 0)
            {
                CurrentSaved = 0;
            }

            decimal.TryParse(SavingsGoalText, out decimal actualGoal);

            if (CurrentSaved > 0 && actualGoal > 0)
            {
                SavingsProgress = (double)(CurrentSaved / actualGoal) * 100;

                if (SavingsProgress > 100)
                {
                    SavingsProgress = 100;
                }
            }

            if (SavingsDeadline.HasValue)
            {
                var dnes = DateTime.Now.Date;
                var deadline = SavingsDeadline.Value.Date;

                if (dnes > deadline)
                {
                    if (actualGoal > 0 && CurrentSaved >= actualGoal)
                    {
                        SavingsMessage = "Cíl byl splněn včas!";
                        SavingsColor = "#10B981";
                    }
                    else
                    {
                        SavingsMessage = "Bohužel, termín vypršel bez splnění.";
                        SavingsColor = "#EF4444";
                    }
                }
                else
                {
                    int zbyvaDni = (deadline - dnes).Days;
                    if (actualGoal > 0 && CurrentSaved >= actualGoal)
                    {
                        SavingsMessage = $"Splněno! (Zbývá {zbyvaDni} dní termínu)";
                        SavingsColor = "#10B981";
                    }
                    else
                    {
                        SavingsMessage = $"Zbývá ti {zbyvaDni} dní do konce.";
                        SavingsColor = SavingsProgress > 50 ? "#EAB308" : "#F59E0B";
                    }
                }
            }
        }

        /*calculates total expenses for the current calendar month and compares them to the set limit, updates the UI progress bar and deadline messages and warning colors*/
        public void CalculateExpenses()
        {
            DateTime zacatekMesice = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            decimal vydaje = 0;

            foreach (var t in _manager.Transactions)
            {
                if (t.Account.Name == CurrentAccountName && t.DateAndTime >= zacatekMesice && t.Amount < 0)
                {
                    vydaje += Math.Abs(t.Amount);
                }
            }

            CurrentSpent = vydaje;
            decimal.TryParse(ExpenseLimitText, out decimal actualLimit);

            if (actualLimit > 0)
            {
                ExpenseProgress = (double)(CurrentSpent / actualLimit) * 100;

                if (ExpenseProgress > 100)
                {
                    ExpenseProgress = 100;
                }
            }
            else
            {
                ExpenseProgress = 0;
            }

            if (ExpenseDeadline.HasValue)
            {
                var dnes = DateTime.Now.Date;
                var deadline = ExpenseDeadline.Value.Date;

                if (CurrentSpent > actualLimit && actualLimit > 0)
                {
                    ExpenseMessage = "Limit překročen.";
                    ExpenseColor = "#EF4444";
                }
                else if (dnes > deadline)
                {
                    ExpenseMessage = "Termín vypršel a limit byl dodržen!";
                    ExpenseColor = "#10B981";
                }
                else
                {
                    int zbyvaDni = (deadline - dnes).Days;
                    if (ExpenseProgress >= 80)
                    {
                        ExpenseMessage = $"Pozor, blížíš se k limitu! (Zbývá {zbyvaDni} dní).";
                        ExpenseColor = "#F59E0B";
                    }
                    else
                    {
                        ExpenseMessage = $"Zatím v normě. (Zbývá {zbyvaDni} dní).";
                        ExpenseColor = "#10B981";
                    }
                }
            }
        }

        /*exports the transactions of the currently selected account to a CSV file, prompts the user with a save file dialog and processes the file writing on a background thread to keep the UI responsive*/
        [RelayCommand]
        public async void ExportCsv(Avalonia.Controls.Window? parentWindow)
        {
            if (parentWindow == null)
            {
                return;
            }

            var storage = parentWindow.StorageProvider;
            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Uložit export transakcí",
                SuggestedFileName = "finance_export.csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV soubory")
                    {
                        Patterns = new[] { "*.csv" }
                    }
                }
            });

            if (file != null)
            {
                var sb = new StringBuilder();

                sb.AppendLine("Datum;Kategorie;Částka;Poznámka;Účet");

                foreach (var t in _manager.Transactions)
                {
                    if (t.Account.Name == CurrentAccountName)
                    {
                        sb.AppendLine($"{t.DateAndTime:dd.MM.yyyy};{t.Category.Name};{t.Amount};{t.Note};{t.Account.Name}");
                    }
                }

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(sb.ToString());
            }
        }

        /*clears the old list and shows only the recurring payments for the current account*/
        public void UpdateRecurringList()
        {
            CurrentRecurringPayments.Clear();
            foreach (var rp in _manager.RecurringPayments)
            {
                if (rp.AccountName == CurrentAccountName)
                {
                    CurrentRecurringPayments.Add(rp);
                }
            }
        }

        /*validates the input, creates a new recurring payment for the active account, saves the data, and clears the input form*/
        [RelayCommand]
        private void AddRecurring()
        {
            if (decimal.TryParse(RecAmountText, out var amount) && amount != 0)
            {
                var newRec = new RecurringPayment
                {
                    Id = _manager.RecurringPayments.Count + 1,
                    Amount = amount,
                    DayOfMonth = RecDate.HasValue ? RecDate.Value.Day : 1,
                    LastProcessedMonth = 0,
                    LastProcessedYear = 0,
                    Note = RecNote,
                    Category = SelectedRecCategory,
                    AccountName = CurrentAccountName
                };

                _manager.RecurringPayments.Add(newRec);
                _manager.SaveData();
                UpdateRecurringList();

                RecAmountText = "";
                RecNote = "";
                SelectedRecCategory = null;
                RecDate = DateTime.Now;
            }
        }

        /*removes the selected payment from the list, saves data, updates the list*/
        [RelayCommand]
        private void DeleteRecurring(RecurringPayment payment)
        {
            if (payment != null)
            {
                _manager.RecurringPayments.Remove(payment);
                _manager.SaveData();
                UpdateRecurringList();
            }
        }

        /**/
        public void CalculatePieChart()
        {
            PieSeries.Clear();

            /*Dictionary has key and value*/
            var souctyKategorii = new Dictionary<Category, decimal>();

            foreach (var t in _manager.Transactions)
            {
                if (t.Account.Name != CurrentAccountName)
                {
                    continue;
                }

                if (t.Amount >= 0)
                {
                    continue;
                }

                if (PieDateFrom.HasValue)
                {
                    if (t.DateAndTime.Date < PieDateFrom.Value.Date)
                    {
                        continue;
                    }
                }

                if (PieDateTo.HasValue)
                {
                    if (t.DateAndTime.Date > PieDateTo.Value.Date)
                    {
                        continue;
                    }
                }

                if (t.Category != null)
                {
                    decimal utrata = Math.Abs(t.Amount);

                    if (souctyKategorii.ContainsKey(t.Category))
                    {
                        souctyKategorii[t.Category] += utrata;
                    }
                    else
                    {
                        souctyKategorii.Add(t.Category, utrata);
                    }
                }
            }

            foreach (var polozka in souctyKategorii)
            {
                Category kategorie = polozka.Key;
                decimal celkovaUtrata = polozka.Value;

                SKColor barvaGrafu = SKColors.DimGray;
                SKColor.TryParse(kategorie.Color, out barvaGrafu);

                string textProGraf = $"{celkovaUtrata:N0} Kč";

                PieSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { (double)celkovaUtrata },
                    Name = kategorie.Name,
                    Fill = new SolidColorPaint(barvaGrafu),
                    InnerRadius = 60,
                    HoverPushout = 10,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle, /*positions the text right in the middle of each part*/
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = bodVGrafu => textProGraf
                });
            }
        }

        /*clears the list of categories, loads them back one by one*/
        public void LoadCategories()
        {
            AllCategories.Clear();
            foreach (var c in _manager.Categories)
            {
                AllCategories.Add(c);
            }
        }

        /*removes the selected category from list, saves it and removes it from the UI list*/
        [RelayCommand]
        private void DeleteCategory(Category categoryToDelete)
        {
            if (categoryToDelete != null)
            {
                _manager.Categories.Remove(categoryToDelete);
                _manager.SaveData();

                AllCategories.Remove(categoryToDelete);
            }
        }
    }
    }


