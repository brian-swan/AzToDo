using System.ComponentModel.DataAnnotations;

namespace AzTodo.Models
{
	public class ListItem
	{
		[Key]
		public int ItemId { get; set; }

		public string ItemDescription { get; set; }

		public int TodoListId { get; set; }

		public TodoList TodoList { get; set; }
	}
}
