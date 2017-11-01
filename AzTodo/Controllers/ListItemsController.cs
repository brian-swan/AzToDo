using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AzTodo.Data;
using AzTodo.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AzTodo.Controllers
{
	public class ListItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
		private string _userId;

		public ListItemsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
			_userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
		}

        // GET: ListItems
        public IActionResult Index()
        {
			return RedirectToAction("Index", "TodoLists");
		}
		
		// GET: ListItems/Create?listId=3
		public async Task<IActionResult> Create(int listId)
        {
			var list = await _context.Lists.Where(l => l.UserId == _userId && l.TodoListId == listId).FirstOrDefaultAsync();
			if (list == null)
				return NotFound("No such list.");
			ViewData["TodoListId"] = listId; 
			return View();
        }

        // POST: ListItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,ItemDescription,TodoListId")] ListItem listItem)
        {
			var list = await _context.Lists.Where(l => l.UserId == _userId && l.TodoListId == listItem.TodoListId).FirstOrDefaultAsync();
			if (list == null)
				return NotFound("No such list.");
			if (ModelState.IsValid)
			{
				_context.Add(listItem);
				await _context.SaveChangesAsync();
				return RedirectToAction("ShowItems", "TodoLists", new { id = listItem.TodoListId });
			}
			ViewData["TodoListId"] = listItem.TodoListId;
            return View(listItem);
		}

        // GET: ListItems/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
			var list = await _context.Items.Where(i => i.ItemId == id).
				Include(l => l.TodoList).
				Where(l => l.TodoList.UserId == _userId).FirstOrDefaultAsync();
			if (list == null)
				return NotFound("No such list.");

			var listItem = await _context.Items.SingleOrDefaultAsync(m => m.ItemId == id);
            if (listItem == null)
            {
                return NotFound();
            }
			
			ViewData["TodoListId"] = list.TodoListId;
            return View(listItem);
        }

        // POST: ListItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemId,ItemDescription,TodoListId")] ListItem listItem)
        {
            if (id != listItem.ItemId)
            {
                return NotFound("No such item.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListItemExists(listItem.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
				return RedirectToAction("ShowItems", "TodoLists", new { id = listItem.TodoListId });
			}
			ViewData["TodoListId"] = listItem.TodoListId;
			return View(listItem);
        }

        // GET: ListItems/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
			var listItem = await _context.Items
                .Include(l => l.TodoList)
				.Where(l => l.TodoList.UserId == _userId)
				.SingleOrDefaultAsync(m => m.ItemId == id);
            if (listItem == null)
            {
                return NotFound("No such Item.");
            }

            return View(listItem);
        }

        // POST: ListItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			var listItem = await _context.Items
				.Include(l => l.TodoList)
				.Where(l => l.TodoList.UserId == _userId)
				.SingleOrDefaultAsync(m => m.ItemId == id);
			if (listItem == null)
			{
				return NotFound("No such Item.");
			}
			_context.Items.Remove(listItem);
            await _context.SaveChangesAsync();
			return RedirectToAction("ShowItems", "TodoLists", new { id = listItem.TodoListId });
		}

        private bool ListItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }
    }
}
