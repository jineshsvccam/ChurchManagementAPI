using System.Text;
using ChurchDTOs.DTOs.Entities;

namespace ChurchServices.WhatsAppBot
{
    public partial class WhatsAppBotService
    {
        // Transaction-related methods:
        // - SendFamilyDuesAsync
        // - HandleTransactionOrYearAsync

        // ...move the above methods here unchanged...

        // Helper for sending family dues
        public async Task SendFamilyDuesAsync(string userMobile)
        {
            UserInfo userInfo = await GetUserInfoAsync(userMobile);
            if (userInfo != null)
            {
                DateTime endDate = DateTime.UtcNow.Date;
                DateTime startDate = endDate.AddMonths(-1);

                var report = await _kudishikaReportRepository.GetKudishikaReportAsync(
                    (int)userInfo.ParishId, userInfo.FamilyNumber, startDate, endDate, false);

                if (report != null)
                {
                    StringBuilder reportBuilder = new StringBuilder();


                    reportBuilder.AppendLine($"📜 Kudishika Report for {report.FamilyName}({report.FamilyNumber}):\n");

                    reportBuilder.AppendLine("```");
                    decimal totalClosingBalance = 0;
                    int maxHeadLength = report.KudishikaItems.Max(i => i.HeadName.Length);

                    foreach (var item in report.KudishikaItems)
                    {
                        string headNamePadded = item.HeadName.PadRight(maxHeadLength);
                        string amountFormatted = $"₹{item.ClosingBalance:N2}";
                        reportBuilder.AppendLine($"• {headNamePadded} : {amountFormatted}");
                        totalClosingBalance += item.ClosingBalance;
                    }
                    reportBuilder.AppendLine("```");

                    reportBuilder.AppendLine($"\n*🔹 Closing Balance: ₹{totalClosingBalance:N2}*");

                    await _messageSender.SendTextMessageAsync(userMobile, reportBuilder.ToString());
                }
                else
                {
                    await _messageSender.SendTextMessageAsync(userMobile, "❌ Failed to retrieve family dues.");
                }
            }
        }
        public async Task<bool> HandleTransactionOrYearAsync(string userMobile, string receivedText)
        {
            var txnState = await _userState.GetStateAsync(userMobile);
            if (txnState == "txn")
            {
                UserInfo userInfo = await GetUserInfoAsync(userMobile);
                if (userInfo == null)
                    return false;

                var report = await _familyReportRepository.GetFamilyReportAsync((int)userInfo.ParishId, userInfo.FamilyNumber);
                if (report?.Transactions == null)
                    return false;

                if (int.TryParse(receivedText, out int transactionCount) && transactionCount >= 1 && transactionCount <= 15)
                {
                    var recentTransactions = report.Transactions
                        .OrderByDescending(t => t.TrDate)
                        .Take(transactionCount)
                        .ToList();

                    string summary = WhatsAppMessageFormatter.FormatTransactionReport($"📜 Last {transactionCount} Transactions:", recentTransactions, report.TotalPaid);
                    await _messageSender.SendTextMessageAsync(userMobile, summary);
                }
                else if (receivedText.Length == 4 && int.TryParse(receivedText, out int year))
                {
                    int currentYear = DateTime.Now.Year;
                    if (year < 2010 || year > currentYear)
                    {
                        await _messageSender.SendTextMessageAsync(userMobile, $"❌ Please enter a valid year between 2010 and {currentYear}.");
                        return true;
                    }

                    var yearlyTransactions = report.Transactions
                        .Where(t => t.TrDate.Year == year)
                        .ToList();

                    string summary = WhatsAppMessageFormatter.FormatTransactionReport($"📜 Transactions for {year}:", yearlyTransactions, report.TotalPaid);
                    await _messageSender.SendTextMessageAsync(userMobile, summary);
                }
                else
                {
                    await _messageSender.SendTextMessageAsync(userMobile, "❌ Please enter a valid transaction count (1-15) or a 4-digit year (2010-current).");
                    return true;
                }

                await _userState.ClearStateAsync(userMobile);
                return true;
            }
            return false;
        }

    }
}
