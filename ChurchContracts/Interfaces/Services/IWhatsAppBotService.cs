using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchDTOs.DTOs.Utils;

namespace ChurchContracts.Interfaces.Services
{
    public interface IWhatsAppBotService
    {
        // Greeting and entry flows
        Task HandleHiFlowAsync(string userMobile);

        // Transaction-based logic
        Task<bool> HandleTransactionOrYearAsync(string userMobile, string receivedText);
       

        // Family dues
        Task SendFamilyDuesAsync(string userMobile);

        // Unit & family selection
        Task SendUnitSelectionAsync(string userMobile);
        Task SendFamilySelectionAsync(string userMobile, int page);
        //Task SendFamilyConfirmationAsync(string userMobile, int familyNumber);


        // Member detail request
        Task<bool> HandleMemberDetailRequestAsync(string userMobile, string receivedText);

        // General fallback
        Task SendDefaultMessageAsync(string userMobile);

        // Button/List processing
        Task HandleButtonPostbackAsync(string userMobile, string buttonPayload);
        Task HandleListReplyAsync(string userMobile, string sectionId, string rowId);

        // Directory and navigation flows
        Task<bool> HandleButtonRepliesAsync(string userMobile, string buttonReplyId, string buttonReplyTitle);
        Task<bool> HandleListRepliesAsync(string userMobile, string listReplyId, string listReplyTitle);
        Task HandleFamilySelectionAsync(string userMobile, int familyNumber);
        Task SendFamilySelectionAsync(string userMobile, int selectedUnitId, int page);
        Task DirectoryBackAsync(string userMobile);

        Task ParseInteractiveReplyAsync(object message, Action<string, string, string, string> setValues);
        Task<bool> EnforceHiFirstMessageAsync(string userMobile, string receivedText, string buttonReplyId);
        Task<bool> HandleNameSearchAsync(string userMobile, string receivedText);
        Task<bool> HandleFamilyNumberSearchAsync(string userMobile, string receivedText);
    }
    public enum TransactionReportStyle
    {
        Default,       // Tabular with triple backticks
        CompactBlock,  // Clean block, bullet-based
        AlignedBlock   // With pipe symbol alignment
    }


}
