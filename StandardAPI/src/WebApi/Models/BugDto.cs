namespace WebApi.Models;

public record BugDto(string Title, string Description);
public record BugResponse(int Id, string Title, string Description, DateTime CreatedAt);