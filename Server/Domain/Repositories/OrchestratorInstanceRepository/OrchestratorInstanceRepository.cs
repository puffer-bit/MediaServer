using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Domain.Repositories.ServerInstanceRepository;

public class OrchestratorInstanceRepository : IOrchestratorInstanceRepository
{
    private readonly ServerDbContext _context;

    public OrchestratorInstanceRepository(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<OrchestratorInstanceEntity?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ServerInstance
            .SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
    }
    
    public async Task SaveOrUpdateAsync(OrchestratorInstanceEntity instance, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ServerInstance
            .FirstOrDefaultAsync(x => x.Id == instance.Id, cancellationToken);

        if (existing == null)
        {
            await _context.ServerInstance.AddAsync(instance, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(instance);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}