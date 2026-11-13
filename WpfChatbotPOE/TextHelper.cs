using System;

namespace WpfChatbotPOE
{
    /// <summary>
    /// Small helper used by every class that builds multi-line chatbot messages.
    /// </summary>
    public static class TextHelper
    {
        // Joins several lines into one multi-line message
        public static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        // Shortens long user input so it fits neatly in the activity log
        public static string Shorten(string text, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength) + "...";
        }
    }
}
