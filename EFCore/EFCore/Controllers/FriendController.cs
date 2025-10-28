using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EFCore.DTOs;
using EFCore.Services;

namespace EFCore.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;
        private long CurrentUserId => long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost("apply/{targetId}")]
        public async Task<IActionResult> Apply(long targetId, [FromBody] FriendApplyRequest req)
        {
            await _friendService.ApplyAsync(CurrentUserId, targetId, req.Message);
            return Ok(new { message = "申请已发送" });
        }

        [HttpPost("accept/{friendId}")]
        public async Task<IActionResult> Accept(long friendId)
        {
            await _friendService.AcceptAsync(CurrentUserId, friendId);
            return Ok(new { message = "已添加好友" });
        }

        [HttpPost("reject/{friendId}")]
        public async Task<IActionResult> Reject(long friendId)
        {
            await _friendService.RejectAsync(CurrentUserId, friendId);
            return Ok(new { message = "已拒绝" });
        }

        [HttpPost("block/{targetId}")]
        public async Task<IActionResult> Block(long targetId)
        {
            await _friendService.BlockAsync(CurrentUserId, targetId);
            return Ok(new { message = "已拉黑" });
        }

        [HttpPost("remove/{friendId}")]
        public async Task<IActionResult> Remove(long friendId)
        {
            await _friendService.RemoveAsync(CurrentUserId, friendId);
            return Ok(new { message = "已删除好友" });
        }

        [HttpGet]
        public async Task<ActionResult<FriendListResponse>> GetFriends()
        {
            var result = await _friendService.GetFriendsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<List<FriendDto>>> GetPending()
        {
            var pending = await _friendService.GetPendingRequestsAsync(CurrentUserId);
            return Ok(pending);
        }
    }
}