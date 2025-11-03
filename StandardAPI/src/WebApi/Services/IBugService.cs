using BugService.Models;

namespace BugService.Services;

public interface IBugService
{
    Task<BugResponse> CreateAsync(BugDto dto);
    Task<List<BugResponse>> GetAllAsync();
}
