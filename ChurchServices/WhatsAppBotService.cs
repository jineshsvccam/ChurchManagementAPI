using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts.Interfaces.Services;
using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class WhatsAppBotService : IWhatsAppBotService
    {
        private readonly IKudishikaReportRepository _kudishikaReportRepository;
        private readonly IFamilyReportRepository _familyReportRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IWhatsAppMessageSender _messageSender;

        private static Dictionary<string, string> UserState = new Dictionary<string, string>();

        public WhatsAppBotService(
            IKudishikaReportRepository kudishikaReportRepository,
            IFamilyReportRepository familyReportRepository,
            IFamilyRepository familyRepository,
            IUnitRepository unitRepository,
            IFamilyMemberService familyMemberService,
            IWhatsAppMessageSender messageSender)
        {
            _kudishikaReportRepository = kudishikaReportRepository;
            _familyReportRepository = familyReportRepository;
            _familyRepository = familyRepository;
            _unitRepository = unitRepository;
            _familyMemberService = familyMemberService;
            _messageSender = messageSender;
        }

        private async Task<UserInfo> GetUserInfoAsync(string mobileNumber)
        {
            var dto = await _familyMemberService.ValidateUserWithMobile(mobileNumber);
            if (dto == null) return null;

            return new UserInfo
            {
                ParishId = (int)dto.ParishId,
                FamilyId = dto.FamilyId,
                FamilyNumber = dto.FamilyNumber,
                FamilyName = dto.FamilyName,
                FirstName = dto.FirstName
            };
        }

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
        public async Task SendUnitSelectionAsync(string userMobile)
        {
            await SendUnitSelectionAsync(userMobile, 1);
        }
        // Helper for sending unit selection with pagination
        public async Task SendUnitSelectionAsync(string userMobile, int page)
        {
            var userInfo = await GetUserInfoAsync(userMobile);
            if (userInfo == null) return;

            var units = (await _unitRepository.GetAllAsync(userInfo.ParishId)).ToList();
            int pageSize = 9;
            var pagedUnits = units.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var rows = pagedUnits
                .Select(u => (id: $"unit_{u.UnitId}", title: u.UnitName))
                .ToList();

            if (units.Count > page * pageSize)
            {
                rows.Add((id: "unit_next_page", title: "Next Page"));
            }

            string sectionTitle = $"Units {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedUnits.Count)}";
            await _messageSender.SendListMessageAsync(
                userMobile,
                "Select a Unit",
                "Please choose a unit from the list below:",
                "Select Unit",
                rows,
                sectionTitle
            );

            UserState[userMobile] = $"unit_page_{page}";
        }
        public string FormatTransactionReport(string title, List<FinancialReportCustomDTO> transactions, decimal totalPaid, TransactionReportStyle style = TransactionReportStyle.CompactBlock)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{title}\n");

            switch (style)
            {
                case TransactionReportStyle.Default:
                    // Table Style
                    sb.AppendLine("```");
                    sb.AppendLine($"{title}\n");
                    sb.AppendLine("Date       | Ref No  | Head         | Amount ");
                    sb.AppendLine("-----------|---------|--------------|--------");

                    foreach (var t in transactions)
                    {
                        string date = t.TrDate.ToString("yyyy-MM-dd").PadRight(10);
                        string refNo = t.VrNo.Length > 8 ? t.VrNo.Substring(0, 8) : t.VrNo.PadRight(8);
                        string head = t.HeadName.Length > 13 ? t.HeadName.Substring(0, 13) : t.HeadName.PadRight(13);
                        string amount = $"₹{t.IncomeAmount:N2}".PadLeft(8);

                        sb.AppendLine($"{date} | {refNo} | {head} | {amount}");
                    }

                    sb.AppendLine("```");
                    break;

                case TransactionReportStyle.CompactBlock:
                    // Block Style with Emojis
                    sb.AppendLine($"{title}\n");

                    sb.AppendLine("```"); // Start monospaced block

                    int i = 1;
                    foreach (var t in transactions)
                    {
                        //sb.AppendLine($"{i}) 📅 Date: {t.TrDate:yyyy-MM-dd}");
                        //sb.AppendLine($"   🧾 Ref: {t.VrNo}");
                        //sb.AppendLine($"   🗂️ Head: {t.HeadName}");
                        //sb.AppendLine($"   💸 Amount: ₹{t.IncomeAmount:N2}\n");
                        //i++;

                        string date = t.TrDate.ToString("yyyy-MM-dd").PadRight(11);     // 11 for date + spacing
                        string head = t.HeadName.Length > 12 ? t.HeadName[..12] : t.HeadName.PadRight(12);

                        string refNo = t.VrNo.Length > 10 ? t.VrNo[..10] : t.VrNo.PadRight(10);
                        string amount = $"₹{t.IncomeAmount:N2}".PadRight(12);             // Right-align amount

                        sb.AppendLine($"• {date}| {head}");
                        sb.AppendLine($"  {refNo}| {amount}\n");

                        i++;
                    }
                    sb.AppendLine("```"); // End monospaced block
                    break;
            }

            sb.AppendLine($"\n💰 *Total Paid:* ₹{totalPaid:N2}\n");
            sb.AppendLine("📌↩ *Type back to return to the previous menu.*");

            return sb.ToString();
        }

        public async Task<bool> HandleTransactionOrYearAsync(string userMobile, string receivedText)
        {
            if (UserState.TryGetValue(userMobile, out string txnState) && txnState == "txn")
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

                    string summary = FormatTransactionReport($"📜 Last {transactionCount} Transactions:", recentTransactions, report.TotalPaid);
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

                    string summary = FormatTransactionReport($"📜 Transactions for {year}:", yearlyTransactions, report.TotalPaid);
                    await _messageSender.SendTextMessageAsync(userMobile, summary);
                }
                else
                {
                    await _messageSender.SendTextMessageAsync(userMobile, "❌ Please enter a valid transaction count (1-15) or a 4-digit year (2010-current).");
                    return true;
                }

                UserState.Remove(userMobile);
                return true;
            }
            return false;
        }



        public Task SendFamilyConfirmationAsync(string userMobile, int familyNumber) => Task.CompletedTask;
        public Task<bool> HandleMemberDetailRequestAsync(string userMobile, string receivedText) => Task.FromResult(false);
        public Task SendDefaultMessageAsync(string userMobile) => Task.CompletedTask;
        public Task HandleButtonPostbackAsync(string userMobile, string buttonPayload) => Task.CompletedTask;
        public Task HandleListReplyAsync(string userMobile, string sectionId, string rowId) => Task.CompletedTask;

        public async Task HandleHiFlowAsync(string userMobile)
        {
            UserInfo userInfo = await GetUserInfoAsync(userMobile);

            if (userInfo != null)
            {
                string intro =
$@"Hey {userInfo.FirstName}! 👋 I’m ChurchMate — your friendly guide from FinChurch.
I can help you with:

• Checking your church dues
• Viewing past transactions
• Searching the member directory with easy filters

Just pick an option below, and I’ll take it from here! 😊";

                await _messageSender.SendInteractiveMessageAsync(
                    userMobile,
                    intro,
                    new Dictionary<string, string>
                    {
                        { "menu_directory", "Directory" },
                        { "menu_dues", "Family Dues" },
                        { "menu_txns", "Transactions" }
                    }
                );
                UserState.Remove(userMobile); // Reset state on fresh "hi"
            }
            else
            {
                await _messageSender.SendTextMessageAsync(userMobile, "❌ Mobile number not found in our system.");
            }
        }



        public async Task<bool> HandleButtonRepliesAsync(string userMobile, string buttonReplyId, string buttonReplyTitle)
        {
            if (buttonReplyId == null) return false;
            switch (buttonReplyId)
            {
                case "menu_directory":
                    UserState[userMobile] = "directory";
                    await _messageSender.SendInteractiveMessageAsync(
                        userMobile,
                        "📌 Choose a search type:",
                        new Dictionary<string, string>
                        {
                            { "search_name", "Name Search" },
                            { "search_family", "Family Number" },
                            { "search_unit", "Unit Search" }
                        }
                    );
                    break;
                case "search_name":
                    UserState[userMobile] = "name";
                    await _messageSender.SendTextMessageAsync(userMobile, "📌 Enter a name (at least 3 characters) to search.");
                    break;
                case "search_family":
                    UserState[userMobile] = "family";
                    await _messageSender.SendTextMessageAsync(userMobile, "📌 Enter a family number to see all members.");
                    break;
                case "search_unit":
                    UserState[userMobile] = "unit_page_1";
                    await SendUnitSelectionAsync(userMobile);
                    break;
                case "menu_dues":
                    await _messageSender.SendTextMessageAsync(userMobile, "📜 Retrieving Family Dues...");
                    await SendFamilyDuesAsync(userMobile);
                    break;
                case "menu_txns":
                    UserState[userMobile] = "txn";
                    await _messageSender.SendTextMessageAsync(userMobile,
                        "📜 Previous Transactions:\n\n" +
                        "Reply with an option (e.g., `2023` or `10`) to get transactions by year or the last N transactions.");
                    break;
                default:
                    await _messageSender.SendTextMessageAsync(userMobile, "❓ Unknown option selected.");
                    break;
            }
            return true;
        }

        public async Task<bool> HandleListRepliesAsync(string userMobile, string listReplyId, string listReplyTitle)
        {
            if (listReplyId == null) return false;

            if (listReplyId == "unit_next_page")
            {
                int page = 1;
                if (UserState.TryGetValue(userMobile, out var pageState) && pageState.StartsWith("unit_page_"))
                {
                    int.TryParse(pageState.Replace("unit_page_", ""), out page);
                }
                page++;
                UserState[userMobile] = $"unit_page_{page}";
                await SendUnitSelectionAsync(userMobile, page);
                return true;
            }
            else if (listReplyId.StartsWith("unit_"))
            {
                if (int.TryParse(listReplyId.Replace("unit_", ""), out int selectedUnitId))
                {
                    await SendFamilySelectionAsync(userMobile, selectedUnitId, 1);
                }
                return true;
            }
            else if (listReplyId == "family_next_page")
            {
                int selectedUnitId = 0, page = 1;
                if (UserState.TryGetValue(userMobile, out var famPageState) && famPageState.StartsWith("family_page_"))
                {
                    var parts = famPageState.Split("_unit_");
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[0].Replace("family_page_", ""), out page);
                        int.TryParse(parts[1], out selectedUnitId);
                    }
                }
                page++;
                UserState[userMobile] = $"family_page_{page}_unit_{selectedUnitId}";
                await SendFamilySelectionAsync(userMobile, selectedUnitId, page);
                return true;
            }
            else if (listReplyId.StartsWith("family_"))
            {
                if (int.TryParse(listReplyId.Replace("family_", ""), out int selectedFamilyNumber))
                {
                    await HandleFamilySelectionAsync(userMobile, selectedFamilyNumber);
                }
                return true;
            }
            return false;
        }

        public async Task HandleFamilySelectionAsync(string userMobile, int familyNumber)
        {
            UserInfo userInfo = await GetUserInfoAsync(userMobile);

            var familyMembers = await _familyMemberService.GetFamilyMembersFilteredAsync((int)userInfo.ParishId, familyNumber,
                new ChurchDTOs.DTOs.Entities.FamilyMemberFilterRequest
                {
                    Filters = new Dictionary<string, string> { { "ActiveMember", familyNumber.ToString() } }
                });

            if (familyMembers?.Data != null && familyMembers.Data.Any())
            {
                // Sort by Age descending (eldest first)
                var sortedMembers = familyMembers.Data
                    .Where(m => m.Age.HasValue)
                    .OrderByDescending(m => m.Age.Value)
                    .ToList();

                string familyName = sortedMembers.FirstOrDefault()?.FamilyName ?? $"#{familyNumber}";

                string responseMessage = $"🔎 Found {sortedMembers.Count()} members in *{familyName} ({familyNumber})* family:\n\n";

                int index = 1;
                foreach (var member in sortedMembers)
                {
                    string genderEmoji = member.Gender?.ToLower() switch
                    {
                        "male" => "👨",
                        "female" => "👵",
                        _ => "👤"
                    };

                    string phone = string.IsNullOrWhiteSpace(member.MobilePhone)
                        ? "Not available ❌"
                        : member.MobilePhone;

                    responseMessage += $"🔹 {index}. {member.FirstName} {genderEmoji}\n📞 {phone}\n\n";
                    index++;
                }

                await _messageSender.SendTextMessageAsync(userMobile, responseMessage);
            }
            else
            {
                await _messageSender.SendTextMessageAsync(userMobile, "❌ No records found for this family number.");
            }
        }

        public async Task SendFamilySelectionAsync(string userMobile, int selectedUnitId, int page)
        {
            var userInfo = await GetUserInfoAsync(userMobile);
            if (userInfo == null) return;

            var families = (await _familyRepository.GetFamiliesAsync(userInfo.ParishId, selectedUnitId, null)).ToList();
            int pageSize = 9;
            var pagedFamilies = families.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var rows = pagedFamilies.Select(f =>
            {
                string fullName = $"{f.HeadName} {f.FamilyName}".Trim();
                string title = fullName.Length > 24 ? fullName.Substring(0, 22) + ".." : fullName;
                return (id: $"family_{f.FamilyNumber}", title);
            }).ToList();

            if (families.Count > page * pageSize)
            {
                rows.Add((id: "family_next_page", title: "Next Page"));
            }

            string sectionTitle = $"Families {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedFamilies.Count)}";

            await _messageSender.SendListMessageAsync(
                userMobile,
                "Select a Family",
                "Please choose a family from the list below:",
                "Select Family",
                rows,
                sectionTitle
            );

            UserState[userMobile] = $"family_page_{page}_unit_{selectedUnitId}";
        }

        public async Task DirectoryBackAsync(string userMobile)
        {
            await _messageSender.SendInteractiveMessageAsync(
                userMobile,
                "📌 Choose a search type:",
                new Dictionary<string, string>
                {
                    { "search_name", "Name Search" },
                    { "search_family", "Family Number" },
                    { "search_unit", "Unit Search" }
                }
            );
            UserState[userMobile] = "directory";
        }

        // Add this overload to satisfy the interface
        public Task SendFamilySelectionAsync(string userMobile, int page)
        {
            // This method is not used in the new flow, but required by the interface.
            // You can implement a default or throw if not needed.
            throw new NotImplementedException();
        }

        public Task ParseInteractiveReplyAsync(object messageObj, Action<string, string, string, string> setValues)
        {
            // messageObj is expected to be of type Message (from controller)
            var message = messageObj as dynamic;
            string buttonReplyId = null, buttonReplyTitle = null, listReplyId = null, listReplyTitle = null;
            if (message != null && message.Type == "interactive" && message.Interactive != null)
            {
                if (message.Interactive.Type == "button_reply")
                {
                    buttonReplyId = message.Interactive.ButtonReply?.Id;
                    buttonReplyTitle = message.Interactive.ButtonReply?.Title;
                    Console.WriteLine($"🟢 Button Reply: id={buttonReplyId}, title={buttonReplyTitle}");
                }
                else if (message.Interactive.Type == "list_reply")
                {
                    listReplyId = message.Interactive.ListReply?.Id;
                    listReplyTitle = message.Interactive.ListReply?.Title;
                    Console.WriteLine($"🟢 List Reply: id={listReplyId}, title={listReplyTitle}");
                }
            }
            setValues(buttonReplyId, buttonReplyTitle, listReplyId, listReplyTitle);
            return Task.CompletedTask;
        }

        public async Task<bool> EnforceHiFirstMessageAsync(string userMobile, string receivedText, string buttonReplyId)
        {
            // ✅ Allow 'Hi' to always go through
            if (receivedText.Equals("hi", StringComparison.OrdinalIgnoreCase))
                return false;

            // ✅ If user has no known state and no button reply, block
            bool hasState = UserState.ContainsKey(userMobile)
                || (UserState.TryGetValue(userMobile, out var st) && (st == "name" || st == "family" || st == "unit" || st == "txn"));

            if (!hasState && string.IsNullOrEmpty(buttonReplyId))
            {
                await _messageSender.SendTextMessageAsync(userMobile, "👋 Please start by typing 'Hi' to proceed.");
                return true;
            }

            return false;
        }

        public async Task<bool> HandleNameSearchAsync(string userMobile, string receivedText)
        {
            if (UserState.TryGetValue(userMobile, out string searchType) && searchType == "name")
            {
                if (receivedText.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    await DirectoryBackAsync(userMobile);
                    // Do NOT remove user state here, let DirectoryBackAsync set it to "directory"
                    return true;
                }

                if (receivedText.Length >= 3)
                {
                    var userInfo = await GetUserInfoAsync(userMobile);
                    var familyMembers = await _familyMemberService.GetFamilyMembersFilteredAsync((int)userInfo.ParishId, null,
                        new FamilyMemberFilterRequest
                        {
                            Filters = new Dictionary<string, string> { { "FirstName", receivedText } }
                        });

                    if (familyMembers?.Data != null && familyMembers.Data.Any())
                    {
                        var membersWithPhone = familyMembers.Data
                            .Where(m => !string.IsNullOrWhiteSpace(m.MobilePhone))
                            .ToList();

                        var membersWithoutPhone = familyMembers.Data
                            .Where(m => string.IsNullOrWhiteSpace(m.MobilePhone))
                            .ToList();

                        string responseMessage = $"🔎 Found {familyMembers.Data.Count()} members matching '{receivedText}':\n\n";

                        int index = 1;

                        if (membersWithPhone.Any())
                        {
                            responseMessage += "📱 *Members with Contact Number:*\n\n";

                            foreach (var member in membersWithPhone)
                            {
                                var lastWord = member.FamilyName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
                                var fullName = $"{member.FirstName} {lastWord}".Trim();

                                string genderEmoji = member.Gender?.ToLower() switch
                                {
                                    "male" => "👨",
                                    "female" => "👵",
                                    _ => "👤"
                                };

                                responseMessage += $"🔸 {index}. {fullName} {genderEmoji}\n📞 {member.MobilePhone}\n\n";
                                index++;
                            }
                        }

                        if (membersWithoutPhone.Any())
                        {
                            responseMessage += "🚫 *Members without Contact Number:*\n\n";

                            foreach (var member in membersWithoutPhone)
                            {
                                var lastWord = member.FamilyName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
                                var fullName = $"{member.FirstName} {lastWord}".Trim();

                                string genderEmoji = member.Gender?.ToLower() switch
                                {
                                    "male" => "👨",
                                    "female" => "👵",
                                    _ => "👤"
                                };

                                responseMessage += $"🔸 {index}. {fullName} {genderEmoji}\n";
                                index++;
                            }
                        }

                        responseMessage += "\n\n📌↩️ Type *back* to return to the previous menu.";

                        await _messageSender.SendTextMessageAsync(userMobile, responseMessage);

                    }
                    else
                    {
                        await _messageSender.SendTextMessageAsync(userMobile, "❌ No records found for this name.");
                    }
                }
                else
                {
                    await _messageSender.SendTextMessageAsync(userMobile, "❌ Name must be at least 3 characters long. Please try again.");
                }
                UserState.Remove(userMobile);
                return true;
            }
            return false;
        }

        public async Task<bool> HandleFamilyNumberSearchAsync(string userMobile, string receivedText)
        {
            if (UserState.TryGetValue(userMobile, out string searchTypef) && searchTypef == "family")
            {
                if (receivedText.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    await DirectoryBackAsync(userMobile);
                    // Do NOT remove user state here, let DirectoryBackAsync set it to "directory"
                    return true;
                }

                if (receivedText.Length >= 1 && int.TryParse(receivedText, out int number))
                {
                    await HandleFamilySelectionAsync(userMobile, number);
                }
                else
                {
                    await _messageSender.SendTextMessageAsync(userMobile, "❌ Please enter a valid family number.");
                }
                UserState.Remove(userMobile);
                return true;
            }
            return false;
        }
    }
}
