using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Domain.Repositories.VideoSessionRepository;

public class VideoSessionRepository : IVideoSessionRepository
{
    private readonly ServerDbContext _context;

    public VideoSessionRepository(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<VideoSessionEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.VideoSessionsEntities
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<VideoSessionEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.VideoSessionsEntities
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(VideoSessionEntity instance, CancellationToken cancellationToken = default)
    {
        await _context.VideoSessionsEntities.AddAsync(instance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(VideoSessionEntity instance, CancellationToken cancellationToken = default)
    {
        _context.VideoSessionsEntities.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            _context.VideoSessionsEntities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<VideoSessionEntity>> GetByCoordinatorIdAsync(string coordinatorInstanceId, CancellationToken cancellationToken = default)
    {
        return await _context.VideoSessionsEntities
            .Where(s => s.CoordinatorInstanceId == coordinatorInstanceId)
            .ToListAsync(cancellationToken);
    }
}