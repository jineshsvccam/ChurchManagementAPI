using System.Configuration;
using System.Diagnostics;
using ChurchCommon.Utils;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Main method...");

        try
        {

            string accessDbPath = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
            Console.WriteLine($"Access DB Path: {accessDbPath}");

            // Define base URL
             string baseUrl = "http://localhost:8080";
           // string baseUrl = "https://finchurch-dce8defbh3duere4.canadacentral-01.azurewebsites.net";
            string authurl = $"{baseUrl}/Auth/login";
            string unitApiUrl = $"{baseUrl}/api/Unit";
            string familydueUrl = $"{baseUrl}/api/FamilyDues";
            string familyContributionUrl = $"{baseUrl}/api/FamilyContribution";
            string contributionsettingurl = $"{baseUrl}/api/ContributionSettings";

            string transactionHeadsGetUrl = $"{baseUrl}/api/TransactionHead";
            string familyApiGetUrl = $"{baseUrl}/api/Family";
            string bankApiGetUrl = $"{baseUrl}/api/Bank";

            string transactionHeadsUrl = $"{baseUrl}/api/TransactionHead/create-or-update";
            string familyApiUrl = $"{baseUrl}/api/Family/create-or-update";
            string bankApiUrl = $"{baseUrl}/api/Bank/create-or-update";
            string transactionApiUrlbulk = $"{baseUrl}/api/Transaction/create-or-update";

            string transactionApiUrl = $"{baseUrl}/api/Transaction";
          

            Console.WriteLine("API URLs set...");

            // Boolean flags to control which actions are executed
            bool processUnits = false;
            bool processTransactionHeads = false;
            bool processFamilies = false;
            bool processBanks = false;
            bool processTransactions = false;
            bool processFamilyDue = false;
            bool processFamilyContribution = true;
            bool processContributionSetting = false;

            Console.WriteLine("Boolean flags set...");

            var dataExporter = new AccessDataExporter();

            // *** NEW: Create and configure HttpClient with BaseAddress ***
            HttpClient httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            // *** NEW: Create instance of ApiService with the preconfigured HttpClient ***
            var apiService = new ApiService(httpClient);

            // *** NEW: Call the auth endpoint to retrieve the token and set the Authorization header ***
            string username = "jiness";
            string password = "January@23";
            AESEncryptionHelper aESEncryptionHelper = new AESEncryptionHelper(null);
            //password = aESEncryptionHelper.EncryptString(password, "my32byteSecretKey1234567890abcd");
            //username = aESEncryptionHelper.EncryptString(username, "my32byteSecretKey1234567890abcd");

            await apiService.AuthenticateAsync(authurl, username, password);

            dataExporter.ParishId = 31;

            Console.WriteLine("Instances created...");

            // Export and Import Units
            if (processUnits)
            {
                Console.WriteLine("Processing Units...");
                var units = dataExporter.ExportUnits(accessDbPath, "UnitL");
                await apiService.ImportItemsOnebyOne(units, unitApiUrl);
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
            if (processFamilyDue)
            {
                Console.WriteLine("Processing FamilyDue...");
                var headNames = await apiService.GetHeadsNamesAsync(transactionHeadsGetUrl);
                var familyNames = await apiService.GetFamiliesAsync(familyApiGetUrl);
                var familyDues = dataExporter.ExportFamilyDues(accessDbPath, "openkudi", headNames, familyNames);

                await apiService.ImportItemsOnebyOne(familyDues, familydueUrl);
            }
            if (processContributionSetting)
            {
                Console.WriteLine("Processing ContributionSettings...");
                var headNames = await apiService.GetHeadsNamesAsync(transactionHeadsGetUrl);

                var contributionsettings = dataExporter.ExportContributionsSettings(accessDbPath, "kudishika", headNames);

                await apiService.ImportItemsOnebyOne(contributionsettings, contributionsettingurl);
            }

            // Export and Import Transactions
            if (processTransactions)
            {
                Console.WriteLine("Processing Transactions...");

                // Start the stopwatch
                var stopwatch = Stopwatch.StartNew();
                bool isbulkinsertrequired = false;

                var headNames = await apiService.GetHeadsNamesAsync(transactionHeadsGetUrl);
                var familyNames = await apiService.GetFamiliesAsync(familyApiGetUrl);
                bankApiGetUrl= $"{baseUrl}/api/Bank?parishId={dataExporter.ParishId}";
                var bankNames = await apiService.GetBanksAsync(bankApiGetUrl);

                var transactions = dataExporter.ExportTransactions(accessDbPath, "DailyData", headNames, familyNames, bankNames);
                var failedTransactionIds = new System.Collections.Generic.List<int>();

                if (isbulkinsertrequired == false)
                {
                    foreach (var transaction in transactions)
                    {
                        try
                        {
                            await apiService.ImportData(transaction, transactionApiUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting transaction: {transaction.TransactionId}. Error: {ex.Message}");
                            failedTransactionIds.Add(transaction.TransactionId);
                        }
                    }

                    if (failedTransactionIds.Count > 0)
                    {
                        string selectStatement = $"SELECT * FROM DailyData WHERE ID IN ({string.Join(", ", failedTransactionIds)})";
                        Console.WriteLine($"Failed transaction IDs: {string.Join(", ", failedTransactionIds)}");
                        Console.WriteLine($"SELECT statement for failed transactions: {selectStatement}");
                    }
                }
                else
                {
                    var batches = dataExporter.SplitList(transactions, 250);
                    foreach (var batch in batches)
                    {
                        await apiService.ImportDataAsync(batch, transactionApiUrlbulk);
                    }
                }

                // Stop the stopwatch and print the elapsed time
                stopwatch.Stop();
                Console.WriteLine("Data imported successfully!");
                Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalSeconds} seconds.");
            }
            if (processFamilyContribution)
            {
                Console.WriteLine("Processing FamilyContribution...");

                // Start the stopwatch
                var stopwatch = Stopwatch.StartNew();
                bool isbulkinsertrequired = false;

                var headNames = await apiService.GetHeadsNamesAsync(transactionHeadsGetUrl);
                var familyNames = await apiService.GetFamiliesAsync(familyApiGetUrl);
                bankApiGetUrl = $"{baseUrl}/api/Bank?parishId={dataExporter.ParishId}";
                var bankNames = await apiService.GetBanksAsync(bankApiGetUrl);

                var transactions = dataExporter.ExportTransactionsJE(accessDbPath, "DailyData", headNames, familyNames, bankNames);
                var failedTransactionIds = new System.Collections.Generic.List<int>();

                if (isbulkinsertrequired == false)
                {
                    foreach (var transaction in transactions)
                    {
                        try
                        {
                            await apiService.ImportData(transaction, familyContributionUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting transaction: {transaction.ContributionId}. Error: {ex.Message}");
                            failedTransactionIds.Add(transaction.ContributionId);
                        }
                    }

                    if (failedTransactionIds.Count > 0)
                    {
                        string selectStatement = $"SELECT * FROM DailyData WHERE ID IN ({string.Join(", ", failedTransactionIds)})";
                        Console.WriteLine($"Failed transaction IDs: {string.Join(", ", failedTransactionIds)}");
                        Console.WriteLine($"SELECT statement for failed transactions: {selectStatement}");
                    }
                }
                else
                {
                    var batches = dataExporter.SplitList(transactions, 250);
                    foreach (var batch in batches)
                    {
                        await apiService.ImportDataAsync(batch, transactionApiUrlbulk);
                    }
                }

                // Stop the stopwatch and print the elapsed time
                stopwatch.Stop();
                Console.WriteLine("Data imported successfully!");
                Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalSeconds} seconds.");
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
