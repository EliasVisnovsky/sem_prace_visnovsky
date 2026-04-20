using semestralni_prace_visnovsky_avalon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace semestralni_prace_visnovsky_avalon
{
    public class FinanceManager
    {
        /*general*/
        public List<Account> Accounts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Transaction> Transactions { get; set; }

        /*ObservableCollection immediately tells axaml something has been added, removed,...*/
        public ObservableCollection<RecurringPayment> RecurringPayments { get; set; } = new ObservableCollection<RecurringPayment>();

        /*paths for txt*/
        private string filePath = @"..\..\..\mojefinance_data.txt";
        private string accountsPath = @"..\..\..\mojefinance_accounts.txt";
        private string recurringPath = @"..\..\..\mojefinance_recurring.txt";
        private string categoriesPath = @"..\..\..\mojefinance_categories.txt";

        public FinanceManager()
        {
            Accounts = new List<Account>();
            Categories = new List<Category>();
            Transactions = new List<Transaction>();

            /*creates basic account for the applications first start*/
            Accounts.Add(new Account 
            { 
                Id = 1, Name = "Běžný účet", 
                CurrentBalance = 0 
            });

            /*creates basic categories for the applications first start*/
            if (!File.Exists(categoriesPath))
            {
                Categories.Add(new Category { Id = 1, Name = "Jídlo a pití", Color = "#EAB308" });
                Categories.Add(new Category { Id = 2, Name = "Bydlení", Color = "#06B6D4" });
                Categories.Add(new Category { Id = 3, Name = "Doprava", Color = "#64748B" });
                Categories.Add(new Category { Id = 4, Name = "Zábava", Color = "#EC4899" });
                Categories.Add(new Category { Id = 5, Name = "Příjem/Výplata", Color = "#14B8A6" });
            }

            LoadData();
            ProcessRecurringPayments();
        }

        /*Executes the recurring payment if the due date is reached and it's pending for this month*/
        public void ProcessRecurringPayments()
        {
            DateTime today = DateTime.Now;
            bool changed = false;

            foreach (var rp in RecurringPayments)
            {
                if (today.Day >= rp.DayOfMonth && (rp.LastProcessedMonth != today.Month || rp.LastProcessedYear != today.Year))
                {

                    Account acc = null;
                    foreach (var a in Accounts)
                    {
                        if (a.Name == rp.AccountName)
                        {
                            acc = a;
                            break; 
                        }
                    }

                    if (acc != null)
                    {
                        /*prevents crash if the month has fewer days than DayOfMonth*/
                        int safeDay = Math.Min(rp.DayOfMonth, DateTime.DaysInMonth(today.Year, today.Month));

                        Transaction t = new Transaction
                        {
                            DateAndTime = new DateTime(today.Year, today.Month, safeDay),
                            Amount = -Math.Abs(rp.Amount),
                            Note = rp.Note + " (Trvalá platba)",
                            Category = rp.Category,
                            Account = acc
                        };

                        Transactions.Add(t);
                        acc.CurrentBalance += t.Amount;

                        rp.LastProcessedMonth = today.Month;
                        rp.LastProcessedYear = today.Year;

                        changed = true;
                    }
                }
            }

            if (changed)
            {
                SaveData();
            }
        }

        /*saves data to individual files*/
        public void SaveData()
        {
            List<string> lines = new List<string>();
            foreach (Transaction trn in Transactions)
            {
                string line = $"{trn.DateAndTime};{trn.Amount};{trn.Note};{trn.Category.Name};{trn.Category.Color};{trn.Account.Name}";
                lines.Add(line);
            }
            File.WriteAllLines(filePath, lines);

            List<string> accLines = new List<string>();
            foreach (Account acc in Accounts)
            {
                accLines.Add($"{acc.Name};{acc.SavingsGoal};{acc.SavingsDeadline};{acc.ExpenseLimit};{acc.ExpenseDeadline}");
            }
            File.WriteAllLines(accountsPath, accLines);

            List<string> recLines = new List<string>();
            foreach (RecurringPayment rp in RecurringPayments)
            {
                string catName = rp.Category != null ? rp.Category.Name : "";
                recLines.Add($"{rp.Amount};{rp.DayOfMonth};{rp.LastProcessedMonth};{rp.LastProcessedYear};{rp.Note};{catName};{rp.AccountName}");
            }
            File.WriteAllLines(recurringPath, recLines);

            List<string> catLines = new List<string>();
            foreach (Category c in Categories)
            {
                catLines.Add($"{c.Name};{c.Color}");
            }
            File.WriteAllLines(categoriesPath, catLines);
        }

        /*loads data from the individual files and ensures that they are loaded properly*/
        public void LoadData()
        {
            if (File.Exists(categoriesPath))
            {
                Categories.Clear();
                string[] catLines = File.ReadAllLines(categoriesPath);
                foreach (string line in catLines)
                {
                    string[] p = line.Split(';');
                    if (p.Length >= 2)
                    {
                        Categories.Add(new Category { Id = Categories.Count + 1, Name = p[0], Color = p[1] });
                    }
                }
            }

            if (File.Exists(accountsPath))
            {
                Accounts.Clear();
                string[] accLines = File.ReadAllLines(accountsPath);
                foreach (string line in accLines)
                {
                    string[] p = line.Split(';');
                    if (p.Length >= 5)
                    {
                        decimal.TryParse(p[1], out decimal sGoal);
                        DateTime.TryParse(p[2], out DateTime sDead);
                        decimal.TryParse(p[3], out decimal eLimit);
                        DateTime.TryParse(p[4], out DateTime eDead);

                        Accounts.Add(new Account
                        {
                            Id = Accounts.Count + 1,
                            Name = p[0],
                            CurrentBalance = 0,
                            SavingsGoal = sGoal,
                            SavingsDeadline = string.IsNullOrEmpty(p[2]) ? (DateTime?)null : sDead,
                            ExpenseLimit = eLimit,
                            ExpenseDeadline = string.IsNullOrEmpty(p[4]) ? (DateTime?)null : eDead
                        });
                    }
                }
            }

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
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

                        /*recreates missing categories for the transaction: attempts to recover the original color from the file, or safely assigns a rotating default color if none exists*/
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
                        }

                        string accName = "Běžný účet";
                        if (parts.Length >= 6)
                        {
                            accName = parts[5];
                        }

                        Account transAccount = null;
                        foreach (var a in Accounts)
                        {
                            if (a.Name == accName)
                            {
                                transAccount = a;
                                break;
                            }
                        }

                        if (transAccount == null)
                        {
                            transAccount = new Account
                            {
                                Id = Accounts.Count + 1,
                                Name = accName,
                                CurrentBalance = 0,
                                SavingsGoal = 5000,
                                ExpenseLimit = 5000
                            };
                            Accounts.Add(transAccount);
                        }

                        Transaction t = new Transaction
                        {
                            DateAndTime = date,
                            Amount = amount,
                            Note = note,
                            Account = transAccount,
                            Category = searchedCategory
                        };

                        Transactions.Add(t);
                        transAccount.CurrentBalance += amount;
                    }
                }
            }

            if (File.Exists(recurringPath))
            {
                string[] recLines = File.ReadAllLines(recurringPath);
                foreach (string line in recLines)
                {
                    string[] p = line.Split(';');
                    if (p.Length >= 7) 
                    {
                        decimal.TryParse(p[0], out decimal amount);
                        int.TryParse(p[1], out int day);
                        int.TryParse(p[2], out int lastMonth);
                        int.TryParse(p[3], out int lastYear);

                        string note = p[4];
                        string catName = p[5];
                        string accName = p[6];

                        Category recCat = null;
                        foreach (var c in Categories)
                        {
                            if (c.Name == catName)
                            {
                                recCat = c;
                                break;
                            }
                        }

                        RecurringPayments.Add(new RecurringPayment
                        {
                            Id = RecurringPayments.Count + 1,
                            Amount = amount,
                            DayOfMonth = day,
                            LastProcessedMonth = lastMonth,
                            LastProcessedYear = lastYear,
                            Note = note,
                            Category = recCat,
                            AccountName = accName
                        });
                    }
                }
            }
        }
    }
}