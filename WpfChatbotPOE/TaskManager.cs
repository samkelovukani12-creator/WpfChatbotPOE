using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static WpfChatbotPOE.DateParser;

namespace WpfChatbotPOE
{

        public class TaskManager
        { 

        private readonly List<TaskItem> tasks = new List<TaskItem>();
        private readonly ActivityLog log;
        private int nextId = 1;

        public TaskManager(ActivityLog log)
        {
            this.log = log;
        }

        // Finds an unfinished task with exactly this title (ignores upper/lower case)
        public TaskItem FindOpenByTitle(string title)
        {
            return tasks.FirstOrDefault(t =>
                !t.IsCompleted &&
                string.Equals(t.Title, title, StringComparison.OrdinalIgnoreCase));
        }

        // Adds a new task
        public TaskItem AddTask(string title)
        {
            TaskItem task = new TaskItem(nextId++, title);
            tasks.Add(task);

            log.Add(ActivityCategory.Task,
                "Task added: '" + task.Title + "' (no reminder set).");

            return task;
        }

        // Sets (or changes) the reminder of an existing task
        public void SetReminder(TaskItem task, DateTime date)
        {
            task.ReminderDate = date;

            // If it is already due the user is told straight away, so don't repeat it
            task.ReminderNotified = date.Date <= DateTime.Today;

            log.Add(ActivityCategory.Reminder,
                "Reminder set for '" + task.Title + "' - due " +
                DateParser.Describe(date, DateTime.Now) + ".");
        }

        // "Remind me to ..." - re-uses a matching open task or creates a new one
        public TaskItem AddReminder(string title, DateTime date)
        {
            TaskItem task = FindOpenByTitle(title);

            if (task == null)
            {
                task = new TaskItem(nextId++, title);
                tasks.Add(task);
            }

            SetReminder(task, date);
            return task;
        }

        public void CompleteTask(TaskItem task)
        {
            task.IsCompleted = true;
            log.Add(ActivityCategory.Task, "Task completed: '" + task.Title + "'.");
        }

        public void DeleteTask(TaskItem task)
        {
            tasks.Remove(task);
            log.Add(ActivityCategory.Task, "Task removed: '" + task.Title + "'.");
        }

        // Finds a task from text like "2", "task 2" or "enable 2FA"
        public TaskItem FindTask(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            string text = reference.Trim().ToLower();

            // A task number
            Match numberMatch = Regex.Match(text, @"^#?\s*(\d+)$");
            if (numberMatch.Success)
            {
                int id;
                if (int.TryParse(numberMatch.Groups[1].Value, out id))
                {
                    return tasks.FirstOrDefault(t => t.Id == id);
                }

                return null;
            }

            // Title contains the text, or the text contains the title
            TaskItem found =
                tasks.FirstOrDefault(t => t.Title.ToLower().Contains(text)) ??
                tasks.FirstOrDefault(t => text.Contains(t.Title.ToLower()));

            if (found != null)
            {
                return found;
            }

            // Every word the user typed appears in the title ("update password")
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return tasks.FirstOrDefault(t => words.All(w => t.Title.ToLower().Contains(w)));
        }

        // Builds the "show my tasks" text
        public string ListTasks()
        {
            if (tasks.Count == 0)
            {
                return "You don't have any tasks yet. Try: \"Add a task to enable two-factor authentication\".";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here are your tasks:");

            foreach (TaskItem task in tasks)
            {
                string status = task.IsCompleted ? "[x]" : "[ ]";

                string reminder = task.ReminderDate.HasValue
                    ? "reminder: " + DateParser.Describe(task.ReminderDate.Value, DateTime.Now)
                    : "no reminder";

                sb.AppendLine(task.Id + ". " + status + " " + task.Title + " (" + reminder + ")");
            }

            return sb.ToString().TrimEnd();
        }

        // Returns reminders that have come due and have not been announced yet
        public List<TaskItem> CollectDueReminders(DateTime now)
        {
            List<TaskItem> due = tasks
                .Where(t => !t.IsCompleted &&
                            t.ReminderDate.HasValue &&
                            !t.ReminderNotified &&
                            t.ReminderDate.Value.Date <= now.Date)
                .ToList();

            foreach (TaskItem task in due)
            {
                task.ReminderNotified = true;
                log.Add(ActivityCategory.Reminder, "Reminder due: '" + task.Title + "'.");
            }

            return due;
        }
    }
}

    

