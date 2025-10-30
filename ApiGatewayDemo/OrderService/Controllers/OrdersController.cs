using Microsoft.AspNetCore.Mvc;

namespace Orderservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOrders()
    {
        // 在这里设置断点，调试微服务响应
        var Orders = new[] { new { Id = 1, ProductId = 1 } };
        return Ok(Orders);
    }
}