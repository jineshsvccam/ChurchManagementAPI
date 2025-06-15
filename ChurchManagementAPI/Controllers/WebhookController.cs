using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft.Json;
using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;
using ChurchServices;
using ChurchDTOs.DTOs.Utils;
using ChurchContracts.Interfaces.Services;
using ChurchData.Utils;

namespace ChurchManagementAPI.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {

        private readonly IWhatsAppBotService _whatsAppBotService;

        public WebhookController(
            IWhatsAppBotService whatsAppBotService)
        {
            _whatsAppBotService = whatsAppBotService;
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
                 //   Console.WriteLine($"🔍 Raw JSON Payload: {rawJson}");

                    var payload = JsonConvert.DeserializeObject<WhatsAppWebhookPayload>(rawJson);
                 //   Console.WriteLine($"✅ Parsed Payload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

                    string userMobile = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Contacts?.FirstOrDefault()?.WaId;
                    Console.WriteLine($"📞 User Mobile: {userMobile}");

                    var message = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Messages?.FirstOrDefault();
                    if (message?.From != null)
                    {
                        string sender = message.From;
                        string receivedText = message.Text?.Body ?? "[no text]";
                        Console.WriteLine($"📩 Received Message from {sender}: {receivedText}");


                        // Handle "back" globally
                        if (receivedText.Equals("back", StringComparison.OrdinalIgnoreCase))
                        {
                            await _whatsAppBotService.RouteBackAsync(userMobile);
                            return Ok(new { status = "received" });
                        }

                        // Parse interactive replies (moved to service)
                        string buttonReplyId = null, buttonReplyTitle = null, listReplyId = null, listReplyTitle = null;
                        await _whatsAppBotService.ParseInteractiveReplyAsync(message, (a, b, c, d) =>
                        {
                            buttonReplyId = a;
                            buttonReplyTitle = b;
                            listReplyId = c;
                            listReplyTitle = d;
                        });

                        // Enforce "Hi" as first message (moved to service)
                        if (await _whatsAppBotService.EnforceHiFirstMessageAsync(userMobile, receivedText, buttonReplyId))
                            return Ok(new { status = "received" });

                        // Handle "hi" flow (moved to service)
                        if (receivedText.Equals("hi", StringComparison.OrdinalIgnoreCase))
                        {
                            await _whatsAppBotService.HandleHiFlowAsync(userMobile);
                            return Ok(new { status = "received" });
                        }

                        // Handle button replies (moved to service)
                        if (await _whatsAppBotService.HandleButtonRepliesAsync(userMobile, buttonReplyId, buttonReplyTitle))
                            return Ok(new { status = "received" });

                        // Handle list replies (moved to service)
                        if (await _whatsAppBotService.HandleListRepliesAsync(userMobile, listReplyId, listReplyTitle))
                            return Ok(new { status = "received" });

                        // Handle name search (moved to service)
                        if (await _whatsAppBotService.HandleNameSearchAsync(userMobile, receivedText))
                            return Ok(new { status = "received" });

                        // Handle family number search (moved to service)
                        if (await _whatsAppBotService.HandleFamilyNumberSearchAsync(userMobile, receivedText))
                            return Ok(new { status = "received" });

                        // Handle transaction/year logic
                        if (await _whatsAppBotService.HandleTransactionOrYearAsync(userMobile, receivedText))
                            return Ok(new { status = "received" });

                        // Handle "back" action if in dues state
                        if (await _whatsAppBotService.SendFamilyDuesAsync(userMobile, receivedText))
                            return Ok(new { status = "received" });

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



    }
}
