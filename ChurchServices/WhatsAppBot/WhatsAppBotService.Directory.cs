using ChurchDTOs.DTOs.Entities;

namespace ChurchServices.WhatsAppBot
{
    public partial class WhatsAppBotService
    {
        // Directory-related methods:
        // - HandleNameSearchAsync
        // - HandleFamilyNumberSearchAsync
        // - DirectoryBackAsync
        // - HandleFamilySelectionAsync
        // - SendFamilySelectionAsync

        public async Task<bool> HandleNameSearchAsync(string userMobile, string receivedText)
        {
            var searchType = await _userState.GetStateAsync(userMobile);

            if (searchType == "name")
            {
                if (receivedText.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    await RouteBackAsync(userMobile);
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

                        responseMessage += "\n\n" + await GetBackNoteAsync(userMobile);

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
                // ⛔ Removed:
                // await _userState.ClearStateAsync(userMobile);
                // Keep user in 'name' flow until they type 'back'

                return true;
            }
            return false;
        }


        public async Task<bool> HandleFamilyNumberSearchAsync(string userMobile, string receivedText)
        {
            var searchTypef = await _userState.GetStateAsync(userMobile);
            if (searchTypef == "family")
            {
                if (receivedText.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    await RouteBackAsync(userMobile);
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
              //  await _userState.ClearStateAsync(userMobile);
                return true;
            }
            return false;
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
            await _userState.SetStateAsync(userMobile, "directory", TimeSpan.FromMinutes(10));
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
                responseMessage += "\n\n" + await GetBackNoteAsync(userMobile);
                await _messageSender.SendTextMessageAsync(userMobile, responseMessage);
            }
            else
            {
                await _messageSender.SendTextMessageAsync(userMobile, "❌ No records found for this family number.");
            }
        }

        // Add this overload to satisfy the interface
        public Task SendFamilySelectionAsync(string userMobile, int page)
        {
            // This method is not used in the new flow, but required by the interface.
            // You can implement a default or throw if not needed.
            throw new NotImplementedException();
        }


    }
}
