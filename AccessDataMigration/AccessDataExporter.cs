using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.OleDb;
using ChurchData;
using ChurchData.DTOs;

public class AccessDataExporter
{
    public  int ParishId { get; set; }
    public List<TransactionHead> ExportTransactionHeads(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var transactionHeads = new List<TransactionHead>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var transactionHead = new TransactionHead
                    {
                        HeadName = reader["HeadName"].ToString(),
                        Type = reader["Typ"].ToString(),
                        IsMandatory = reader["Remarks"].ToString().Contains("Kudishika"),
                        Description = reader["Remarks"].ToString(),
                        Aramanapct = reader.IsDBNull(reader.GetOrdinal("Aramana")) || string.IsNullOrEmpty(reader["Aramana"].ToString()) ? 0 : Convert.ToDouble(reader["Aramana"]),
                        Ordr = reader["Odr"].ToString(),
                        HeadNameMl = reader["HdNameL"].ToString(),
                        Action = "INSERT",
                        ParishId = ParishId
                    };
                    transactionHeads.Add(transactionHead);
                }
            }
        }

        return transactionHeads;
    }

    public List<Unit> ExportUnits(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var units = new List<Unit>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open(); // Use synchronous open method
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader()) // Use OleDbDataReader
            {
                while (reader.Read()) // Use synchronous read method
                {
                    var unit = new Unit
                    {
                        UnitName = reader["Unit"].ToString(),
                        Description = "",
                        UnitPresident = "",
                        UnitSecretary = "",
                        ParishId = ParishId
                    };

                    units.Add(unit);
                }
            }
        }
        return units;
    }

    public List<Family> ExportFamilies(string accessDbPath, string tableName, Dictionary<string, int> unitNameLookup)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var families = new List<Family>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var words = reader["MemName"].ToString().Split();
                    var headName = words.Length > 0 ? words[0] : "head";
                    var familyName = words.Length > 1 ? words[^1] : reader["MemName"].ToString();
                    var status = string.IsNullOrEmpty(reader["stat"].ToString()) ? "Live" : reader["stat"].ToString();

                    var unitName = reader["UnitName"].ToString();
                    var unitID = unitNameLookup.ContainsKey(unitName) ? unitNameLookup[unitName] : 0;

                    var family = new Family
                    {
                        Action = "INSERT",
                        UnitId = unitID,
                        ParishId = ParishId,
                        FamilyName = familyName,
                        Address = null, // Set to null or modify as necessary
                        ContactInfo = null, // Set to null or modify as necessary
                        Category = "Low", // Default category, modify as necessary
                        FamilyNumber = Convert.ToInt32(reader["Hno"]),
                        Status = status,
                        HeadName = headName
                    };

                    families.Add(family);
                }
            }
        }

        return families;
    }

    public List<Bank> ExportBanks(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var banks = new List<Bank>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var bank = new Bank
                    {
                        BankName = reader["BankName"].ToString(),
                        AccountNumber = reader["AccountNo"].ToString(),
                        OpeningBalance = Convert.ToDecimal(reader["OB"]),
                        CurrentBalance = Convert.ToDecimal(reader["OB"]), // Assuming current balance is the same as opening balance for now
                        ParishId = 2, // Default parish ID, modify as necessary
                        Action = "INSERT"
                    };

                    banks.Add(bank);
                }
            }
        }

        return banks;
    }

    public List<Transaction> ExportTransactions(string accessDbPath, string tableName, Dictionary<string, int> headNameLookup, Dictionary<string, int> familyNameLookup, Dictionary<string, int> bankNameLookup)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var transactions = new List<Transaction>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
           // using (OleDbCommand cmd = new OleDbCommand($"SELECT TOP 10 * FROM {tableName}", conn))
            using (OleDbCommand cmd = new OleDbCommand($"SELECT {tableName}.* FROM {tableName} where CB<>\"JE\"", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var transactionType = reader["Type"].ToString();
                    transactionType = transactionType == "Receipt" ? "Income" : "Expense";

                    var transaction = new Transaction
                    {
                        Action = "INSERT",
                       // TransactionId = Convert.ToInt32(reader["ID"]),
                        TrDate = Convert.ToDateTime(reader["VRDate"]),
                        VrNo = reader["VRNo"].ToString(),
                        TransactionType = transactionType,
                        HeadId = headNameLookup.ContainsKey(reader["Head"].ToString()) ? headNameLookup[reader["Head"].ToString()] : 0,
                        FamilyId = familyNameLookup.ContainsKey(reader["Hno"].ToString()) ? familyNameLookup[reader["Hno"].ToString()] : 0,
                        BankId = bankNameLookup.ContainsKey(reader["CB"].ToString()) ? bankNameLookup[reader["CB"].ToString()] : 0,
                        IncomeAmount = Convert.ToDecimal(reader["Credit"]),
                        ExpenseAmount = Convert.ToDecimal(reader["Debit"]),
                        Description = reader["Remarks"].ToString(),
                        ParishId = 2 // Default parish ID, modify as necessary
                    };

                    transactions.Add(transaction);
                }
            }
        }

        return transactions;
    }

    public void ImportUnits(List<Unit> units, string apiUrl)
    {
        var apiService = new ApiService();

        foreach (var unit in units)
        {
            try
            {
                apiService.ImportData(unit, apiUrl);
            }
            catch (HttpRequestException ex)
            {
                // Log the detailed error message
                Console.WriteLine($"Request error for Unit: {unit.UnitName}, Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other exceptions that might occur
                Console.WriteLine($"Unexpected error for Unit: {unit.UnitName}, Error: {ex.Message}");
            }
        }
    }

}
