using Server.Domain.Entities;

namespace Server.Domain.Repositories.UserRepository;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserEntity instance, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserEntity instance, CancellationToken cancellationToken = default);
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);
}