using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Repositories.Users
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .HasColumnType("varbinary(256)");

            builder.Property(u => u.PasswordSalt)
                .HasColumnType("varbinary(128)");

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);  // Varsayılan olarak aktif

            builder.Property(u => u.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");  // Varsayılan olarak şu anki zaman

            // İlişkiler (HasMany ve WithMany)
            builder.HasMany(u => u.UserRoles)
                .WithOne(a =>a.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Kullanıcı silindiğinde ilgili roller de silinsin

            builder.HasMany(u => u.Categories)
                .WithOne(b =>b.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde kategoriler silinmesin

            builder.HasMany(u => u.Branches)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde şubeler silinmesin

            builder.HasMany(u => u.Companies)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde şirketler silinmesin

            builder.HasMany(u => u.BarcodeCards)
                .WithOne(bc => bc.User)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde barkod kartları silinmesin

            builder.HasMany(u => u.StockCards)
                .WithOne(sc => sc.User)
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde stok kartları silinmesin

            builder.HasMany(u => u.StockTransactions)
                .WithOne(st => st.User)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde stok işlemleri silinmesin

            builder.HasMany(u => u.MainGroups)
                .WithOne(mg => mg.User)
                .HasForeignKey(mg => mg.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde ana gruplar silinmesin

            builder.HasMany(u => u.SubGroups)
                .WithOne(sg => sg.User)
                .HasForeignKey(sg => sg.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde alt gruplar silinmesin

            builder.HasMany(u => u.Warehouses)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde depolar silinmesin

            builder.HasMany(u => u.PriceDefinitions)
                .WithOne(pd => pd.User)
                .HasForeignKey(pd => pd.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Kullanıcı silindiğinde fiyatlar silinmesin
        }
    }
}
