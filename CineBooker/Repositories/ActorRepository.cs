using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CineBooker.Repositories
{
    public class ActorRepository : Repository<Actor>, IActorRepository
    {
        private readonly ApplicationDbContext _context;

        public ActorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Actor>> GetAllWithMoviesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Actors
                .Include(g => g.MovieActors)
                .ThenInclude(mg => mg.Movie)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Actor?> GetActorWithMoviesAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Actors
                .Where(a => a.Id == id)
                .Include(a => a.MovieActors)      
                .ThenInclude(ma => ma.Movie)     
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
