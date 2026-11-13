using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatbotPOE
{
    
        public enum ActivityCategory
        {
            Task,
            Reminder,
            Quiz,
            NLP,
            General
        }

        /// <summary>
        /// One line in the activity log: when it happened, what type it was and a short description.
        /// </summary>
        public class ActivityLogEntry
        {
            public DateTime Timestamp { get; private set; }
            public ActivityCategory Category { get; private set; }
            public string Description { get; private set; }

            public ActivityLogEntry(ActivityCategory category, string description)
            {
                Timestamp = DateTime.Now;
                Category = category;
                Description = description;
            }

            public override string ToString()
            {
                return "[" + Timestamp.ToString("dd MMM HH:mm", CultureInfo.InvariantCulture) + "] " + Description;
            }
        }

        /// <summary>
        /// Stores every significant action the chatbot has taken and formats the
        /// "Show activity log" / "What have you done for me?" output.
        /// </summary>
        public class ActivityLog
        {
            // How many entries are shown by default (the task asks for 5 - 10)
            public const int DefaultDisplayCount = 5;

            private readonly List<ActivityLogEntry> entries = new List<ActivityLogEntry>();

            public int Count
            {
                get { return entries.Count; }
            }

            // Adds a new action to the log
            public void Add(ActivityCategory category, string description)
            {
                entries.Add(new ActivityLogEntry(category, description));
            }

            // Builds the text shown to the user. showAll = true is the "Show more" option.
            public string GetSummary(bool showAll)
            {
                if (entries.Count == 0)
                {
                    return "I haven't done anything for you yet. Try adding a task or setting a reminder!";
                }

                int take = showAll ? entries.Count : Math.Min(DefaultDisplayCount, entries.Count);
                List<ActivityLogEntry> recent = entries.Skip(entries.Count - take).ToList();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(showAll
                    ? "Here's the full history of actions:"
                    : "Here's a summary of recent actions:");

                for (int i = 0; i < recent.Count; i++)
                {
                    sb.AppendLine((i + 1) + ". " + recent[i]);
                }

                if (!showAll && entries.Count > take)
                {
                    sb.AppendLine();
                    sb.Append("Showing the last " + take + " of " + entries.Count +
                              " actions. Type 'show more' to see the full history.");
                }

                return sb.ToString().TrimEnd();
            }
        }
    }


