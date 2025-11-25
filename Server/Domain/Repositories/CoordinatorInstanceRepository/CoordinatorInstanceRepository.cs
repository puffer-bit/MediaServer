using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Domain.Repositories.CoordinatorInstanceRepository;

public class CoordinatorInstanceRepository : ICoordinatorInstanceRepository
{
    private readonly ServerDbContext _context;

    public CoordinatorInstanceRepository(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<CoordinatorInstanceEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.CoordinatorInstances
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CoordinatorInstanceEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CoordinatorInstances
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CoordinatorInstanceEntity instance, CancellationToken cancellationToken = default)
    {
        await _context.CoordinatorInstances.AddAsync(instance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CoordinatorInstanceEntity instance, CancellationToken cancellationToken = default)
    {
        _context.CoordinatorInstances.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            _context.CoordinatorInstances.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}