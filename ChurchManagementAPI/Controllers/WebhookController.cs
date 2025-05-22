using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;

namespace ChurchManagementAPI.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {

        private readonly IKudishikaReportRepository _kudishikaReportRepository;
        private readonly IFamilyReportRepository _familyReportRepository;
        private readonly IFamilyMemberRepository _familyMemberRepository;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IUnitRepository _unitRepository;
        private readonly IFamilyRepository _familyRepository;
        private static Dictionary<string, string> UserState = new Dictionary<string, string>();

        public WebhookController(IKudishikaReportRepository kudishikaReportRepository,
            IFamilyReportRepository familyReportRepository,
            IFamilyMemberRepository familyMemberRepository,
            IFamilyMemberService familyMemberService,
            IUnitRepository unitRepository,
            IFamilyRepository familyRepository)
        {

            _kudishikaReportRepository = kudishikaReportRepository;
            _familyReportRepository = familyReportRepository;
            _familyMemberRepository = familyMemberRepository;
            _familyMemberService = familyMemberService;
            _unitRepository = unitRepository;
            _familyRepository = familyRepository;   
        }


        [HttpGet]
        public IActionResult VerifyWebhook(
         [FromQuery(Name = "hub.mode")] string mode,
         [FromQuery(Name = "hub.verify_token")] string token,
         [FromQuery(Name = "hub.challenge")] string challenge)
        {
            const string verifyToken = "FinChurchVerifyToken123";

            if (mode == "subscribe" && token == verifyToken)
            {
                return Ok(challenge); // Meta expects this response
            }

            return Forbid();
        }


        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    string rawJson = await reader.ReadToEndAsync();
                    Console.WriteLine($"🔍 Raw JSON Payload: {rawJson}");

