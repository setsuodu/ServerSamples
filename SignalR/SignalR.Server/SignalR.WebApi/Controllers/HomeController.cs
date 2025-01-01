using Microsoft.AspNetCore.Mvc;

namespace Game.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        /// <summary>
        /// 初始化构造函数
        /// </summary>
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Get请求
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object GetData()
        {
            return new
            {
                Id = 123,
                Name = "VinCente"
            };
        }
        /// <summary>
        /// Post请求
        /// 新增数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public object PostData()
        {
            return new
            {
                Success = true,
                Message = "新增操作成功"
            };
        }
        /// <summary>
        /// Put请求
        /// 修改数据
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public object PutDate()
        {
            return new
            {
                Success = true,
                Message = "修改操作成功"
            };
        }
        /// <summary>
        /// Delete请求
        /// 删除数据
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public object DeleteDate()
        {
            return new
            {
                Success = true,
                Message = "删除操作成功"
            };
        }
    }
}