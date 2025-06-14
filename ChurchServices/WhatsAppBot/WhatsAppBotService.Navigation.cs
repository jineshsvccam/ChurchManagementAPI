using ChurchDTOs.DTOs.Entities;

namespace ChurchServices.WhatsAppBot
{
    public partial class WhatsAppBotService
    {
        // Navigation/menu methods:
        // - HandleHiFlowAsync
        // - SendUnitSelectionAsync
        // - SendUnitSelectionAsync (overload)
        // - SendFamilySelectionAsync (overload)

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
                await _userState.ClearStateAsync(userMobile); // Reset state on fresh "hi"
            }
            else
            {
                await _messageSender.SendTextMessageAsync(userMobile, "❌ Mobile number not found in our system.");
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

            await _userState.SetStateAsync(userMobile, $"unit_page_{page}", TimeSpan.FromMinutes(10));
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

            await _userState.SetStateAsync(userMobile, $"family_page_{page}_unit_{selectedUnitId}", TimeSpan.FromMinutes(10));
        }

    }
}
