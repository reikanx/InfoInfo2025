using InfoInfo2025.Data;
using InfoInfo2025.Infrastructure;
using InfoInfo2025.Models;
using InfoInfo2025.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Claims;

namespace InfoInfo2025.Controllers
{
    public class TextsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;

        public TextsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
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

            var selectedText = await _context.Texts
                .Include(t => t.Author)
                .Include(t => t.Category)
                .Include(t => t.Opinions)
                .ThenInclude(c => c.Author)
                .Where(t => t.Active == true && t.TextId == id)
                .FirstOrDefaultAsync();

            if (selectedText == null)
            {
                return NotFound();
            }

            TextWithOpinions textWithOpinions = new()
            {
                SelectedText = selectedText,
                ReadingTime = (int)Math.Ceiling((double)selectedText.Content.Length / 1400),
                CommentsCount = selectedText.Opinions.Count,
                NewOpinion = new()
                {
                    TextId = selectedText.TextId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Text = selectedText
                }
            };

            textWithOpinions.RatingsCount = textWithOpinions.CommentsCount > 0 ? selectedText.Opinions.Count(x => x.Rating != null) : 0;
            textWithOpinions.AverageRating = textWithOpinions.RatingsCount > 0 ? (float)selectedText.Opinions.Where(x => x.Rating != null).Average(x => (int)x.Rating) : 0f;
            textWithOpinions.Description = Variety.Phrase("komentarz", "komentarze", "komentarzy", textWithOpinions.CommentsCount);

            return View(textWithOpinions);
        }

        [Authorize(Roles = "admin,author")]
        [HttpGet]
        // GET: Texts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }


        // POST: Texts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,author")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TextId,Title,Summary,Keywords,Content,Active,CategoryId")] Text text, IFormFile? picture)
        {
            if (ModelState.IsValid)
            {
                if (picture != null && picture.Length > 0)
                {
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(picture.ContentType))
                    {
                        ModelState.AddModelError("Graphic", "Niedozwolony typ pliku");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId",
                        "Name", text.CategoryId);
                        return View(text);
                    }
                    else
                    {
                        ImageFileUpload imageFileResult = new(_hostEnvironment);
                        FileSendResult fileSendResult = imageFileResult.SendFile(picture, "img", 600);

                        if (fileSendResult.Success)
                        {
                            text.Graphic = fileSendResult.Name;
                        }
                        else
                        {
                            ModelState.AddModelError("Graphic", fileSendResult.Error);
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", text.CategoryId);
                            return View(text);
                        }
                    }
                }

                text.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                text.AddedDate = DateTime.Now;
                _context.Add(text);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", text.CategoryId);
            return View(text);
        }


        // GET: Texts/Edit/5
        [Authorize(Roles = "admin,author")]
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

            if (string.Compare(User.FindFirstValue(ClaimTypes.NameIdentifier), text.UserId) == 0 || User.IsInRole("admin"))
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", text.CategoryId);
                ViewData["Author"] = text.UserId;
                return View(text);

            }
            else
            {
                return RedirectToAction(nameof(Index));
            }

            
        }

        // POST: Texts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize(Roles = "admin,author")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int textid, [Bind("TextId,Title,Summary,Keywords,Content,Graphic,Active,AddedDate,CategoryId,UserId")] InfoInfo2025.Models.Text text, IFormFile? picture)
        {
            if (textid != text.TextId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (picture != null && picture.Length > 0)
                {
                    ImageFileUpload imageFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = imageFileResult.SendFile(picture, "img", 600);
                    if (fileSendResult.Success)
                    {
                        text.Graphic = fileSendResult.Name;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Wybrany plik nie jest obrazkiem!";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", text.CategoryId);
                        ViewData["Author"] = text.UserId;
                        return View(text);
                    }
                }

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
            ViewData["Author"] = text.UserId;
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", text.CategoryId);
            return View(text);
        }

        [Authorize(Roles = "admin")]
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

        [Authorize(Roles = "admin")]
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
