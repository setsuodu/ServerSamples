using EFCore.DTOs;

namespace EFCore.Services
{
    // Services/IFriendService.cs
    public interface IFriendService
    {
        Task ApplyAsync(long userId, long targetId, string? message);
        Task AcceptAsync(long userId, long friendId);
        Task RejectAsync(long userId, long friendId);
        Task BlockAsync(long userId, long targetId);
        Task RemoveAsync(long userId, long friendId);
        Task<FriendListResponse> GetFriendsAsync(long userId);
        Task<List<FriendDto>> GetPendingRequestsAsync(long userId);
    }
}