                    var payload = JsonConvert.DeserializeObject<WhatsAppWebhookPayload>(rawJson);
                    Console.WriteLine($"✅ Parsed Payload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

                    string userMobile = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Contacts?.FirstOrDefault()?.WaId;
                    Console.WriteLine($"📞 User Mobile: {userMobile}");

                    // Ensure messages exist
                    var message = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Messages?.FirstOrDefault();
                    if (message?.From != null)
                    {
                        string sender = message.From;
                        string receivedText = message.Text?.Body ?? "[no text]";
                        Console.WriteLine($"📩 Received Message from {sender}: {receivedText}");

                        // Handle interactive button reply
                        string buttonReplyId = null;
                        string buttonReplyTitle = null;
                        string listReplyId = null;
                        string listReplyTitle = null;
                        if (message.Type == "interactive" && message.Interactive != null)
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

                        // ✅ Step 1: Handle "hi" message first
                        // ✅ Step 1: Enforce "Hi" as the first message
                        // Only enforce "Hi" if user has no state and is not in a valid flow
                        if (!receivedText.Equals("hi", StringComparison.OrdinalIgnoreCase)
                            && !UserState.ContainsKey(userMobile)
                            && buttonReplyId == null
                            && !(UserState.TryGetValue(userMobile, out var st) && (st == "name" || st == "family" || st == "unit" || st == "txn")))
                        {
                            await SendWhatsAppTextMessageAsync(userMobile, "👋 Please start by typing 'Hi' to proceed.");
                            return Ok(new { status = "received" }); // Stop further processing
                        }

                        // Handle initial "hi" flow
                        if (receivedText.Equals("hi", StringComparison.OrdinalIgnoreCase))
                        {
                            UserInfo userInfo = await GetUserInfoAsync(userMobile);

                            if (userInfo != null)
                            {
                                await SendWhatsAppInteractiveMessageAsync(
                                    userMobile,
                                    $"Hello {userInfo.FamilyName}! Choose an option:",
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
                                await SendWhatsAppTextMessageAsync(userMobile, "❌ Mobile number not found in our system.");
                            }
                            return Ok(new { status = "received" });
                        }

                        // Handle button replies
                        if (buttonReplyId != null)
                        {
                            switch (buttonReplyId)
                            {
                                case "menu_directory":
                                    UserState[userMobile] = "directory";
                                    await SendWhatsAppInteractiveMessageAsync(
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
                                    await SendWhatsAppTextMessageAsync(userMobile, "📌 Enter a name (at least 3 characters) to search.");
                                    break;
                                case "search_family":
                                    UserState[userMobile] = "family";
                                    await SendWhatsAppTextMessageAsync(userMobile, "📌 Enter a family number to see all members.");
                                    break;
                                case "search_unit":
                                    UserState[userMobile] = "unit_page_1";
                                    {
                                        UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                        if (userInfo != null)
                                        {
                                            var units = (await _unitRepository.GetAllAsync(userInfo.ParishId)).ToList();
                                            if (units.Any())
                                            {
                                                int pageSize = 9; // 9 units + 1 "Next Page" = 10 (WhatsApp hard limit)
                                                int page = 1;
                                                if (UserState.TryGetValue(userMobile, out var pageState) && pageState.StartsWith("unit_page_"))
                                                {
                                                    int.TryParse(pageState.Replace("unit_page_", ""), out page);
                                                }
                                                var pagedUnits = units.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                                                var rows = pagedUnits.Select(u => new
                                                {
                                                    id = $"unit_{u.UnitId}",
                                                    title = u.UnitName
                                                }).ToList();

                                                // Add "Next Page" if there are more units
                                                if (units.Count > page * pageSize)
                                                {
                                                    rows.Add(new
                                                    {
                                                        id = "unit_next_page",
                                                        title = "Next Page"
                                                    });
                                                }

                                                var sections = new List<object>
                                                {
                                                    new
                                                    {
                                                        title = $"Units {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedUnits.Count)}",
                                                        rows = rows.ToArray()
                                                    }
                                                };

                                                var unitpayload = new
                                                {
                                                    messaging_product = "whatsapp",
                                                    to = userMobile,
                                                    type = "interactive",
                                                    interactive = new
                                                    {
                                                        type = "list",
                                                        header = new { type = "text", text = "Select a Unit" },
                                                        body = new { text = "Please choose a unit from the list below:" },
                                                        action = new
                                                        {
                                                            button = "Select Unit",
                                                            sections = sections
                                                        }
                                                    }
                                                };

                                                await SendWhatsAppMessageAsync(unitpayload);

                                                // Save page state
                                                UserState[userMobile] = $"unit_page_{page}";
                                            }
                                            else
                                            {
                                                await SendWhatsAppTextMessageAsync(userMobile, "❌ No units found in your parish.");
                                            }
                                        }
                                    }
                                    break;
                                case "menu_dues":
                                    await SendWhatsAppTextMessageAsync(userMobile, "📜 Retrieving Family Dues...");
                                    {
                                        UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                        if (userInfo != null)
                                        {
                                            var report = await _kudishikaReportRepository.GetKudishikaReportAsync(
                                                userInfo.ParishId, userInfo.FamilyNumber, DateTime.Parse("2024-04-01"), DateTime.Parse("2025-05-21"), false);

                                            if (report != null)
                                            {
                                                string reportSummary = $"📜 Kudishika Report for {report.FamilyName}:\n\n";
                                                decimal totalClosingBalance = 0;
                                                foreach (var item in report.KudishikaItems)
                                                {
                                                    reportSummary += $"{item.HeadName} - {item.ClosingBalance}\n";
                                                    totalClosingBalance += item.ClosingBalance;
                                                }
                                                reportSummary += $"\n🔹 Total Closing Balance: {totalClosingBalance}";
                                                await SendWhatsAppTextMessageAsync(userMobile, reportSummary);
                                            }
                                            else
                                            {
                                                await SendWhatsAppTextMessageAsync(userMobile, "❌ Failed to retrieve family dues.");
                                            }
                                        }
                                    }
                                    break;
                                case "menu_txns":
                                    UserState[userMobile] = "txn";
                                    await SendWhatsAppTextMessageAsync(userMobile,
                                        "📜 Previous Transactions:\n\n" +
                                        "Reply with an option (e.g., `2023` or `10`) to get transactions by year or the last N transactions.");
                                    break;
                                default:
                                    await SendWhatsAppTextMessageAsync(userMobile, "❓ Unknown option selected.");
                                    break;
                            }
                            return Ok(new { status = "received" });
                        }

                        // Handle list reply for unit selection
                        if (listReplyId != null)
                        {
                            if (listReplyId == "unit_next_page")
                            {
                                // Go to next page
                                int page = 1;
                                if (UserState.TryGetValue(userMobile, out var pageState) && pageState.StartsWith("unit_page_"))
                                {
                                    int.TryParse(pageState.Replace("unit_page_", ""), out page);
                                }
                                page++;
                                UserState[userMobile] = $"unit_page_{page}";

                                // Re-run the unit selection logic for the next page
                                UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                if (userInfo != null)
                                {
                                    var units = (await _unitRepository.GetAllAsync(userInfo.ParishId)).ToList();
                                    int pageSize = 9;
                                    var pagedUnits = units.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                                    var rows = pagedUnits.Select(u => new
                                    {
                                        id = $"unit_{u.UnitId}",
                                        title = u.UnitName
                                    }).ToList();

                                    if (units.Count > page * pageSize)
                                    {
                                        rows.Add(new
                                        {
                                            id = "unit_next_page",
                                            title = "Next Page"
                                        });
                                    }

                                    var sections = new List<object>
                                    {
                                        new
                                        {
                                            title = $"Units {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedUnits.Count)}",
                                            rows = rows.ToArray()
                                        }
                                    };

                                    var unitpayload = new
                                    {
                                        messaging_product = "whatsapp",
                                        to = userMobile,
                                        type = "interactive",
                                        interactive = new
                                        {
                                            type = "list",
                                            header = new { type = "text", text = "Select a Unit" },
                                            body = new { text = "Please choose a unit from the list below:" },
                                            action = new
                                            {
                                                button = "Select Unit",
                                                sections = sections
                                            }
                                        }
                                    };

                                    await SendWhatsAppMessageAsync(unitpayload);
                                    UserState[userMobile] = $"unit_page_{page}";
                                }
                                return Ok(new { status = "received" });
                            }
                            else if (listReplyId.StartsWith("unit_"))
                            {
                                // Extract unitId from listReplyId
                                if (int.TryParse(listReplyId.Replace("unit_", ""), out int selectedUnitId))
                                {
                                    UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                    if (userInfo != null)
                                    {
                                        var families = (await _familyRepository.GetFamiliesAsync(userInfo.ParishId, selectedUnitId, null)).ToList();
                                        if (families.Any())
                                        {
                                            int pageSize = 9; // 9 families + 1 "Next Page" = 10
                                            int page = 1;
                                            if (UserState.TryGetValue(userMobile, out var famPageState) && famPageState.StartsWith("family_page_"))
                                            {
                                                int.TryParse(famPageState.Replace("family_page_", ""), out page);
                                            }
                                            var pagedFamilies = families.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                                            var rows = pagedFamilies.Select(f => new
                                            {
                                                id = $"family_{f.FamilyNumber}",
                                                // WhatsApp row title max length is 24 chars
                                                // Truncate if needed and remove extra spaces
                                                title = $"{(f.HeadName + " " + f.FamilyName).Trim()}".Length > 24
                                                    ? $"{(f.HeadName + " " + f.FamilyName).Trim().Substring(0, 22)}.."
                                                    : $"{(f.HeadName + " " + f.FamilyName).Trim()}"
                                            }).ToList();

                                            // Add "Next Page" if there are more families
                                            if (families.Count > page * pageSize)
                                            {
                                                rows.Add(new
                                                {
                                                    id = "family_next_page",
                                                    title = "Next Page"
                                                });
                                            }

                                            var sections = new List<object>
                                            {
                                                new
                                                {
                                                    title = $"Families {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedFamilies.Count)}",
                                                    rows = rows.ToArray()
                                                }
                                            };

                                            var familypayload = new
                                            {
                                                messaging_product = "whatsapp",
                                                to = userMobile,
                                                type = "interactive",
                                                interactive = new
                                                {
                                                    type = "list",
                                                    header = new { type = "text", text = "Select a Family" },
                                                    body = new { text = "Please choose a family from the list below:" },
                                                    action = new
                                                    {
                                                        button = "Select Family",
                                                        sections = sections
                                                    }
                                                }
                                            };

                                            await SendWhatsAppMessageAsync(familypayload);

                                            // Save page state
                                            UserState[userMobile] = $"family_page_{page}_unit_{selectedUnitId}";
                                        }
                                        else
                                        {
                                            await SendWhatsAppTextMessageAsync(userMobile, "❌ No families found in this unit.");
                                        }
                                    }
                                }
                                return Ok(new { status = "received" });
                            }
                            else if (listReplyId == "family_next_page")
                            {
                                // Pagination for families
                                // Extract unit id from state
                                int selectedUnitId = 0;
                                int page = 1;
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

                                UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                if (userInfo != null)
                                {
                                    var families = (await _familyRepository.GetFamiliesAsync(userInfo.ParishId, selectedUnitId, null)).ToList();
                                    int pageSize = 9;
                                    var pagedFamilies = families.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                                    var rows = pagedFamilies.Select(f => new
                                    {
                                        id = $"family_{f.FamilyNumber}",
                                        // WhatsApp row title max length is 24 chars
                                        // Truncate if needed and remove extra spaces
                                        title = $"{(f.HeadName + " " + f.FamilyName).Trim()}".Length > 24
                                            ? $"{(f.HeadName + " " + f.FamilyName).Trim().Substring(0, 22)}.."
                                            : $"{(f.HeadName + " " + f.FamilyName).Trim()}"
                                    }).ToList();

                                    if (families.Count > page * pageSize)
                                    {
                                        rows.Add(new
                                        {
                                            id = "family_next_page",
                                            title = "Next Page"
                                        });
                                    }

                                    var sections = new List<object>
                                    {
                                        new
                                        {
                                            title = $"Families {((page - 1) * pageSize + 1)}-{((page - 1) * pageSize + pagedFamilies.Count)}",
                                            rows = rows.ToArray()
                                        }
                                    };

                                    var familypayload = new
                                    {
                                        messaging_product = "whatsapp",
                                        to = userMobile,
                                        type = "interactive",
                                        interactive = new
                                        {
                                            type = "list",
                                            header = new { type = "text", text = "Select a Family" },
                                            body = new { text = "Please choose a family from the list below:" },
                                            action = new
                                            {
                                                button = "Select Family",
                                                sections = sections
                                            }
                                        }
                                    };

                                    await SendWhatsAppMessageAsync(familypayload);
                                    UserState[userMobile] = $"family_page_{page}_unit_{selectedUnitId}";
                                }
                                return Ok(new { status = "received" });
                            }
                            else if (listReplyId.StartsWith("family_"))
                            {
                                // Extract family number from listReplyId and call the handler
                                if (int.TryParse(listReplyId.Replace("family_", ""), out int selectedFamilyNumber))
                                {
                                    await HandleFamilySelectionAsync(userMobile, selectedFamilyNumber);
                                }
                                return Ok(new { status = "received" });
                            }
                        }

                        // Add this after the button reply switch, to handle user reply with year or count:
                        // Handle user reply for name search
                        if (UserState.TryGetValue(userMobile, out string searchType) && searchType == "name")
                        {
                            if (receivedText.Length >= 3)
                            {
                                UserInfo userInfo = await GetUserInfoAsync(userMobile);
                                var familyMembers = await _familyMemberService.GetFamilyMembersFilteredAsync(userInfo.ParishId, null,
                                    new FamilyMemberFilterRequest
                                    {
                                        Filters = new Dictionary<string, string> { { "FirstName", receivedText } }
                                    });

                                if (familyMembers.Data.Any())
                                {
                                    string responseMessage = $"🔎 Found {familyMembers.Data.Count()} members with name '{receivedText}':\n\n";
                                    foreach (var member in familyMembers.Data)
                                    {
                                        // Get last word from FamilyName
                                        var lastWord = member.FamilyName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
                                        responseMessage += $"👤 {member.FirstName} {lastWord}: {member.MobilePhone}\n";
                                    }
                                    await SendWhatsAppTextMessageAsync(userMobile, responseMessage);
                                }
                                else
                                {
                                    await SendWhatsAppTextMessageAsync(userMobile, "❌ No records found for this name.");
                                }
                            }
                            else
                            {
                                await SendWhatsAppTextMessageAsync(userMobile, "❌ Name must be at least 3 characters long. Please try again.");
                            }
                            UserState.Remove(userMobile); // Clear state after processing
                            return Ok(new { status = "received" });
                        }

                        // Handle user reply for family number search
                        if (UserState.TryGetValue(userMobile, out string searchTypef) && searchTypef == "family")
                        {
                            if (receivedText.Length >= 1 && int.TryParse(receivedText, out int number))
                            {
                                await HandleFamilySelectionAsync(userMobile, number);
                            }
                            else
                            {
                                await SendWhatsAppTextMessageAsync(userMobile, "❌ Please enter a valid family number.");
                            }
                            UserState.Remove(userMobile); // Clear state after processing
                            return Ok(new { status = "received" });
                        }

                        // Transaction/year logic
                        if (UserState.TryGetValue(userMobile, out string txnState) && txnState == "txn")
                        {
                            if (receivedText.Length == 2 && int.TryParse(receivedText, out int transactionCount))
                            {
                                // User entered a number → Fetch last N transactions
                                UserInfo userInfo = await GetUserInfoAsync(userMobile);

                                if (userInfo != null)
                                {
                                    var report = await _familyReportRepository.GetFamilyReportAsync(userInfo.ParishId, userInfo.FamilyNumber);

                                    if (report?.Transactions != null)
                                    {
                                        var recentTransactions = report.Transactions
                                            .OrderByDescending(t => t.TrDate)
                                            .Take(transactionCount)
                                            .ToList();

                                        string transactionSummary = $"📜 Last {transactionCount} Transactions:\n\n";

                                        foreach (var transaction in recentTransactions)
                                        {
                                            transactionSummary += $"{transaction.TrDate:yyyy-MM-dd} | {transaction.VrNo} | {transaction.HeadName} | {transaction.IncomeAmount}\n";
                                        }

                                        // Always include Total Paid
                                        transactionSummary += $"\n💰 Total Paid: {report.TotalPaid}";

                                        await SendWhatsAppTextMessageAsync(userMobile, transactionSummary);
                                    }
                                }
                            }
                            else if (receivedText.Length == 4 && int.TryParse(receivedText, out int year))
                            {
                                DateTime selectedYear = new DateTime(year, 1, 1); // Convert year to DateTime

                                // User entered a year → Fetch all transactions from that year
                                UserInfo userInfo = await GetUserInfoAsync(userMobile);

                                if (userInfo != null)
                                {
                                    var report = await _familyReportRepository.GetFamilyReportAsync(userInfo.ParishId, userInfo.FamilyNumber);

                                    if (report?.Transactions != null)
                                    {
                                        var yearlyTransactions = report.Transactions
                                            .Where(t => t.TrDate.Year == selectedYear.Year)
                                            .ToList();

                                        string transactionSummary = $"📜 Transactions for {selectedYear.Year}:\n\n";

                                        foreach (var transaction in yearlyTransactions)
                                        {
                                            transactionSummary += $"{transaction.TrDate:yyyy-MM-dd} | {transaction.VrNo} | {transaction.HeadName} | {transaction.IncomeAmount}\n";
                                        }
                                        // Always include Total Paid
                                        transactionSummary += $"\n💰 Total Paid: {report.TotalPaid}";

                                        await SendWhatsAppTextMessageAsync(userMobile, transactionSummary);
                                    }
                                }
                            }
                            UserState.Remove(userMobile); // Clear state after processing
                            return Ok(new { status = "received" });
                        }
                      
                    }
                    return Ok(new { status = "received" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 Error processing webhook: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        private readonly HttpClient _httpClient = new HttpClient();

        private async Task SendWhatsAppMessageAsync(object payload)
        {
            string phoneNumberId = "576545658884985"; // Replace with your actual phone number ID
            string accessToken = "EACNw4WbHROwBOZCxI3TZCpeZByUitWc1oKGiZB9sKzCluBfDzn0ztA4q2OdDr1ZAJhUYu96orW0mGa31btNYUpLNJK64pd5mGhiUeKmk2ZBbkkA0QsWwpI3PP0AKeEiyq1hYZC0avATb9ZAnhztgWrRtOuiFpBYYaTeKU39948tXaHHyCIICDYEMEuMKln6KjcXIZC4jNBZARofMd5UmYgZCrHUuBwofPAZD";

            string url = $"https://graph.facebook.com/v19.0/{phoneNumberId}/messages";

            var json = JsonConvert.SerializeObject(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"WhatsApp API response: {response.StatusCode}\n{responseContent}");
        }

        // Helper for text messages
        private async Task SendWhatsAppTextMessageAsync(string recipientNumber, string messageText)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = recipientNumber,
                type = "text",
                text = new { body = messageText }
            };
            await SendWhatsAppMessageAsync(payload);
        }

   
        // Helper for interactive messages (customizable)
        private async Task SendWhatsAppInteractiveMessageAsync(string recipientNumber, string bodyText, Dictionary<string, string> buttons)
        {
            var buttonList = buttons.Select(kv => new
            {
                type = "reply",
                reply = new { id = kv.Key, title = kv.Value }
            }).ToArray();

            var payload = new
            {
                messaging_product = "whatsapp",
                to = recipientNumber,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    body = new { text = bodyText },
                    action = new
                    {
                        buttons = buttonList
                    }
                }
            };
            await SendWhatsAppMessageAsync(payload);
        }

        private async Task<UserInfo> GetUserInfoAsync(string mobileNumber)
        {
            UserInfo user = GetDummyUserInfo();
            return user;

            return null;
        }

        private UserInfo GetDummyUserInfo()
        {
            return new UserInfo
            {
                ParishId = 31,
                FamilyId = 299,
                FamilyNumber = 29,
                FamilyName = "Kozhikunnath"
            };
        }

        #region Model Claas


        public class WhatsAppWebhookPayload
        {
            [JsonProperty("object")]
            public string Object { get; set; }

            [JsonProperty("entry")]
            public List<Entry> Entry { get; set; }
        }

        public class Entry
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("changes")]
            public List<Change> Changes { get; set; }
        }

