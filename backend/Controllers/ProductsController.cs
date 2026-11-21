using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController(ProductService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? category,
        [FromQuery] StockStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _service.GetListAsync(keyword, category, status, page, pageSize));

    [HttpGet("summary")]
    public async Task<ActionResult<ProductSummary>> GetSummary()
        => Ok(await _service.GetSummaryAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductRequest req)
    {
        try
        {
            var created = await _service.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = $"商品編號 {req.Code} 已存在" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest req)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, req);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = $"商品編號 {req.Code} 已存在" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}