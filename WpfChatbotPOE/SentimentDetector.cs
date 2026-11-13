namespace WpfChatbotPOE
{
    /// <summary>
    /// Detects simple emotions (worried, curious, frustrated) from keywords
    /// and returns an empathetic message.
    /// </summary>
    public class SentimentDetector
    {
        // Returns the empathetic message ("" if nothing was detected) and the sentiment name.
        public string Detect(string input, out string label)
        {
            string message = input.ToLower();
            label = "";

            // Detect worried sentiment
            if (message.Contains("worried") ||
                message.Contains("scared") ||
                message.Contains("afraid") ||
                message.Contains("anxious") ||
                message.Contains("concerned"))
            {
                label = "worried";
                return TextHelper.Lines(
                    "I understand that cybersecurity can sometimes feel worrying.",
                    "Don't worry! I'm here to help you stay safe online.",
                    "Let's work through your concern together.");
            }

            // Detect curious sentiment
            if (message.Contains("curious") ||
                message.Contains("interested") ||
                message.Contains("wonder") ||
                message.Contains("learn more"))
            {
                label = "curious";
                return TextHelper.Lines(
                    "That's great! Being curious about cybersecurity is a good thing.",
                    "Learning more about online safety can help you protect yourself.",
                    "I'm happy to help you explore this topic.");
            }

            // Detect frustrated sentiment
            if (message.Contains("frustrated") ||
                message.Contains("confused") ||
                message.Contains("annoyed") ||
                message.Contains("difficult") ||
                message.Contains("don't understand"))
            {
                label = "frustrated";
                return TextHelper.Lines(
                    "I understand that this can sometimes be frustrating.",
                    "Don't worry! Let's take things one step at a time.",
                    "I'll try to explain things in a simpler way.");
            }

            // No sentiment detected
            return "";
        }
    }
}