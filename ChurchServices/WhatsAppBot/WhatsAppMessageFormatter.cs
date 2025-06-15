using System.Collections.Generic;
using System.Text;
using ChurchContracts.Interfaces.Services;
using ChurchDTOs.DTOs.Entities;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices.WhatsAppBot
{
    public static class WhatsAppMessageFormatter
    {
        public static string FormatTransactionReport(string title, List<FinancialReportCustomDTO> transactions, decimal totalPaid, TransactionReportStyle style = TransactionReportStyle.CompactBlock)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{title}\n");

            switch (style)
            {
                case TransactionReportStyle.Default:
                    // Table Style
                    sb.AppendLine("```");                  
                    sb.AppendLine("Date       | Ref No  | Head         | Amount ");
                    sb.AppendLine("-----------|---------|--------------|--------");

                    foreach (var t in transactions)
                    {
                        string date = t.TrDate.ToString("yyyy-MM-dd").PadRight(10);
                        string refNo = t.VrNo.Length > 8 ? t.VrNo.Substring(0, 8) : t.VrNo.PadRight(8);
                        string head = t.HeadName.Length > 13 ? t.HeadName.Substring(0, 13) : t.HeadName.PadRight(13);
                        string amount = $"₹{t.IncomeAmount:N2}".PadLeft(8);

                        sb.AppendLine($"{date} | {refNo} | {head} | {amount}");
                    }

                    sb.AppendLine("```");
                    sb.AppendLine("\n📌↩️ Type *back* to return to the previous menu.");
                    break;

                case TransactionReportStyle.CompactBlock:
                    // Block Style with Emojis
                    
                    sb.AppendLine("```"); // Start monospaced block

                    int i = 1;
                    foreach (var t in transactions)
                    {
                        //sb.AppendLine($"{i}) 📅 Date: {t.TrDate:yyyy-MM-dd}");
                        //sb.AppendLine($"   🧾 Ref: {t.VrNo}");
                        //sb.AppendLine($"   🗂️ Head: {t.HeadName}");
                        //sb.AppendLine($"   💸 Amount: ₹{t.IncomeAmount:N2}\n");
                        //i++;

                        string date = t.TrDate.ToString("yyyy-MM-dd").PadRight(11);     // 11 for date + spacing
                        string head = t.HeadName.Length > 12 ? t.HeadName[..12] : t.HeadName.PadRight(12);

                        string refNo = t.VrNo.Length > 10 ? t.VrNo[..10] : t.VrNo.PadRight(10);
                        string amount = $"₹{t.IncomeAmount:N2}".PadRight(12);             // Right-align amount

                        sb.AppendLine($"• {date}| {head}");
                        sb.AppendLine($"  {refNo}| {amount}\n");

                        i++;
                    }
                    sb.AppendLine("```"); // End monospaced block
                    break;
            }

            sb.AppendLine($"\n💰 *Total Paid:* ₹{totalPaid:N2}\n");
            sb.AppendLine("📌↩ *Type back to return to the previous menu.*");

            return sb.ToString();
        }

    }
}
