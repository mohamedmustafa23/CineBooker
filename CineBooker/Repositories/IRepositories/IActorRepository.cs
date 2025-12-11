using System.Linq.Expressions;

namespace CineBooker.Repositories.IRepositories
{
    public interface IActorRepository : IRepository<Actor>
    {
        Task<IEnumerable<Actor>> GetAllWithMoviesAsync(CancellationToken cancellationToken = default);
        Task<Actor?> GetActorWithMoviesAsync(int id, CancellationToken cancellationToken);
    }
}
