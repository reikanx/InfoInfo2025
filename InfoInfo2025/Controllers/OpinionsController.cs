using InfoInfo2025.Data;
using InfoInfo2025.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InfoInfo2025.Controllers
{
    public class OpinionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OpinionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Opinions
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                var allOpinions = _context.Opinions.Include(o => o.Author).Include(o => o.Text);
                return View(await allOpinions.ToListAsync());
            }
            else
            {
                var filteredOpinions = _context.Opinions.Include(o => o.Author).Include(o => o.Text).Where(o => o.TextId == id);
                return View(await filteredOpinions.ToListAsync());
            }
        }


        // GET: Opinions/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opinion = await _context.Opinions
                .Include(o => o.Author)
                .Include(o => o.Text)
                .FirstOrDefaultAsync(m => m.OpinionId == id);
            if (opinion == null)
            {
                return NotFound();
            }

            return View(opinion);
        }

        // GET: Opinions/Create
        [Authorize]
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Text text = _context.Texts.Find(id);
            if (text == null)
            {
                return BadRequest();
            }

            var opinion = new Opinion
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                TextId = text.TextId
            };

            ViewData["TextTitle"] = text.Title;

            return View(opinion);
        }

        // POST: Opinions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OpinionId,Comment,Rating,TextId,UserId")] Opinion opinion)
        {
            if (ModelState.IsValid)
            {
                opinion.AddedDate = DateTime.Now;
                _context.Add(opinion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Texts", new { id = opinion.TextId }, "comments");
            }

            ViewData["TextTitle"] = opinion.Text?.Title;
            opinion.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(opinion);
        }

        // POST: Opinions/_CreatePartial
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial([Bind("OpinionId,Comment,Rating,TextId,UserId")] Opinion opinion)
        {
            if (ModelState.IsValid)
            {
                opinion.AddedDate = DateTime.Now;
                _context.Add(opinion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Texts", new { id = opinion.TextId }, "comments");
            }

            ViewData["TextTitle"] = opinion.Text?.Title;
            opinion.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return RedirectToAction("Details", "Texts", new { id = opinion.TextId }, "comments");
        }

        // GET: Opinions/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var opinion = await _context.Opinions.FindAsync(id);
            if (opinion == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", opinion.UserId);
            ViewData["TextId"] = new SelectList(_context.Texts, "TextId", "Content", opinion.TextId);
            return View(opinion);
        }

        // POST: Opinions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OpinionId,Comment,AddedDate,Rating,TextId,UserId")] Opinion opinion)
        {
            if (id != opinion.OpinionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opinion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpinionExists(opinion.OpinionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", opinion.UserId);
            ViewData["TextId"] = new SelectList(_context.Texts, "TextId", "Content", opinion.TextId);
            return View(opinion);
        }

        // GET: Opinions/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opinion = await _context.Opinions
                .Include(o => o.Author)
                .Include(o => o.Text)
                .FirstOrDefaultAsync(m => m.OpinionId == id);
            if (opinion == null)
            {
                return NotFound();
            }

            return View(opinion);
        }

        // POST: Opinions/Delete/5
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opinion = await _context.Opinions.FindAsync(id);
            if (opinion != null)
            {
                _context.Opinions.Remove(opinion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpinionExists(int id)
        {
            return _context.Opinions.Any(e => e.OpinionId == id);
        }
    }
}
