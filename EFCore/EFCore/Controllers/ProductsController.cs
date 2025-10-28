using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EFCore.Data;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly GameDbContext _context;

    public ProductsController(GameDbContext context)
    {
        _context = context;
    }
    /*
    // GET: api/Products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // POST: api/Products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
    */
}