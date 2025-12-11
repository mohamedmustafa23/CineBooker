using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Areas.Admin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class DashboardController : Controller
    {
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IRepository<CinemaHall> _hallRepo;
        private readonly IRepository<Show> _showRepo;
        private readonly IRepository<ShowSeat> _showSeatRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IRepository<Movie> movieRepo,
            IRepository<Booking> bookingRepo,
            IRepository<Cinema> cinemaRepo,
            IRepository<CinemaHall> hallRepo,
            IRepository<Show> showRepo,
            IRepository<ShowSeat> showSeatRepo,
            UserManager<ApplicationUser> userManager)
        {
            _movieRepo = movieRepo;
            _bookingRepo = bookingRepo;
            _cinemaRepo = cinemaRepo;
            _hallRepo = hallRepo;
            _showRepo = showRepo;
            _showSeatRepo = showSeatRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepo.GetAsync();
            var bookings = await _bookingRepo.GetAsync(include: b => b.Include(x => x.Show).ThenInclude(s => s.Movie).Include(x => x.User));
            var cinemas = await _cinemaRepo.GetAsync();
            var halls = await _hallRepo.GetAsync();

            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            var vm = new DashboardVM
            {
                TotalMovies = movies.Count(),
                TotalCinemas = cinemas.Count(),
                TotalHalls = halls.Count(),
                TotalBookings = bookings.Count(),
                TotalRevenue = bookings.Where(b => b.StatusOfPayment == PaymentStatus.Approved).Sum(b => b.Amount),
                ActiveMoviesCount = movies.Count(m => m.IsActive)
            };

            var bookingsThisMonth = bookings.Count(b => b.BookedDate >= startOfThisMonth);
            var bookingsLastMonth = bookings.Count(b => b.BookedDate >= startOfLastMonth && b.BookedDate < startOfThisMonth);
            vm.BookingsGrowth = CalculateGrowth(bookingsThisMonth, bookingsLastMonth);

            var revenueThisMonth = bookings.Where(b => b.BookedDate >= startOfThisMonth && b.StatusOfPayment == PaymentStatus.Approved).Sum(b => b.Amount);
            var revenueLastMonth = bookings.Where(b => b.BookedDate >= startOfLastMonth && b.BookedDate < startOfThisMonth && b.StatusOfPayment == PaymentStatus.Approved).Sum(b => b.Amount);
            vm.RevenueGrowth = CalculateGrowth((double)revenueThisMonth, (double)revenueLastMonth);

            var moviesThisMonth = movies.Count(m => m.CreatedAt >= startOfThisMonth);
            var moviesLastMonth = movies.Count(m => m.CreatedAt >= startOfLastMonth && m.CreatedAt < startOfThisMonth);
            vm.MoviesGrowth = CalculateGrowth(moviesThisMonth, moviesLastMonth);


            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                vm.ChartLabels.Add(date.ToString("ddd")); 
                vm.ChartValues.Add(bookings.Count(b => b.BookedDate.Date == date));
            }

            var activeShowIds = (await _showRepo.GetAsync(s => s.StartTime >= DateTime.Now)).Select(s => s.Id).ToList();

            var allShowSeats = await _showSeatRepo.GetAsync(s => activeShowIds.Contains(s.ShowId));

            vm.SeatsBooked = allShowSeats.Count(s => s.Status == SeatStatus.Booked);
            vm.SeatsLocked = allShowSeats.Count(s => s.Status == SeatStatus.Locked);
            vm.SeatsAvailable = allShowSeats.Count(s => s.Status == SeatStatus.Available);


            // ================== Top Movies Table ==================
            vm.TopMovies = bookings
                .Where(b => b.Show != null)
                .GroupBy(b => b.Show.Movie)
                .Select(g => new TopMovieStat
                {
                    Title = g.Key.Title,
                    PosterUrl = g.Key.PosterUrl,
                    Rating = (double)(g.Key.AverageRating ?? 0),
                    BookingsCount = g.Count(),
                    Revenue = g.Sum(b => b.Amount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();


            // ================== Recent Bookings ==================
            vm.RecentBookings = bookings.OrderByDescending(b => b.BookedDate).Take(5).ToList();


            // ================== Today's Shows ==================
            var todaysShows = await _showRepo.GetAsync(
                s => s.StartTime.Date == DateTime.Today,
                include: s => s.Include(x => x.Movie).Include(x => x.CinemaHall).Include(x => x.ShowSeats)
            );

            vm.TodaysShows = todaysShows.Select(s => new TodaysShowStat
            {
                MovieTitle = s.Movie.Title,
                HallName = s.CinemaHall.Name,
                Time = s.StartTime,
                OccupancyPercentage = s.ShowSeats.Any()
                    ? (int)((double)s.ShowSeats.Count(ss => ss.Status == SeatStatus.Booked) / s.ShowSeats.Count * 100)
                    : 0
            }).OrderBy(s => s.Time).Take(5).ToList();

            vm.SeatsSoldToday = todaysShows.Sum(s => s.ShowSeats.Count(ss => ss.Status == SeatStatus.Booked));


            // ================== Users Stats ==================
            var users = _userManager.Users.ToList();
            vm.AdminCount = 5; 
            vm.CustomerCount = users.Count();

            return View(vm);
        }

        private double CalculateGrowth(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((current - previous) / previous) * 100, 1);
        }
    }
}