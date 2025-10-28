namespace EFCore.DTOs
{
    public class FriendDto
    {
        public long UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? RemarkName { get; set; }
        public string FriendGroup { get; set; } = "默认分组";
        public DateTime AddedAt { get; set; }
    }
    public class FriendApplyRequest
    {
        public string? Message { get; set; }
    }
    public class FriendListResponse
    {
        public List<FriendDto> Friends { get; set; } = new();
        public int Total { get; set; }
    }
}