namespace CineBooker.Repositories.IRepositories
{
    public interface IGenreRepository : IRepository<Genre>
    {
        Task<IEnumerable<Genre>> GetAllWithMoviesAsync(CancellationToken cancellationToken = default);                
    }
}
