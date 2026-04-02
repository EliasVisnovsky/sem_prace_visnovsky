using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class FinanceManager
{
    public List<Account> Accounts { get; set; }
    public List<Category> Categories { get; set; }
    public List<Transaction> Transactions { get; set; }

    private string filePath = @"..\..\..\mojefinance_data.txt";

    public FinanceManager()
    {
        Accounts = new List<Account>();
        Categories = new List<Category>();
        Transactions = new List<Transaction>();


        Accounts.Add(new Account { Id = 1, Name = "Běžný účet", CurrentBalance = 0 });

        LoadData();

    }

    public void SaveData()
    {
        List<string> lines = new List<string>();

        foreach (Transaction trn in Transactions)
        {
            string line = $"{trn.DateAndTime};{trn.Amount};{trn.Note};{trn.Category.Name};{trn.Category.Color}";
            lines.Add(line);

        }

        File.WriteAllLines(filePath, lines);
    }

    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            Account myAccount = Accounts[0];

            string[] selectedColors = { "#EF4444", "#3B82F6", "#10B981", "#F59E0B", "#8B5CF6", "#EC4899", "#06B6D4" };

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                if (parts.Length >= 4)
                {
                    DateTime date = DateTime.Parse(parts[0]);
                    decimal amount = decimal.Parse(parts[1]);
                    string note = parts[2];
                    string categoryName = parts[3];

                    Category searchedCategory = null;

                    foreach (var c in Categories)
                    {
                        if (c.Name.ToLower() == categoryName.ToLower())
                        {
                            searchedCategory = c;
                            break;
                        }
                    }

                    if (searchedCategory == null)
                    {
                        string categoryColor;

                        if (parts.Length >= 5)
                        {
                            categoryColor = parts[4];
                        }
                        else
                        {
                            int colorIndex = Categories.Count;

                            while (colorIndex >= selectedColors.Length)
                            {
                                colorIndex = colorIndex - selectedColors.Length;
                            }

                            categoryColor = selectedColors[colorIndex];
                        }

                        searchedCategory = new Category 
                        { 
                            Id = Categories.Count + 1,
                            Name = categoryName, 
                            Color = categoryColor 
                        };

                        Categories.Add(searchedCategory);
                    }

                  

                    Transaction t = new Transaction
                    {
                        DateAndTime = date,
                        Amount = amount,
                        Note = note,
                        Account = myAccount,
                        Category = searchedCategory
                    };

                    Transactions.Add(t);
                    myAccount.CurrentBalance += amount;
                }
            }
        }
    }

}