        public class Change
        {
            [JsonProperty("value")]
            public Value Value { get; set; }

            [JsonProperty("field")]
            public string Field { get; set; }
        }

        public class Value
        {
            [JsonProperty("messaging_product")]
            public string MessagingProduct { get; set; }

            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }

            [JsonProperty("contacts")]
            public List<Contact> Contacts { get; set; }

            [JsonProperty("messages")]
            public List<Message> Messages { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("display_phone_number")]
            public string DisplayPhoneNumber { get; set; }

            [JsonProperty("phone_number_id")]
            public string PhoneNumberId { get; set; }
        }

        public class Contact
        {
            [JsonProperty("profile")]
            public Profile Profile { get; set; }

            [JsonProperty("wa_id")]
            public string WaId { get; set; }
        }

        public class Profile
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Message
        {
            [JsonProperty("from")]
            public string From { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("timestamp")]
            public string Timestamp { get; set; }

            [JsonProperty("text")]
            public TextContent Text { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("interactive")]
            public InteractiveContent Interactive { get; set; }
        }

        public class TextContent
        {
            [JsonProperty("body")]
            public string Body { get; set; }
        }

        public class InteractiveContent
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("button_reply")]
            public ButtonReplyContent ButtonReply { get; set; }

            [JsonProperty("list_reply")]
            public ListReplyContent ListReply { get; set; }
        }

        public class ButtonReplyContent
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }
        }

        public class ListReplyContent
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }
        }
        #endregion

        public class UserInfo
        {
            public int ParishId { get; set; }
            public int FamilyId { get; set; }
            public int FamilyNumber { get; set; }
            public string FamilyName { get; set; }
        }


        // Add this method inside your WebhookController class (anywhere before the closing brace)
        private async Task HandleFamilySelectionAsync(string userMobile, int familyNumber)
        {
            UserInfo userInfo = await GetUserInfoAsync(userMobile);
            var familyMembers = await _familyMemberService.GetFamilyMembersFilteredAsync(userInfo.ParishId, familyNumber,
                new FamilyMemberFilterRequest
                {
                    Filters = new Dictionary<string, string> { { "ActiveMemberd", familyNumber.ToString() } }
                });

            if (familyMembers.Data.Any())
            {
                string responseMessage = $"🔎 Found {familyMembers.Data.Count()} members for family number '{familyNumber}':\n\n";
                foreach (var member in familyMembers.Data)
                {
                    // Get last word from FamilyName
                    var lastWord = member.FamilyName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
                    responseMessage += $"👤 {member.FirstName} {lastWord}: {member.MobilePhone}\n";
                }
                await SendWhatsAppTextMessageAsync(userMobile, responseMessage);
            }
            else
            {
                await SendWhatsAppTextMessageAsync(userMobile, "❌ No records found for this family number.");
            }
        }

    }
}
