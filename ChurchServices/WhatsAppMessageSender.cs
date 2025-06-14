using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ChurchServices
{
    public class WhatsAppMessageSender:IWhatsAppMessageSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public WhatsAppMessageSender(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        private async Task SendWhatsAppMessageAsync(object payload)
        {
            string phoneNumberId = _config["WhatsApp:PhoneNumberId"];
            string accessToken = _config["WhatsApp:AccessToken"];

            string url = $"https://graph.facebook.com/v19.0/{phoneNumberId}/messages";

            var json = JsonConvert.SerializeObject(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📤 WhatsApp API response: {response.StatusCode}\n{responseContent}");
        }

        public async Task SendTextMessageAsync(string recipientNumber, string messageText)
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

        public async Task SendInteractiveMessageAsync(string recipientNumber, string bodyText, Dictionary<string, string> buttons)
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
                    action = new { buttons = buttonList }
                }
            };

            await SendWhatsAppMessageAsync(payload);
        }

        public async Task SendListMessageAsync(string userMobile, string header, string body, string buttonLabel, List<(string id, string title)> rows, string sectionTitle)
        {
            var rowObjects = rows.Select(r => new
            {
                id = r.id,
                title = r.title
            }).ToArray();

            var payload = new
            {
                messaging_product = "whatsapp",
                to = userMobile,
                type = "interactive",
                interactive = new
                {
                    type = "list",
                    header = new { type = "text", text = header },
                    body = new { text = body },
                    action = new
                    {
                        button = buttonLabel,
                        sections = new[]
                        {
                    new
                    {
                        title = sectionTitle,
                        rows = rowObjects
                    }
                }
                    }
                }
            };

            await SendWhatsAppMessageAsync(payload);
        }

    }
}
