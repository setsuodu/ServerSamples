using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public PlayerController(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayerAsync(string id)
    {
        string key = $"player:{id}";

        // 1️⃣ 从 Redis 获取
        var json = await _db.StringGetAsync(key);
        PlayerData player;

        if (json.IsNullOrEmpty)
        {
            // 2️⃣ 如果没有，就生成默认数据并存入 Redis
            player = new PlayerData
            {
                Id = id,
                Name = "Player_" + id,
                Level = 1,
                Gold = 100
            };

            await _db.StringSetAsync(key, JsonSerializer.Serialize(player));
        }
        else
        {
            player = JsonSerializer.Deserialize<PlayerData>(json!);
        }

        return Ok(player);
    }
}

public class PlayerData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public int Gold { get; set; }
}
