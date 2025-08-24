using App.Services.StockCardServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace App.API.Controllers
{
    [Authorize]
    public class StockCardController(IStockCardService stockcardService) : CustomBaseController
    {

        [HttpGet] //---> istek yaparken
        public async Task<IActionResult> GetAll() => CreateActionResult(await stockcardService.GetAllList());
        [HttpGet("{count:int}")]
        //public async Task<IActionResult> GetTopPriceASync() => CreateActionResult(await stockcardService.GetAllList());
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id) => CreateActionResult(await productService.GetByIdAsync(id));
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]//--->eklme yaparken
        public async Task<IActionResult> Create(CreateStockCardRequest request) => CreateActionResult(await stockcardService.CreateAsync(request));
        [Authorize(Roles = "Admin,Editor")]
        [HttpPut("{id:int}")]//---> güncelleme yaparken
        public async Task<IActionResult> Update(int id, UpdateStockCardRequest request) => CreateActionResult(await stockcardService.UpdateAsync(id, request));

        [HttpGet("{pageNumber:int}/{pageSize:int}")]
        public async Task<IActionResult> GetPagedAll (int pageNumber, int pageSize) =>
        CreateActionResult(await stockcardService.GetPagedAllListAsync(pageNumber, pageSize));


    }
}
