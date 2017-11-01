using System.Collections.Generic;

namespace AzTodo.Models
{
	public class TodoList
	{
		public int TodoListId { get; set; }

		public string TodoListName { get; set; }

		public virtual ICollection<ListItem> TodoItems { get; set; }

		public string UserId { get; set; }

		public ApplicationUser User { get; set; }
	}
}