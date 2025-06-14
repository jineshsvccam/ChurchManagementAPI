using Newtonsoft.Json;

namespace ChurchData.Utils
{
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
        public string FirstName { get; set; }
    }
}
