using Microsoft.EntityFrameworkCore;

namespace CineBooker.Repositories
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        private readonly ApplicationDbContext _context;
        public GenreRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllWithMoviesAsync(CancellationToken cancellationToken)
        {
            return await _context.Genres
                .Include(g => g.MovieGenres)      
                .ThenInclude(mg => mg.Movie)      
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
