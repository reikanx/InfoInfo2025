using System.Diagnostics;
using InfoInfo2025.Data;
using InfoInfo2025.Models;
using InfoInfo2025.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfoInfo2025.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HomeDataViewModel homeData = new HomeDataViewModel();
            homeData.DisplayCategories = _context.Categories
                .Where(c => c.Display == true && c.Active == true)
                .OrderBy(c => c.Name);

            homeData.Authors = (IEnumerable<AppUser>?)_context.Texts
                .Include(t => t.Author)
                .Select(t => t.Author)
                .Distinct();

            return View(homeData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
