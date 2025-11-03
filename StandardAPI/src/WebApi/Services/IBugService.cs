using WebApi.Models;

namespace WebApi.Services;

public interface IBugService
{
    Task<BugResponse> CreateAsync(BugDto dto);
    Task<List<BugResponse>> GetAllAsync();
}
