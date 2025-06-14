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

        public WhatsAppBotService(
            IKudishikaReportRepository kudishikaReportRepository,
            IFamilyReportRepository familyReportRepository,
            IFamilyRepository familyRepository,
            IUnitRepository unitRepository,
            IFamilyMemberService familyMemberService,
            IWhatsAppMessageSender messageSender,
            IUserStateService userState)
        {
            _kudishikaReportRepository = kudishikaReportRepository;
            _familyReportRepository = familyReportRepository;
            _familyRepository = familyRepository;
            _unitRepository = unitRepository;
            _familyMemberService = familyMemberService;
            _messageSender = messageSender;
            _userState = userState;
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
            
    

        public Task SendFamilyConfirmationAsync(string userMobile, int familyNumber) => Task.CompletedTask;
        public Task<bool> HandleMemberDetailRequestAsync(string userMobile, string receivedText) => Task.FromResult(false);
        public Task SendDefaultMessageAsync(string userMobile) => Task.CompletedTask;
        public Task HandleButtonPostbackAsync(string userMobile, string buttonPayload) => Task.CompletedTask;
        public Task HandleListReplyAsync(string userMobile, string sectionId, string rowId) => Task.CompletedTask;

      


     
       
    }
}
