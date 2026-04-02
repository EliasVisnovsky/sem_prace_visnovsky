using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace semestralni_prace_visnovsky_avalon;

public partial class AddTransactionWindow : Window
{
    private FinanceManager manager;
    public AddTransactionWindow()
    {

        InitializeComponent();

    }
    public AddTransactionWindow(FinanceManager manager) : this()

    {
        this.manager = manager;

        List<string> categoryNames = new List<string>();
        foreach (var c in manager.Categories)
        {
            categoryNames.Add(c.Name);
        }

        cmbCategory.ItemsSource = categoryNames;
        dpDate.SelectedDate = DateTime.Now;
    }

    private void BtnToday_Click(object sender, RoutedEventArgs e)
    {
        dpDate.SelectedDate = DateTime.Now;
    }

    private void BtnYesterday_Click(object sender, RoutedEventArgs e)
    {
        dpDate.SelectedDate = DateTime.Now.AddDays(-1);
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        bool isThisNumberCorrect = decimal.TryParse(txtAmount.Text, out decimal amount);

        if (isThisNumberCorrect == false)
        {
            return;
        }

        object selectedItem = cmbCategory.SelectedItem;
        string catName = selectedItem != null ? selectedItem.ToString() : cmbCategory.Text;

        if (string.IsNullOrWhiteSpace(catName))
        {
            return;
        }

        Category category = null;
        foreach (var c in manager.Categories)
        {
            if (c.Name.ToLower() == catName.ToLower())
            {
                category = c;
                break;
            }
        }

        if (category == null)
        {
            string[] categoryColor = {
            "#F59E0B",
            "#8B5CF6",
            "#EF4444", 
            "#3B82F6", 
            "#10B981",   
            "#EC4899", 
            "#06B6D4"  
        };

            int colorIndex = manager.Categories.Count % categoryColor.Length;
            string autoColor = categoryColor[colorIndex];

            category = new Category
            {
                Id = manager.Categories.Count + 1,
                Name = catName,
                Color = autoColor
            };

            manager.Categories.Add(category);
        }

        DateTime selectedDate = dpDate.SelectedDate.HasValue ? dpDate.SelectedDate.Value : DateTime.Now;

        Transaction newTransaction = new Transaction
        {
            Id = manager.Transactions.Count + 1,
            Amount = amount,
            DateAndTime = selectedDate,
            Note = txtNote.Text,
            Category = category,
            Account = manager.Accounts[0]
        };

        manager.Transactions.Add(newTransaction);
        manager.Accounts[0].CurrentBalance += amount;

        Close(true);
    }
}