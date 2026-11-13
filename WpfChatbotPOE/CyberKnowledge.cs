using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WpfChatbotPOE
{
    /// <summary>
    /// Answers general cybersecurity questions using keyword detection.
    /// Each topic accepts several spellings / phrasings (e.g. "2FA", "two-factor", "two step").
    /// </summary>
    public class CyberKnowledgeBase
    {
        private const RegexOptions Opts = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private readonly TipsDatabase tips;

        public CyberKnowledgeBase(TipsDatabase tips)
        {
            this.tips = tips;
        }

        /// <summary>
        /// Looks for a cybersecurity topic in the message.
        /// Returns false when nothing was recognised.
        /// </summary>
        public bool TryRespond(string input, out string response, out string topic, out string keyword)
        {
            string message = input.Trim();
            response = null;
            topic = null;

            // Strong password
            if (Test(message, @"strong\s+password|(?:create|make|choose|generate|build|pick|set)\s+(?:a\s+)?(?:good\s+|secure\s+|safe\s+)?password", out keyword))
            {
                topic = "Strong passwords";
                response = TextHelper.Lines(
                    "A strong password should:",
                    "1. Be at least 12–16 characters long.",
                    "2. Use uppercase and lowercase letters.",
                    "3. Include numbers and symbols.",
                    "4. Be unique for every account.",
                    "5. Avoid personal information.");
                return true;
            }

            // Password
            if (Test(message, @"pass\s?words?|passphrase|credentials", out keyword))
            {
                topic = "Passwords";
                response = TextHelper.Lines(
                    "Password security is very important.",
                    "",
                    "Here is a password security tip:",
                    tips.GetTip(TipCategory.Password),
                    "",
                    "You can also ask me how to create a strong password.");
                return true;
            }

            // Identify phishing (must be checked BEFORE the general phishing check)
            if (Test(message, @"identify\s+phishing|signs?\s+of\s+(?:a\s+)?phishing|phishing\s+signs?|(?:spot|recogni[sz]e|detect)\s+(?:a\s+)?(?:phishing|scam)", out keyword))
            {
                topic = "Identifying phishing";
                response = TextHelper.Lines(
                    "Some common signs of phishing include:",
                    "- Suspicious links.",
                    "- Urgent or threatening messages.",
                    "- Requests for passwords.",
                    "- Unexpected attachments.",
                    "- Spelling and grammar mistakes.",
                    "- Messages pretending to be from trusted companies.");
                return true;
            }

            // Phishing
            if (Test(message, @"phish\w*|\bscams?\b|\bfraud\w*|suspicious\s+(?:email|link|message)s?", out keyword))
            {
                topic = "Phishing";
                response = TextHelper.Lines(
                    "Phishing is a cyberattack where criminals pretend to be a trusted person or organisation to steal sensitive information.",
                    "",
                    "Here is a phishing safety tip:",
                    tips.GetTip(TipCategory.Phishing),
                    "",
                    "You can also ask me how to identify phishing.");
                return true;
            }

            // Antivirus (checked BEFORE malware, because "antivirus" contains the word "virus")
            if (Test(message, @"anti[\s-]?virus", out keyword))
            {
                topic = "Antivirus";
                response = TextHelper.Lines(
                    "Antivirus software helps detect, block and remove certain types of malware.",
                    "",
                    "Make sure your antivirus software is updated regularly.");
                return true;
            }

            // Ransomware
            if (Test(message, @"ransomware", out keyword))
            {
                topic = "Ransomware";
                response = TextHelper.Lines(
                    "Ransomware is malware that locks your files and demands payment to unlock them.",
                    "",
                    "Here is a safety tip:",
                    tips.GetTip(TipCategory.Malware),
                    "",
                    "Regular offline backups are the best protection.");
                return true;
            }

            // Malware
            if (Test(message, @"\bmalware\b|\bviruses\b|\bvirus\b|\btrojans?\b|\bspyware\b|\badware\b", out keyword))
            {
                topic = "Malware";
                response = TextHelper.Lines(
                    "Malware is malicious software designed to damage systems, steal information or gain unauthorised access.",
                    "",
                    "Here is a malware safety tip:",
                    tips.GetTip(TipCategory.Malware));
                return true;
            }

            // Hacking
            if (Test(message, @"\bhack\w*|cyber\s?attack", out keyword))
            {
                topic = "Hacking";
                response = TextHelper.Lines(
                    "Hacking involves gaining access to a computer system, network or account.",
                    "",
                    "You can protect yourself by:",
                    "- Using strong passwords.",
                    "- Enabling two-factor authentication.",
                    "- Keeping your software updated.",
                    "- Avoiding suspicious links.");
                return true;
            }

            // 2FA
            if (Test(message, @"two[\s-]?factor|2[\s-]?fa\b|\bmfa\b|multi[\s-]?factor|two[\s-]?step|2[\s-]?step", out keyword))
            {
                topic = "Two-factor authentication";
                response = TextHelper.Lines(
                    "Two-factor authentication, or 2FA, adds an additional layer of security.",
                    "",
                    "It requires something more than just your password to access your account.",
                    "",
                    "I recommend enabling 2FA on important accounts.");
                return true;
            }

            // Social media
            if (Test(message, @"social\s+media|instagram|facebook|tiktok|twitter|whatsapp|snapchat|linkedin", out keyword))
            {
                topic = "Social media";
                response = TextHelper.Lines(
                    "To stay safe on social media:",
                    "- Keep your account private where appropriate.",
                    "- Avoid sharing sensitive information.",
                    "- Use strong passwords.",
                    "- Enable two-factor authentication.",
                    "- Be careful when accepting friend requests.",
                    "- Avoid suspicious links.");
                return true;
            }

            // Wi-Fi
            if (Test(message, @"wi[\s-]?fi|hotspot", out keyword))
            {
                topic = "Wi-Fi";
                response = TextHelper.Lines(
                    "Public Wi-Fi can be risky.",
                    "",
                    "When using public Wi-Fi:",
                    "- Avoid accessing sensitive accounts.",
                    "- Make sure websites use HTTPS.",
                    "- Keep your firewall enabled.",
                    "- Avoid suspicious Wi-Fi networks.");
                return true;
            }

            // VPN
            if (Test(message, @"\bvpn\b|virtual\s+private\s+network", out keyword))
            {
                topic = "VPN";
                response = TextHelper.Lines(
                    "A VPN (Virtual Private Network) encrypts your internet traffic, which helps protect you on public Wi-Fi.",
                    "",
                    "Choose a trusted provider - a VPN does not replace strong passwords or 2FA.");
                return true;
            }

            // Firewall
            if (Test(message, @"firewall", out keyword))
            {
                topic = "Firewall";
                response = TextHelper.Lines(
                    "A firewall monitors network traffic and blocks suspicious connections.",
                    "",
                    "Keep your operating system's firewall switched on.");
                return true;
            }

            // Backups
            if (Test(message, @"back[\s-]?ups?\b|backing\s+up", out keyword))
            {
                topic = "Backups";
                response = TextHelper.Lines(
                    "Regular backups let you recover your files after a device failure or a ransomware attack.",
                    "",
                    "Keep at least one backup copy offline or in a separate location.");
                return true;
            }

            // Privacy
            if (Test(message, @"privacy|personal\s+(?:information|data|details)|identity\s+theft", out keyword))
            {
                topic = "Privacy";
                response = TextHelper.Lines(
                    "Protecting your personal information is very important.",
                    "",
                    "Never share sensitive information such as:",
                    "- Passwords",
                    "- PINs",
                    "- Banking details",
                    "- Identity information",
                    "- Security codes");
                return true;
            }

            // Software updates
            if (Test(message, @"software\s+updates?|\bupdates?\b|\bupdating\b|\bpatch(?:es)?\b", out keyword))
            {
                topic = "Software updates";
                response = TextHelper.Lines(
                    "Software updates are important because they often contain security fixes.",
                    "",
                    "Keep your operating system, browser and applications updated.");
                return true;
            }

            // Thanks
            if (Test(message, @"thank\s*you|thanks|\bthx\b|cheers", out keyword))
            {
                topic = "Thanks";
                response = TextHelper.Lines(
                    "You're welcome!",
                    "I'm always happy to help.");
                return true;
            }

            // Help - what the chatbot can do
            if (Test(message, @"^\s*(?:help|help\s+me|commands|options|menu)\W*$|what\s+can\s+you\s+do", out keyword))
            {
                topic = "Help";
                response = TextHelper.Lines(
                    "Here is what I can do:",
                    "",
                    "Cybersecurity topics: passwords, phishing, malware, hacking, 2FA, social media, Wi-Fi, antivirus, privacy and software updates.",
                    "",
                    "Tasks and reminders:",
                    "- \"Add a task to enable 2FA\"",
                    "- \"Remind me to update my password tomorrow\"",
                    "- \"Show my tasks\"",
                    "- \"Complete task 1\" or \"Delete task 1\"",
                    "",
                    "Other:",
                    "- \"Quiz\" to play the cybersecurity mini-game",
                    "- \"Show activity log\" or \"What have you done for me?\"",
                    "- \"Give me a tip\" for a random safety tip");
                return true;
            }

            // Tips / advice
            if (Test(message, @"\btips?\b|\badvice\b|\bhelp\b|\bsuggestions?\b", out keyword))
            {
                topic = "Safety tips";
                response = tips.GetRandomTip();
                return true;
            }

            return false;
        }

        private static bool Test(string message, string pattern, out string keyword)
        {
            Match match = Regex.Match(message, pattern, Opts);
            keyword = match.Success ? match.Value.Trim() : "";
            return match.Success;
        }
    }
}
