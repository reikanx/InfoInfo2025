using InfoInfo2025.Data;
using InfoInfo2025.Models;
using InfoInfo2025.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InfoInfo2025.Controllers
{
    public class TextsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TextsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Texts
        public async Task<IActionResult> Index(string Fraza, string Autor, int? Kategoria, string Klucz, int PageNumber = 1)
        {
            TextIndexViewModel textIndexViewModel = new()
            {
                TextList = new()
            };

            var SelectedTexts = _context.Texts
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Where(t => t.Active == true)
                .OrderByDescending(t => t.AddedDate);

            if (Kategoria != null)
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts
                    .Where(r => r.Category.CategoryId == Kategoria);
            }

            if (!String.IsNullOrEmpty(Autor))
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts
                    .Where (r => r.Author.Id == Autor);
            }

            if (!String.IsNullOrEmpty(Fraza))
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts
                    .Where(r => r.Content.Contains(Fraza));
            }

            if (!String.IsNullOrEmpty(Klucz))
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts
                .Where(r => r.Keywords.Contains(Klucz));
            }

            textIndexViewModel.TextList.TextCount = SelectedTexts.Count();

            textIndexViewModel.TextList.PageNumber = PageNumber;

            textIndexViewModel.TextList.Author = Autor;
            textIndexViewModel.TextList.Phrase = Fraza;
            textIndexViewModel.TextList.Category = Kategoria;
            textIndexViewModel.TextList.Keyword = Klucz;

            textIndexViewModel.Texts = await SelectedTexts
                .Skip((PageNumber - 1) * textIndexViewModel.TextList.PageSize)
                .Take(textIndexViewModel.TextList.PageSize)
                .ToListAsync();

            ViewData["Category"] = new SelectList(_context.Categories
                .Where(c => c.Active == true)
                .OrderBy (c => c.Name),
                "CategoryId", "Name", Kategoria);

            ViewData["Author"] = new SelectList(_context.Texts
                .Include(u => u.Author)
                .Select(u => u.Author)
                .Distinct(),
                "Id", "FullName", Autor);

            var tags = _context.Texts
                .AsEnumerable()
                .Where(t => !string.IsNullOrEmpty(t.Keywords))
                .SelectMany(t => t.Keywords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(k => k.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k)
                .ToList();

            ViewData["Keyword"] = tags;

            return View(textIndexViewModel);
        }

        // GET: List of Texts
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> List()
        {
            var applicationDbContext = _context.Texts.Include(t => t.Author).Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Texts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var text = await _context.Texts
                .Include(t => t.Author)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TextId == id);
            if (text == null)
            {
                return NotFound();
            }

            return View(text);
        }

        [Authorize(Roles = "admin,author")]
        // GET: Texts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Description");
            return View();
        }

        [Authorize(Roles = "admin,author")]
        // POST: Texts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TextId,Title,Summary,Keywords,Content,Graphic,Active,AddedDate,CategoryId,UserId")] Text text)
        {
            if (ModelState.IsValid)
            {
                _context.Add(text);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", text.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Description", text.CategoryId);
            return View(text);
        }

        [Authorize(Roles = "admin,author")]
        // GET: Texts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var text = await _context.Texts.FindAsync(id);
            if (text == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", text.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Description", text.CategoryId);
            return View(text);
        }

        [Authorize(Roles = "admin,author")]
        // POST: Texts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TextId,Title,Summary,Keywords,Content,Graphic,Active,AddedDate,CategoryId,UserId")] Text text)
        {
            if (id != text.TextId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(text);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TextExists(text.TextId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", text.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Description", text.CategoryId);
            return View(text);
        }

        [Authorize(Roles = "admin,author")]
        // GET: Texts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var text = await _context.Texts
                .Include(t => t.Author)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TextId == id);
            if (text == null)
            {
                return NotFound();
            }

            return View(text);
        }

        [Authorize(Roles = "admin,author")]
        // POST: Texts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var text = await _context.Texts.FindAsync(id);
            if (text != null)
            {
                _context.Texts.Remove(text);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        private bool TextExists(int id)
        {
            return _context.Texts.Any(e => e.TextId == id);
        }
    }
}
