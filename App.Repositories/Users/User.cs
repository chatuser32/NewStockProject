using App.Repositories.Warehouses;
using App.Repositories.PriceDefinitions;
using App.Repositories.Categories;
using App.Repositories.UserRoles;
using App.Repositories.Branches;
using App.Repositories.Companies;
using App.Repositories.BarcodeCards;
using App.Repositories.MainGroups;
using App.Repositories.SubGroups;
using App.Repositories.StockCards;

namespace App.Repositories.Users
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<Company> Companies { get; set; } = new List<Company>();
        public ICollection<BarcodeCard> BarcodeCards { get; set; } = new List<BarcodeCard>();
        public ICollection<StockCard> StockCards { get; set; } = new List<StockCard>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        public ICollection<MainGroup> MainGroups { get; set; } = new List<MainGroup>();
        public ICollection<SubGroup> SubGroups { get; set; } = new List<SubGroup>();
        public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public ICollection<PriceDefinition> PriceDefinitions { get; set; } = new List<PriceDefinition>();

    }
}
