using Server.Domain.Entities;

namespace Server.Domain.Repositories.ServerInstanceRepository;

public interface IOrchestratorInstanceRepository
{
    Task<OrchestratorInstanceEntity?> GetAsync(CancellationToken cancellationToken = default);
    Task SaveOrUpdateAsync(OrchestratorInstanceEntity instance, CancellationToken cancellationToken = default);
}