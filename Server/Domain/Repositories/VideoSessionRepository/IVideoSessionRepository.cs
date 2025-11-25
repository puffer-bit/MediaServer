using Server.Domain.Entities;

namespace Server.Domain.Repositories.VideoSessionRepository;

public interface IVideoSessionRepository
{
    Task<VideoSessionEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VideoSessionEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(VideoSessionEntity instance, CancellationToken cancellationToken = default);
    Task UpdateAsync(VideoSessionEntity instance, CancellationToken cancellationToken = default);
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);
    Task<List<VideoSessionEntity>> GetByCoordinatorIdAsync(string coordinatorInstanceId,
        CancellationToken cancellationToken = default);
}