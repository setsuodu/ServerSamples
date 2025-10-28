using Microsoft.EntityFrameworkCore;
using EFCore.Data;
using EFCore.DTOs;
using EFCore.Exceptions;
using EFCore.Models;

namespace EFCore.Services
{
    public class FriendService : IFriendService
    {
        private readonly GameDbContext _context;
        private readonly ILogger<FriendService> _logger;

        public FriendService(GameDbContext context, ILogger<FriendService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ApplyAsync(long userId, long targetId, string? message)
        {
            if (userId == targetId)
                throw new BusinessException("不能添加自己为好友");

            var exists = await _context.Friends.AnyAsync(f =>
                (f.UserId == userId && f.FriendUserId == targetId) ||
                (f.UserId == targetId && f.FriendUserId == userId));

            if (exists)
                throw new BusinessException("好友关系已存在");

            var target = await _context.Users.AnyAsync(u => u.Id == targetId && u.IsActive && !u.IsBanned);
            if (!target)
                throw new BusinessException("目标用户不存在或被封禁");

            var apply1 = new Friend { UserId = userId, FriendUserId = targetId, Status = FriendStatus.Pending, ApplyMessage = message };
            var apply2 = new Friend { UserId = targetId, FriendUserId = userId, Status = FriendStatus.Pending };

            _context.Friends.AddRange(apply1, apply2);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 向 {TargetId} 发送好友申请", userId, targetId);
        }

        public async Task AcceptAsync(long userId, long friendId)
        {
            var records = await GetBothRecords(userId, friendId);
            foreach (var r in records) r.Status = FriendStatus.Accepted;
            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(long userId, long friendId)
        {
            var records = await GetBothRecords(userId, friendId);
            foreach (var r in records) r.Status = FriendStatus.Rejected;
            await _context.SaveChangesAsync();
        }

        public async Task BlockAsync(long userId, long targetId)
        {
            var records = await GetBothRecords(userId, targetId);
            foreach (var r in records) r.Status = FriendStatus.Blocked;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(long userId, long friendId)
        {
            var records = await GetBothRecords(userId, friendId);
            foreach (var r in records) r.Status = FriendStatus.Deleted;
            await _context.SaveChangesAsync();
        }

        public async Task<FriendListResponse> GetFriendsAsync(long userId)
        {
            var friends = await _context.Friends
                .Where(f => f.UserId == userId && f.Status == FriendStatus.Accepted)
                .Include(f => f.FriendUser)
                .Select(f => new FriendDto
                {
                    UserId = f.FriendUser.Id,
                    Username = f.FriendUser.Username,
                    RemarkName = f.RemarkName,
                    FriendGroup = f.FriendGroup,
                    AddedAt = f.CreatedAt
                })
                .OrderBy(f => f.AddedAt)
                .ToListAsync();

            return new FriendListResponse { Friends = friends, Total = friends.Count };
        }

        public async Task<List<FriendDto>> GetPendingRequestsAsync(long userId)
        {
            return await _context.Friends
                .Where(f => f.FriendUserId == userId && f.Status == FriendStatus.Pending)
                .Include(f => f.User)
                .Select(f => new FriendDto
                {
                    UserId = f.User.Id,
                    Username = f.User.Username,
                    RemarkName = f.ApplyMessage,
                    AddedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<List<Friend>> GetBothRecords(long a, long b)
        {
            var records = await _context.Friends
                .Where(f => (f.UserId == a && f.FriendUserId == b) || (f.UserId == b && f.FriendUserId == a))
                .ToListAsync();

            if (!records.Any())
                throw new BusinessException("好友关系不存在", 404);

            return records;
        }
    }
}