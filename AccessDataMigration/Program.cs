using System;
using System.Configuration;
using System.Threading.Tasks;

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
            string bankApiUrl = "http://localhost:8080/api/Bank/create-or-update";

            Console.WriteLine("API URLs set...");

            // Boolean flags to control which actions are executed
            bool processUnits = false;
            bool processTransactionHeads = false;
            bool processFamilies = false;
            bool processBanks = true;

            Console.WriteLine("Boolean flags set...");

            var dataExporter = new AccessDataExporter();
            var apiService = new ApiService();

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
                var transactionHeads = dataExporter.ExportTransactionHeads(accessDbPath, "HeadMal");
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

            Console.WriteLine("Data imported successfully!");
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
