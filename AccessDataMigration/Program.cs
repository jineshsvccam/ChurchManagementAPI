using System;
using System.Configuration;
using System.Threading.Tasks;
using ChurchData;

class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine("Starting Main method...");

        try
        {
            string accessDbPath = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
            Console.WriteLine($"Access DB Path: {accessDbPath}");

            string unitApiUrl = "http://localhost:8080/api/Unit";
            string transactionHeadsUrl = "http://localhost:8080/api/TransactionHead";
            string familyApiUrl = "http://localhost:8080/api/Family";
            string bankApiGetUrl = "http://localhost:8080/api/Bank";
            string bankApiUrl = "http://localhost:8080/api/Bank/create-or-update";
            string transactionApiUrl = "http://localhost:8080/api/Transaction/create-or-update";

            Console.WriteLine("API URLs set...");

            // Boolean flags to control which actions are executed
            bool processUnits = false;
            bool processTransactionHeads = false;
            bool processFamilies = false;
            bool processBanks = false;
            bool processTransactions = true;

            Console.WriteLine("Boolean flags set...");

            var dataExporter = new AccessDataExporter();
            var apiService = new ApiService();

            dataExporter.ParishId = 2;

            Console.WriteLine("Instances created...");

            // Export and Import Units
            if (processUnits)
            {
                Console.WriteLine("Processing Units...");
                var units = dataExporter.ExportUnits(accessDbPath, "UnitL");
                dataExporter.ImportUnits(units, unitApiUrl);
            }

            // Export and Import Transaction Heads
            if (processTransactionHeads)
            {
                Console.WriteLine("Processing Transaction Heads...");
                var transactionHeads = dataExporter.ExportTransactionHeads(accessDbPath, "Heads");
                await apiService.ImportDataAsync(transactionHeads, transactionHeadsUrl);
            }

            // Export and Import Families
            if (processFamilies)
            {
                Console.WriteLine("Processing Families...");
                var unitNames = await apiService.GetUnitNamesAsync(unitApiUrl);
                var families = dataExporter.ExportFamilies(accessDbPath, "Namelist", unitNames);
                await apiService.ImportDataAsync(families, familyApiUrl);
            }

            // Export and Import Banks
            if (processBanks)
            {
                Console.WriteLine("Processing Banks...");
                var banks = dataExporter.ExportBanks(accessDbPath, "Bank");
                await apiService.ImportDataAsync(banks, bankApiUrl);
            }

            // Export and Import Transactions
            if (processTransactions)
            {
                Console.WriteLine("Processing Transactions...");
                var headNames = await apiService.GetHeadsNamesAsync(transactionHeadsUrl);
                var familyNames = await apiService.GetFamiliesAsync(familyApiUrl);
                var bankNames = await apiService.GetBanksAsync(bankApiGetUrl);

                var transactions = dataExporter.ExportTransactions(accessDbPath, "DailyData", headNames, familyNames, bankNames);
                List<int> failedTransactionIds = new List<int>();

                foreach (var transaction in transactions)
                {
                    try
                    {
                        await apiService.ImportDataAsync(new List<Transaction> { transaction }, transactionApiUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting transaction: {transaction.TransactionId}. Error: {ex.Message}");
                        failedTransactionIds.Add(transaction.TransactionId);
                    }
                }

                if (failedTransactionIds.Any())
                {
                    string selectStatement = $"SELECT * FROM DailyData WHERE ID IN ({string.Join(", ", failedTransactionIds)})";
                    Console.WriteLine($"Failed transaction IDs: {string.Join(", ", failedTransactionIds)}");
                    Console.WriteLine($"SELECT statement for failed transactions: {selectStatement}");
                }


                Console.WriteLine("Data imported successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        // Keep the console window open
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
