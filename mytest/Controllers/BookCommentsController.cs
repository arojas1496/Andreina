using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mytest.Data;
using mytest.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mytest.Controllers
{
    public class BookCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookCommentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: BookComments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Comments.ToListAsync());
        }

        // GET: BookComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookComment = await _context.Comments
                .SingleOrDefaultAsync(m => m.CommentId == id);
            if (bookComment == null)
            {
                return NotFound();
            }

            return View(bookComment);
        }

        // GET: BookComments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,Comments,ThisDateTime,BookISBN,Rating")] BookComment bookComment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bookComment);
        }

        // GET: BookComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookComment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);
            if (bookComment == null)
            {
                return NotFound();
            }
            return View(bookComment);
        }

        // POST: BookComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Comments,ThisDateTime,BookISBN,Rating")] BookComment bookComment)
        {
            if (id != bookComment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookCommentExists(bookComment.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(bookComment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Add(IFormCollection form)
        {
            var comment = form["Comment"].ToString();
            var bookISBN = form["bookISBN"].ToString();
            var rating = int.Parse(form["Rating"]);
            string userId;
            if (form["Anonymous"].ToString() == "Checked")
                userId = null;
            else
                userId = form["UserId"].ToString();

            BookComment artComment = new BookComment()
            {
                BookISBN = bookISBN,
                Comments = comment,
                Rating = rating,
                ThisDateTime = DateTime.Now,
                UserId = userId
            };

            var ratings = _context.Comments.Where(d => d.BookISBN.Equals(bookISBN)).ToList();
            int ratingSum;
            int ratingCount;
            ratingSum = ratings.Sum(d => d.Rating.Value) + rating;
            ratingCount = ratings.Count()+1;

            var book = _context.Books.Where(x => x.ISBN.Equals(bookISBN)).FirstOrDefault<Book>();
            book.Rating = ratingSum/ratingCount;


            _context.Entry(book).State = EntityState.Modified;
            _context.SaveChanges();

            _context.Comments.Add(artComment);
            _context.SaveChanges();

            return RedirectToAction("ShowBook", "Home", new { field = bookISBN.ToString() });
        }
        // GET: BookComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookComment = await _context.Comments
                .SingleOrDefaultAsync(m => m.CommentId == id);
            if (bookComment == null)
            {
                return NotFound();
            }

            return View(bookComment);
        }

        // POST: BookComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookComment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);
            var bookISBN = bookComment.BookISBN;
            int rating = (int) bookComment.Rating;
            _context.Comments.Remove(bookComment);
            await _context.SaveChangesAsync();


            var ratings = _context.Comments.Where(d => d.BookISBN.Equals(bookISBN)).ToList();
            int ratingSum = ratings.Sum(d => d.Rating.Value);
            int ratingCount = ratings.Count();
            if (ratingCount != 0)
            {
                var book = _context.Books.Where(x => x.ISBN.Equals(bookISBN)).FirstOrDefault<Book>();
                book.Rating = ratingSum / ratingCount;
                _context.Entry(book).State = EntityState.Modified;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private bool BookCommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
