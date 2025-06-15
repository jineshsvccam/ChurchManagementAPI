using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts.Interfaces.Services;
using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchDTOs.DTOs.Utils;
using ChurchCommon.Utils;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;


namespace ChurchServices.WhatsAppBot
{
    public partial class WhatsAppBotService : IWhatsAppBotService
    {
        private readonly IKudishikaReportRepository _kudishikaReportRepository;
        private readonly IFamilyReportRepository _familyReportRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IWhatsAppMessageSender _messageSender;

        //  private static Dictionary<string, string> UserState = new Dictionary<string, string>();
        private readonly IUserStateService _userState;
        private readonly IMemoryCache _memoryCache;

        public WhatsAppBotService(
            IKudishikaReportRepository kudishikaReportRepository,
            IFamilyReportRepository familyReportRepository,
            IFamilyRepository familyRepository,
            IUnitRepository unitRepository,
            IFamilyMemberService familyMemberService,
            IWhatsAppMessageSender messageSender,
            IUserStateService userState,
            IMemoryCache memoryCache)
        {
            _kudishikaReportRepository = kudishikaReportRepository;
            _familyReportRepository = familyReportRepository;
            _familyRepository = familyRepository;
            _unitRepository = unitRepository;
            _familyMemberService = familyMemberService;
            _messageSender = messageSender;
            _userState = userState;
            _memoryCache = memoryCache;
        }

        private async Task<UserInfo?> GetUserInfoAsync(string mobileNumber)
        {
            string cacheKey = $"userinfo:{mobileNumber}";

            // Check cache first
            if (_memoryCache.TryGetValue<UserInfo>(cacheKey, out var cachedUser))
            {
                return cachedUser;
            }

            // Otherwise fetch and cache
            var dto = await _familyMemberService.ValidateUserWithMobile(mobileNumber);
            if (dto == null) return null;

            var userInfo = new UserInfo
            {
                ParishId = (int)dto.ParishId,
                FamilyId = dto.FamilyId,
                FamilyNumber = dto.FamilyNumber,
                FamilyName = dto.FamilyName,
                FirstName = dto.FirstName
            };

            _memoryCache.Set(cacheKey, userInfo, TimeSpan.FromMinutes(2)); // Cache for 2 mins
                                                                           // _memoryCache.Remove($"userinfo:{mobileNumber}");//If you ever need to force refresh:
            return userInfo;
        }

        private async Task<string> GetBackNoteAsync(string userMobile)
        {
            var state = await _userState.GetStateAsync(userMobile);

            if (string.IsNullOrEmpty(state)) return string.Empty;

            return state switch
            {
                var s when s.StartsWith("selected_family_") => "↩️ Type *back* to return to family list.",
                var s when s.StartsWith("family_page_") => "↩️ Type *back* to return to unit list.",
                var s when s.StartsWith("unit_page_") => "📌↩️ Type *back* to return to main menu.",
                var s when s == "dues" => "📌↩️ Type *back* to return to your dashboard.",
                var s when s == "name" => "↩️ Type *back* to return to member directory.",
                var s when s == "family" => "↩️ Type *back* to return to family directory.",
                _ => "📌↩️ Type *back* to return to the previous menu."
            };
        }


        public async Task RouteBackAsync(string userMobile)
        {
            var state = await _userState.GetStateAsync(userMobile);

            if (string.IsNullOrEmpty(state))
            {
                await HandleHiFlowAsync(userMobile);
                return;
            }

            if (state == "name" || state == "family")
            {
                await DirectoryBackAsync(userMobile); // resets to "directory"
                return;
            }

            if (state == "dues")
            {
                await MainMenuBackAsync(userMobile); // resets to main menu (hi menu or dashboard)
                return;
            }

            if (state.StartsWith("selected_family_"))
            {
                // Optional: parse previous unit if tracked
                await SendFamilySelectionAsync(userMobile,1, 1);
                await _userState.SetStateAsync(userMobile, "family_page_1_unit_1");
                return;
            }

            if (state.StartsWith("family_page_"))
            {
                await SendUnitSelectionAsync(userMobile, 1);
                await _userState.SetStateAsync(userMobile, "unit_page_1");
                return;
            }

            await HandleHiFlowAsync(userMobile); // fallback
            await _userState.ClearStateAsync(userMobile);
        }

        private int ExtractUnitIdFromState(string state)
        {
            var match = Regex.Match(state, @"_unit_(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }
        public Task SendFamilyConfirmationAsync(string userMobile, int familyNumber) => Task.CompletedTask;
        public Task<bool> HandleMemberDetailRequestAsync(string userMobile, string receivedText) => Task.FromResult(false);
        public Task SendDefaultMessageAsync(string userMobile) => Task.CompletedTask;
        public Task HandleButtonPostbackAsync(string userMobile, string buttonPayload) => Task.CompletedTask;
        public Task HandleListReplyAsync(string userMobile, string sectionId, string rowId) => Task.CompletedTask;


    }
}
