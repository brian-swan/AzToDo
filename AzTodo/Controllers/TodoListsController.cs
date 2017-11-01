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
	public class TodoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
		private string _userId;

		public TodoListsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
			_userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
		}

        // GET: TodoLists
        public async Task<IActionResult> Index()
        {
			var todoLists = _context.Lists.Where(u => u.UserId == _userId);
            return View(await todoLists.ToListAsync());
        }

        // GET: TodoLists/Details/5
        public IActionResult Details(int? id)
        {
			return RedirectToAction("ShowItems", "TodoLists", new { id = id});
        }

        // GET: TodoLists/Create
        public IActionResult Create()
        {
			ViewData["UserId"] = _userId;
            return View();
        }

        // POST: TodoLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TodoListId,TodoListName,UserId")] TodoList todoList)
        {
			if (ModelState.IsValid)
			{
				_context.Add(todoList);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["UserId"] = todoList.UserId;
			return RedirectToAction("ShowItems", "TodoLists", todoList.TodoListId);
        }

		// GET: TodoLists/Show/3
		public async Task<IActionResult> ShowItems(int id)
		{
			var list = _context.Lists.Where(l => l.TodoListId == id && l.UserId == _userId).FirstOrDefault();
			if (list == null)
				return NotFound("No such list.");
			var items = _context.Items.Where(i => i.TodoListId == list.TodoListId);
			ViewBag.ListName = list.TodoListName;
			ViewBag.ListId = id;
			return View(await items.ToListAsync());
		}

		// GET: TodoLists/Edit/5
		public async Task<IActionResult> Edit(int? id)
        {
            var todoList = await _context.Lists.Where(l => l.UserId == _userId).SingleOrDefaultAsync(m => m.TodoListId == id);
			if (id == null)
			{
				return NotFound("No such list.");
			}

			if (todoList == null)
            {
                return NotFound();
            }
			ViewData["UserId"] = _userId;
            return View(todoList);
        }

        // POST: TodoLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TodoListId,TodoListName,UserId")] TodoList todoList)
        {
            if (id != todoList.TodoListId)
            {
                return NotFound("No such list.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todoList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoListExists(todoList.TodoListId))
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
			ViewData["UserId"] = todoList.UserId;
            return View(todoList);
        }

        // GET: TodoLists/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var todoList = await _context.Lists
				.Where(l => l.UserId == _userId)
                .SingleOrDefaultAsync(m => m.TodoListId == id);
            if (todoList == null)
                return NotFound("No such list");

            return View(todoList);
        }

        // POST: TodoLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			var todoList = await _context.Lists
				.Where(l => l.UserId == _userId)
				.SingleOrDefaultAsync(m => m.TodoListId == id);
			if (todoList == null)
				return NotFound("No such list");

			_context.Lists.Remove(todoList);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoListExists(int id)
        {
            return _context.Lists.Any(e => e.TodoListId == id);
        }
    }
}
