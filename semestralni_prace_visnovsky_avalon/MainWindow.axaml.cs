using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace semestralni_prace_visnovsky_avalon
{
    public partial class MainWindow : Window
    {
        private FinanceManager manager;

        public MainWindow()
        {
            InitializeComponent();
            manager = new FinanceManager();
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (manager.Accounts.Count > 0)
            {
                txtBalance.Text = manager.Accounts[0].CurrentBalance.ToString("N0") + " Kč";
            }

            if (cmbFilterCategory != null)
            {
                List<string> names = new List<string>();
                foreach (var c in manager.Categories)
                {
                    names.Add(c.Name);
                }
                cmbFilterCategory.ItemsSource = names;
            }

            if (cmbSortOrder != null && cmbSortOrder.SelectedIndex != -1)
            {
                cmbSortOrder.SelectedIndex = 0;
            }

            if (tab7Days == null || gridTransactions == null) return;

            if (tab7Days.IsChecked == true)
            {
                Load7DaysData();
            }
            else
            {
                LoadAllData();
            }

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            List<CategoryStat> stats = new List<CategoryStat>();
            decimal totalExpenses = 0;

            foreach (var t in manager.Transactions)
            {

                if (t.Amount < 0)
                {
                    decimal absAmount = Math.Abs(t.Amount);
                    totalExpenses += absAmount;

                    CategoryStat existingStat = null;
                    foreach (var s in stats)
                    {
                        if(s.Name == t.Category.Name)
                        {
                            existingStat = s;
                            break;
                        }
                    }

                    if (existingStat != null) 
                    {
                        decimal currentSum = decimal.Parse(existingStat.FormattedAmount);
                        existingStat.FormattedAmount = (currentSum + absAmount).ToString();

                    }
                    else
                    {
                        CategoryStat newStat = new CategoryStat
                        {
                            Name = t.Category.Name,
                            Color = t.Category.Color,
                            FormattedAmount = absAmount.ToString()
                        };
                        stats.Add(newStat);
                    }
                }
            }

            foreach (var s in stats)
            {
                decimal categoryTotal = decimal.Parse(s.FormattedAmount);

                if (totalExpenses > 0)
                {
                    s.Percentage = (double)(categoryTotal / totalExpenses) * 100;
                }

                s.FormattedAmount = categoryTotal.ToString("N0") + " Kč";
            }

            listStatistics.ItemsSource = stats;
        }


        private void Load7DaysData()
        {
            DateTime sevenDaysAgo = DateTime.Now.Date.AddDays(-7);
            List<Transaction> filtered = new List<Transaction>();

            foreach (var t in manager.Transactions)
            {
                if (t.DateAndTime.Date >= sevenDaysAgo)
                {
                    filtered.Add(t);
                }
            }

            filtered.Sort((x, y) => y.DateAndTime.CompareTo(x.DateAndTime));
            gridTransactions.ItemsSource = filtered;
        }

        private void LoadAllData()
        {
            List<Transaction> all = new List<Transaction>(manager.Transactions);

            all.Sort((x, y) => y.DateAndTime.CompareTo(x.DateAndTime));

            gridTransactions.ItemsSource = all;
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            List<Transaction> filteredList = new List<Transaction>();

            string selCat = cmbFilterCategory.SelectedItem as string;
            DateTime? dateFrom = dpFilterFrom.SelectedDate;
            DateTime? dateTo = dpFilterTo.SelectedDate;

            decimal minAmt;
            bool hasMin = decimal.TryParse(txtFilterMin.Text, out minAmt);
            decimal maxAmt;
            bool hasMax = decimal.TryParse(txtFilterMax.Text, out maxAmt);

            foreach (var t in manager.Transactions)
            {
                bool matches = true;
                if (selCat != null && t.Category.Name != selCat) matches = false;
                if (dateFrom != null && t.DateAndTime.Date < dateFrom.Value.Date) matches = false;
                if (dateTo != null && t.DateAndTime.Date > dateTo.Value.Date) matches = false;
                if (hasMin && t.Amount < minAmt) matches = false;
                if (hasMax && t.Amount > maxAmt) matches = false;

                if (matches) filteredList.Add(t);
            }

            if (cmbSortOrder.SelectedIndex == 0) filteredList.Sort((x, y) => y.DateAndTime.CompareTo(x.DateAndTime));
            else if (cmbSortOrder.SelectedIndex == 1) filteredList.Sort((x, y) => x.DateAndTime.CompareTo(y.DateAndTime));
            else if (cmbSortOrder.SelectedIndex == 2) filteredList.Sort((x, y) => y.Amount.CompareTo(x.Amount));
            else if (cmbSortOrder.SelectedIndex == 3) filteredList.Sort((x, y) => x.Amount.CompareTo(y.Amount));

            gridTransactions.ItemsSource = filteredList;
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterCategory.SelectedItem = null;
            dpFilterFrom.SelectedDate = null;
            dpFilterTo.SelectedDate = null;
            txtFilterMin.Text = "";
            txtFilterMax.Text = "";
            RefreshUI();
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (manager != null)
            {
                RefreshUI();
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionWindow addWindow = new AddTransactionWindow(manager);

            var result = await addWindow.ShowDialog<bool>(this);

            if (result == true)
            {
                RefreshUI();
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            manager.SaveData();
            this.Close();
        }
    }

    public class CategoryStat
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string FormattedAmount { get; set; }
        public double Percentage { get; set; } 
    }
}