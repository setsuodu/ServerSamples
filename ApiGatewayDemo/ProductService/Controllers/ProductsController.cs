using Microsoft.AspNetCore.Mvc;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // 在这里设置断点，调试微服务响应
        var products = new[] { new { Id = 1, Name = "Laptop" }, new { Id = 2, Name = "Phone" } };
        return Ok(products);
    }
}