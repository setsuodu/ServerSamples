using Microsoft.EntityFrameworkCore;
using BugService.Data;
using BugService.Models;

namespace BugService.Services;

public class BugService : IBugService
{
    private readonly AppDbContext _db;

    public BugService(AppDbContext db) => _db = db;

    public async Task<BugResponse> CreateAsync(BugDto dto)
    {
        var bug = new Bug { Title = dto.Title, Description = dto.Description };
        _db.Bugs.Add(bug);
        await _db.SaveChangesAsync();
        return new BugResponse(bug.Id, bug.Title, bug.Description, bug.CreatedAt);
    }

    public async Task<List<BugResponse>> GetAllAsync()
    {
        return await _db.Bugs
            .Select(b => new BugResponse(b.Id, b.Title, b.Description, b.CreatedAt))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
}