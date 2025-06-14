using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices.WhatsAppBot
{
    public partial class WhatsAppBotService
    {
        // Reply handler methods:
        // - HandleButtonRepliesAsync
        // - HandleListRepliesAsync
        // - ParseInteractiveReplyAsync
        // - EnforceHiFirstMessageAsync

        public async Task<bool> HandleButtonRepliesAsync(string userMobile, string buttonReplyId, string buttonReplyTitle)
        {
            if (buttonReplyId == null) return false;
            switch (buttonReplyId)
            {
                case "menu_directory":                   
                    await _userState.SetStateAsync(userMobile, "directory", TimeSpan.FromMinutes(10));
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
                    await _userState.SetStateAsync(userMobile, "name", TimeSpan.FromMinutes(10));
                    await _messageSender.SendTextMessageAsync(userMobile, "📌 Enter a name (at least 3 characters) to search.");
                    break;
                case "search_family":
                    await _userState.SetStateAsync(userMobile, "family", TimeSpan.FromMinutes(10));
                    await _messageSender.SendTextMessageAsync(userMobile, "📌 Enter a family number to see all members.");
                    break;
                case "search_unit":
                   
                    await _userState.SetStateAsync(userMobile, "unit_page_1", TimeSpan.FromMinutes(10));
                    await SendUnitSelectionAsync(userMobile);
                    break;
                case "menu_dues":
                    await _messageSender.SendTextMessageAsync(userMobile, "📜 Retrieving Family Dues...");
                    await SendFamilyDuesAsync(userMobile);
                    break;
                case "menu_txns":
                   
                    await _userState.SetStateAsync(userMobile, "txn", TimeSpan.FromMinutes(10));
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
                var pageState = await _userState.GetStateAsync(userMobile);
                if (pageState.StartsWith("unit_page_"))
                {
                    int.TryParse(pageState.Replace("unit_page_", ""), out page);
                }
                page++;
               
                await _userState.SetStateAsync(userMobile, $"unit_page_{page}", TimeSpan.FromMinutes(10));
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
                var famPageState = await _userState.GetStateAsync(userMobile);
                if (famPageState.StartsWith("family_page_"))
                {
                    var parts = famPageState.Split("_unit_");
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[0].Replace("family_page_", ""), out page);
                        int.TryParse(parts[1], out selectedUnitId);
                    }
                }
                page++;
                await _userState.SetStateAsync(userMobile, $"family_page_{page}_unit_{selectedUnitId}", TimeSpan.FromMinutes(10));             
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
            var validStates = new HashSet<string> { "name", "family", "unit", "txn" };
            var st = await _userState.GetStateAsync(userMobile);
            bool hasState = validStates.Contains(st ?? string.Empty);

            if (!hasState && string.IsNullOrEmpty(buttonReplyId))
            {
                await _messageSender.SendTextMessageAsync(userMobile, "👋 Please start by typing 'Hi' to proceed.");
                return true;
            }

            return false;
        }


    }
}