using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Domain.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly ServerDbContext _context;

    public UserRepository(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.UserEntities
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.UserEntities
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserEntity instance, CancellationToken cancellationToken = default)
    {
        await _context.UserEntities.AddAsync(instance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserEntity instance, CancellationToken cancellationToken = default)
    {
        _context.UserEntities.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            _context.UserEntities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}