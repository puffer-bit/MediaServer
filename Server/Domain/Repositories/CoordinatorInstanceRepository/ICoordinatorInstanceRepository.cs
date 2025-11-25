using Server.Domain.Entities;

namespace Server.Domain.Repositories.CoordinatorInstanceRepository;

public interface ICoordinatorInstanceRepository
{
    Task<CoordinatorInstanceEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CoordinatorInstanceEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CoordinatorInstanceEntity instance, CancellationToken cancellationToken = default);
    Task UpdateAsync(CoordinatorInstanceEntity instance, CancellationToken cancellationToken = default);
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);
}