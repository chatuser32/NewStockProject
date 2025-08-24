using App.Repositories;
using App.Repositories.Categories;
using App.Repositories.StockCards;
using App.Repositories.BarcodeCards;
using App.Services.BarcodeCardGeneratorService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace App.Services.StockCardServices
{
    public class StockCardService(
        IStockCardRepository stockcardRepository,
        IBarcodeCardRepository barcodeCardRepository,
        IBarcodeGeneratorService barcodeGeneratorService,
        IUnitOfWork unitOfWork) : IStockCardService
    {
        //public async Task<ServiceResult<List<StockCardDto>>> GetTopPriceASync(int count)
        //{
        //    var stockcards = await stockcardRepository.GetTopPriceProductsAsync(count);

        //    var productsAsDto = stockcards.Select(p => new StockCardDto(p.Id, p.Name)).ToList();

        //    return new ServiceResult<List<StockCardDto>>()
        //    {
        //        Data = productsAsDto
        //    };
        //}
        //public async Task<ServiceResult<StockCardDto?>> GetByIdAsync(int id)
        //{
        //    var stockcard = await stockcardRepository.GetByIdAsync(id);

        //    if (stockcard == null)
        //    {
        //        ServiceResult<StockCardDto>.Fail(errorMessage: "Product not found", HttpStatusCode.NotFound);
        //    }

        //    var productsAsDto = new StockCardDto(stockcard!.Id, stockcard.Name, stockcard.Price);

        //    return ServiceResult<StockCardDto>.Success((StockCardDto)productsAsDto)!;

        //}

        public async Task<ServiceResult<CreateStockCardResponse>> CreateAsync(CreateStockCardRequest request)
        {
            var anyStockCard = await stockcardRepository
                .Where(x => x.CompanyId == request.CompanyId && x.Name == request.Name)
                .AnyAsync();
            if (anyStockCard)
            {
                return ServiceResult<CreateStockCardResponse>.Fail("Bu isimde bir ürün zaten mevcut.", HttpStatusCode.BadRequest);
            }

            var stockcard = new StockCard()
            {
                Name = request.Name,
                Code = request.Code,
                Tax = request.Tax,
                Type = request.Type,
                Unit = request.Unit,
                SubGroupId = request.SubGroupId,
                CompanyId = request.CompanyId,
                MainGroupId = request.MainGroupId,
                BranchId = request.BranchId,
                CategoryId = request.CategoryId,
                UserId = request.UserId,

                // default değerler:
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await stockcardRepository.AddAsync(stockcard);
            await unitOfWork.SaveChangesAsync();

            string? defaultBarcodeCode = null;

            // Eğer varsayılan barkod oluşturulması isteniyorsa
            if (request.CreateDefaultBarcode)
            {
                try
                {
                    // Benzersiz barkod üret
                    defaultBarcodeCode = barcodeGeneratorService.GenerateUniqueBarcode(
                        request.DefaultBarcodeType,
                        stockcard.Id,
                        request.CompanyId,
                        code => barcodeCardRepository.ExistsByBarcodeCodeAsync(code).Result
                    );

                    var barcodeCard = new BarcodeCard
                    {
                        BarcodeCode = defaultBarcodeCode,
                        BarcodeType = request.DefaultBarcodeType,
                        IsDefault = true,
                        StockCardId = stockcard.Id,
                        UserId = request.UserId,
                        BranchId = request.BranchId,
                        CompanyId = request.CompanyId,
                        CreateDate = DateTime.UtcNow
                    };

                    await barcodeCardRepository.AddAsync(barcodeCard);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Barkod oluşturulamazsa warning olarak devam et, stock card'ı geri alma
                    // Log the error if logging is available
                    defaultBarcodeCode = null;
                }
            }

            return ServiceResult<CreateStockCardResponse>.Success(new CreateStockCardResponse(stockcard.Id, defaultBarcodeCode));
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateStockCardRequest request)
        {
            var stockcard = await stockcardRepository.GetByIdAsync(id);

            if (stockcard == null)
            {
                return ServiceResult.Fail("StockCard not found", HttpStatusCode.NotFound);
            }

            stockcard.Name = request.Name;
            stockcard.Code = request.Code;
            stockcard.Tax = request.Tax;
            stockcard.Type = request.Type;
            stockcard.Unit = request.Unit;
            stockcard.SubGroupId = request.SubGroupId;
            stockcard.CompanyId = request.CompanyId;
            stockcard.MainGroupId = request.MainGroupId;
            stockcard.BranchId = request.BranchId;
            stockcard.CategoryId = request.CategoryId;
            stockcard.UserId = request.UserId;
            stockcard.IsActive = request.IsActive;

            stockcardRepository.Update(stockcard);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult<List<StockCardDto>>> GetAllList()
        {
            var stockcards = await stockcardRepository.GetAllWithDetailsAsync();

            var stockcardsAsDto = stockcards.Select(sc => new StockCardDto(
                sc.Name,
                sc.Code,
                sc.Type,
                sc.Unit,
                sc.Tax,
                sc.CreatedDate,
                sc.Company?.Name ?? "Unknown Company",
                sc.Branch?.Name ?? "Unknown Branch",
                sc.MainGroup?.Name ?? "Unknown MainGroup",
                sc.SubGroup?.Name,
                sc.Category?.Name,
                sc.BarcodeCards?.Select(bc => bc.BarcodeCode).ToList() ?? new List<string>()
            )).ToList();

            return ServiceResult<List<StockCardDto>>.Success(stockcardsAsDto);
        }

        public async Task<ServiceResult<List<StockCardDto>>> GetPagedAllListAsync(int pageNumber, int pageSize)
        {
            var stockCardsQuery = stockcardRepository.GetAll();

            var stockCards = await stockCardsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var stockCardAsDto = stockCards.Select(p => new StockCardDto(
                p.Name,
                p.Code,
                p.Type,
                p.Unit,
                p.Tax,
                p.CreatedDate,
                p.Company.Name,
                p.Branch.Name,
                p.MainGroup.Name,
                p.SubGroup?.Name,
                p.Category?.Name,
                p.BarcodeCards.Select(b => b.BarcodeCode).ToList()
            )).ToList();

            return ServiceResult<List<StockCardDto>>.Success(stockCardAsDto);
        }
    }
}