using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatbotPOE
{
    public class TaskItem
    {
        public int Id { get; private set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsCompleted { get; set; }

        // Null means no reminder has been set
        public DateTime? ReminderDate { get; set; }

        // True once the user has been told that the reminder is due
        public bool ReminderNotified { get; set; }

        public TaskItem(int id, string title)
        {
            Id = id;
            Title = title;
            CreatedAt = DateTime.Now;
            IsCompleted = false;
        }
    }
}

