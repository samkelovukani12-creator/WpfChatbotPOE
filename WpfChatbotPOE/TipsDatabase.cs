using System;

namespace WpfChatbotPOE
{
    public enum TipCategory
    {
        Password = 0,
        Phishing = 1,
        Malware = 2
    }

    /// <summary>
    /// Holds the cybersecurity tips (2D array: category x tip) and hands them out at random.
    /// </summary>
    public class TipsDatabase
    {
        private readonly Random random;

        private readonly string[,] tips = new string[3, 3]
        {
            {
                "Use a password manager so you don't have to remember every password.",
                "Never reuse the same password on different accounts.",
                "Change your password immediately if a service reports a breach."
            },
            {
                "Check the sender's email address before clicking any link.",
                "Banks and companies will never ask for your password by email.",
                "Hover over links to see where they really lead before you click."
            },
            {
                "Only download software from official websites and app stores.",
                "Keep your antivirus and operating system updated.",
                "Back up important files regularly in case of ransomware."
            }
        };

        public TipsDatabase(Random random)
        {
            this.random = random;
        }

        // A random tip from one category
        public string GetTip(TipCategory category)
        {
            return tips[(int)category, random.Next(0, tips.GetLength(1))];
        }

        // Picks the category that best matches the user's message
        public string GetRelevantTip(string input)
        {
            string message = input.ToLower();

            if (message.Contains("password"))
            {
                return GetTip(TipCategory.Password);
            }

            if (message.Contains("phishing") ||
                message.Contains("scam") ||
                message.Contains("email"))
            {
                return GetTip(TipCategory.Phishing);
            }

            if (message.Contains("malware") ||
                message.Contains("virus"))
            {
                return GetTip(TipCategory.Malware);
            }

            // General cybersecurity tip
            return GetRandomTip();
        }

        // A random tip from a random category, with a heading
        public string GetRandomTip()
        {
            int category = random.Next(0, tips.GetLength(0));
            int tip = random.Next(0, tips.GetLength(1));

            string categoryName = category == 0 ? "Password Security"
                                : category == 1 ? "Phishing"
                                : "Malware";

            return TextHelper.Lines(
                "Here is a random " + categoryName + " tip:",
                tips[category, tip]);
        }
    }
}