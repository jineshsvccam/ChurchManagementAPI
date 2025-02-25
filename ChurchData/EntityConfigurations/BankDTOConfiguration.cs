using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class BankDTOConfiguration : IEntityTypeConfiguration<BankDTO>
    {
        public void Configure(EntityTypeBuilder<BankDTO> builder)
        {
            builder.HasNoKey();
            builder.Property(e => e.BankId).HasColumnName("bank_id");
            builder.Property(b => b.BankName).HasColumnName("bank_name");
            builder.Property(b => b.OpeningBalance).HasColumnName("opening_balance");
            builder.Property(b => b.ClosingBalance).HasColumnName("closing_balance");
            builder.Property(b => b.Balance).HasColumnName("balance");
        }
    }
}
