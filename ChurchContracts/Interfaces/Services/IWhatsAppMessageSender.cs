using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchContracts.Interfaces.Services
{
    public interface IWhatsAppMessageSender
    {
        Task SendTextMessageAsync(string recipientNumber, string messageText);
        Task SendInteractiveMessageAsync(string recipientNumber, string bodyText, Dictionary<string, string> buttons);
        Task SendListMessageAsync(string userMobile, string header, string body, string buttonLabel, List<(string id, string title)> rows, string sectionTitle);

    }

}
