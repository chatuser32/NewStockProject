using App.Repositories.StockCards;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace App.Services.StockCardServices
{
    public class CreateStockCardRequestValidator : AbstractValidator<CreateStockCardRequest>
    {
        private readonly IStockCardRepository stockCardRepository;

        public CreateStockCardRequestValidator(IStockCardRepository stockCardRepository)
        {
            this.stockCardRepository = stockCardRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün ismi gereklidir.")
                .MinimumLength(3).WithMessage("Ürün ismi en az 3 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Ürün ismi en fazla 100 karakter olabilir.")
                .MustAsync(async (request, name, ct) =>
                    !await stockCardRepository
                        .Where(sc => sc.CompanyId == request.CompanyId && sc.Name == name)
                        .AnyAsync(ct))
                .WithMessage("Aynı şirket içerisinde bu ürün ismi zaten mevcut.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Ürün kodu gereklidir.")
                .MinimumLength(2).WithMessage("Ürün kodu en az 2 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Ürün kodu en fazla 50 karakter olabilir.")
                .Matches("^[A-Za-z0-9_-]+$").WithMessage("Ürün kodu yalnızca harf, rakam, alt çizgi ve tire içerebilir.")
                .MustAsync(async (request, code, ct) =>
                    !await stockCardRepository
                        .Where(sc => sc.CompanyId == request.CompanyId && sc.Code == code)
                        .AnyAsync(ct))
                .WithMessage("Aynı şirket içerisinde bu ürün kodu zaten mevcut.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Geçersiz ürün tipi.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Birim gereklidir.")
                .MaximumLength(20).WithMessage("Birim en fazla 20 karakter olabilir.");

            RuleFor(x => x.Tax)
                .InclusiveBetween(0, 100).WithMessage("Vergi yüzdesi 0 ile 100 arasında olmalıdır.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("CompanyId pozitif olmalıdır.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId pozitif olmalıdır.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("BranchId pozitif olmalıdır.");

            RuleFor(x => x.MainGroupId)
                .GreaterThan(0).WithMessage("MainGroupId pozitif olmalıdır.");

            When(x => x.SubGroupId.HasValue, () =>
            {
                RuleFor(x => x.SubGroupId!.Value)
                    .GreaterThan(0).WithMessage("SubGroupId pozitif olmalıdır.");
            });

            When(x => x.CategoryId.HasValue, () =>
            {
                RuleFor(x => x.CategoryId!.Value)
                    .GreaterThan(0).WithMessage("CategoryId pozitif olmalıdır.");
            });

            When(x => x.CreateDefaultBarcode, () =>
            {
                RuleFor(x => x.DefaultBarcodeType)
                    .IsInEnum().WithMessage("Geçersiz varsayılan barkod tipi.");
            });
        }
    }
}
