using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Repositories.StockCards
{
    public class StockCardConfiguration : IEntityTypeConfiguration<StockCard>
    {
        public void Configure(EntityTypeBuilder<StockCard> builder)
        {
            builder.ToTable("StockCards");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            // Unique indexes scoped by Company
            builder.HasIndex(sc => new { sc.CompanyId, sc.Name })
                   .IsUnique();

            builder.HasIndex(sc => new { sc.CompanyId, sc.Code })
                   .IsUnique();

            builder.Property(sc => sc.Type)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(s => s.Unit)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(s => s.Tax)
                   .IsRequired()
                   .HasColumnType("decimal(5,2)");

            builder.Property(s => s.IsActive)
                   .HasDefaultValue(true);

            builder.Property(s => s.CreatedDate)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(s => s.Company)
                   .WithMany(c => c.StockCards)
                   .HasForeignKey(s => s.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Branch)
                   .WithMany(b => b.StockCards)
                   .HasForeignKey(s => s.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.MainGroup)
                   .WithMany(mg => mg.StockCards)
                   .HasForeignKey(s => s.MainGroupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.SubGroup)
                   .WithMany(sg => sg.StockCards)
                   .HasForeignKey(s => s.SubGroupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Category)
                   .WithMany(c => c.StockCards)
                   .HasForeignKey(s => s.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.BarcodeCards)
                   .WithOne(bc => bc.StockCard)
                   .HasForeignKey(bc => bc.StockCardId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.PriceDefinitions)
                   .WithOne(pd => pd.StockCard)
                   .HasForeignKey(pd => pd.StockCardId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.StockTransactions)
                   .WithOne(st => st.StockCard)
                   .HasForeignKey(st => st.StockCardId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sc => sc.User)
                   .WithMany(s => s.StockCards)
                   .HasForeignKey(sc => sc.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sc => sc.WarehouseStocks)
                   .WithOne(ws => ws.StockCard)
                   .HasForeignKey(ws => ws.StockCardId)
                   .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
