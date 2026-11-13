using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfChatbotPOE
{
    
        /// <summary>
        /// What the user is trying to do, worked out from their sentence.
        /// </summary>
        public enum NlpIntent
        {
            Unknown,
            Greeting,
            HowAreYou,
            MyName,
            Exit,
            ShowLog,
            ShowMore,
            ListTasks,
            CompleteTask,
            DeleteTask,
            SetReminder,
            AddTask,
            StartQuiz
        }

        /// <summary>
        /// The result of analysing one sentence.
        /// </summary>
        public class NlpResult
        {
            public NlpIntent Intent { get; set; }

            // The task / reminder text, e.g. "Update my password"
            public string Title { get; set; }

            // The date found in the sentence (reminders only)
            public DateTime? Date { get; set; }

            // Which task the user means for "complete ..." / "delete ..."
            public string Reference { get; set; }

            // The keyword(s) that triggered the intent
            public string MatchedKeyword { get; set; }

            public NlpResult()
            {
                Intent = NlpIntent.Unknown;
                Title = "";
                Reference = "";
                MatchedKeyword = "";
            }
        }

        /// <summary>
        /// Simulated Natural Language Processing (Task 3).
        /// Uses keyword detection, string manipulation and regular expressions so that
        /// "Can you remind me to update my password?", "Set a reminder: update password"
        /// and "Remind me tomorrow to update my password" all mean the same thing.
        /// </summary>
        public class NlpProcessor
        {
            private const RegexOptions Opts = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            // "please", "can you", "could you please" ... at the start of a sentence
            private const string Polite =
                @"(?:(?:please|kindly)\s+)?(?:(?:can|could|would|will)\s+you\s+)?(?:(?:please|kindly)\s+)?";

            // ---------- Intent patterns (each one lists many ways of saying the same thing) ----------

            private static readonly string ExitPattern =
                @"^\s*(?:exit|quit|bye|goodbye|see\s+you(?:\s+later)?|good\s*night)\W*$";

            private static readonly string GreetingPattern =
                @"^\s*(?:hello|hi|hey|hiya|howzit|good\s+(?:morning|afternoon|evening))(?:\s+there)?\W*$";

            private static readonly string HowAreYouPattern =
                @"\bhow\s+are\s+you\b|\bhow'?s\s+it\s+going\b|\bhow\s+do\s+you\s+do\b";

            private static readonly string MyNamePattern =
                @"\bmy\s+name\b|\bwho\s+am\s+i\b";

            private static readonly string ShowMorePattern =
                @"^\s*show\s+(?:me\s+)?(?:more|all|everything)\W*$|\b(?:full|complete|entire)\s+(?:activity\s+)?(?:log|history)\b";

            private static readonly string ShowLogPattern =
                @"\b(?:activity|action)s?\s+(?:log|history)\b|\b(?:show|view|display|see|open|get|check)\b.*\blog\b|" +
                @"\bwhat\s+(?:have|did)\s+you\s+(?:done|do)\b|\brecent\s+(?:actions|activity|activities)\b|" +
                @"\bwhat\s+have\s+you\s+been\s+(?:up\s+to|doing)\b";

            private static readonly string DeletePattern =
                "^\\s*" + Polite + @"(?:delete|remove|cancel|clear|erase|scrap)\b(?!.*\bquiz\b)";

            private static readonly string CompletePattern =
                "^\\s*" + Polite +
                @"(?:(?:mark|complete|finish|tick(?:\s+off)?|check\s+off)\b|(?:i(?:'ve|\s+have)?\s+)?(?:finished|completed|done\s+with)\b)(?!.*\bquiz\b)";

            private static readonly string SetReminderPattern =
                @"\bremind\s+me\b|\b(?:set|add|create|make|schedule)\s+(?:a\s+|an\s+|another\s+|the\s+)?(?:new\s+)?reminder\b|" +
                @"\bdon'?t\s+(?:let\s+me\s+)?forget\b|\bdo\s+not\s+let\s+me\s+forget\b|\breminder\s+(?:to|for|about)\b|" +
                @"\bnotify\s+me\b|\balert\s+me\b";

            private static readonly string AddTaskPattern =
                @"\b(?:add|create|make|put|log|note|save|insert)\b.*\b(?:tasks?|to-?dos?)\b|" +
                @"\bnew\s+(?:task|to-?do)\b(?=\s*[:\-]|\s+(?:to|for|called|named)\b)|^\s*task\s*[:\-]";

            private static readonly string ListTasksPattern =
                @"\b(?:show|list|view|display|see|check|what(?:'s|\s+is|\s+are)?|which)\b.*\b(?:tasks?|reminders?|to-?dos?)\b|" +
                @"\bmy\s+(?:tasks|reminders|to-?do(?:\s+list)?)\b|\bto-?do\s+list\b";

            private static readonly string QuizPattern =
                @"^(?!.*\b(?:quit|stop|end|exit|cancel)\b).*(?:\bquiz\b|\bmini-?game\b|\btrivia\b|\bplay\s+(?:a\s+)?game\b|\btest\s+(?:me|my\s+knowledge)\b)";

            // ---------- Text that is stripped off to leave just the task / reminder title ----------

            private static readonly string[] LeadingPatterns =
            {
            // "Can you please ..."
            "^\\s*" + Polite,

            // "I'd like you to ...", "I want to ..."
            @"^\s*i(?:'d|\s+would)?\s+(?:like|want|need)\s+(?:you\s+)?to\s+",

            // "Remind me to ...", "Set a reminder to ...", "Don't let me forget to ..."
            @"^\s*(?:(?:set|add|create|make|schedule)\s+(?:a\s+|an\s+|another\s+)?(?:new\s+)?reminder|remind\s+me|" +
            @"don'?t\s+(?:let\s+me\s+)?forget|do\s+not\s+let\s+me\s+forget|notify\s+me|alert\s+me|reminder)" +
            @"\s*(?:to|about|for|that\s+i\s+(?:need|have|must|should)\s+to|that|:|-)?\s*"
        };

            private static readonly string TaskLeadingPattern =
                @"^\s*(?:(?:add|create|make|put|log|note|save|insert)\s+)?(?:(?:a|an|another|the)\s+)?(?:new\s+)?" +
                @"(?:task|to-?do|todo)s?\s*(?:to|for|called|named|about|that|:|-)?\s*";

            private static readonly string TaskVerbPattern =
                @"^\s*(?:add|put|create|make|log|note|save|insert)\s+";

            private static readonly string[] TrailingPatterns =
            {
            @"\s+(?:please|pls|thanks|thank\s+you)\W*$",
            @"\s+as\s+(?:a\s+)?(?:new\s+)?(?:task|reminder|to-?do|todo)$",
            @"\s+(?:on|to|in|onto)\s+(?:my\s+|the\s+)?(?:task|to-?do|todo)(?:\s+list)?$"
        };

            /// <summary>
            /// Works out what the user wants from their sentence.
            /// </summary>
            public NlpResult Analyse(string input, DateTime now)
            {
                NlpResult result = new NlpResult();
                string text = (input ?? "").Trim();
                string keyword;

                if (Test(text, ExitPattern, out keyword)) return Done(result, NlpIntent.Exit, keyword);
                if (Test(text, GreetingPattern, out keyword)) return Done(result, NlpIntent.Greeting, keyword);
                if (Test(text, HowAreYouPattern, out keyword)) return Done(result, NlpIntent.HowAreYou, keyword);
                if (Test(text, MyNamePattern, out keyword)) return Done(result, NlpIntent.MyName, keyword);

                // Activity log (Task 4) - "Show more" is checked first because it is more specific
                if (Test(text, ShowMorePattern, out keyword)) return Done(result, NlpIntent.ShowMore, keyword);
                if (Test(text, ShowLogPattern, out keyword)) return Done(result, NlpIntent.ShowLog, keyword);

                // Managing existing tasks
                if (Test(text, DeletePattern, out keyword))
                {
                    result.Reference = ExtractReference(text);
                    return Done(result, NlpIntent.DeleteTask, keyword);
                }

                if (Test(text, CompletePattern, out keyword))
                {
                    result.Reference = ExtractReference(text);
                    return Done(result, NlpIntent.CompleteTask, keyword);
                }

                // "Remind me to update my password tomorrow"
                if (Test(text, SetReminderPattern, out keyword))
                {
                    DateTime date;
                    string remaining;

                    if (Dateparser.TryExtract(text, now, out date, out remaining))
                    {
                        result.Date = date;
                    }
                    else
                    {
                        remaining = text;
                    }

                    result.Title = CleanTitle(remaining, false);
                    return Done(result, NlpIntent.SetReminder, keyword);
                }

                // "Add a task to enable two-factor authentication"
                if (Test(text, AddTaskPattern, out keyword))
                {
                    result.Title = CleanTitle(text, true);
                    return Done(result, NlpIntent.AddTask, keyword);
                }

                if (Test(text, ListTasksPattern, out keyword)) return Done(result, NlpIntent.ListTasks, keyword);
                if (Test(text, QuizPattern, out keyword)) return Done(result, NlpIntent.StartQuiz, keyword);

                return result;
            }

            /// <summary>
            /// Strips the command words from a sentence, leaving only the task text,
            /// e.g. "Can you remind me to update my password?" becomes "Update my password".
            /// </summary>
            public static string CleanTitle(string text, bool isTask)
            {
                string t = Regex.Replace(text.Trim(), @"\s+", " ");

                foreach (string pattern in LeadingPatterns)
                {
                    t = Regex.Replace(t, pattern, "", Opts);
                }

                if (isTask)
                {
                    string stripped = Regex.Replace(t, TaskLeadingPattern, "", Opts);

                    // "Put enabling 2FA on my to-do list" has no "task" straight after the verb
                    if (stripped == t)
                    {
                        stripped = Regex.Replace(t, TaskVerbPattern, "", Opts);
                    }

                    t = stripped;
                }

                t = TrimPunctuation(t);

                foreach (string pattern in TrailingPatterns)
                {
                    t = Regex.Replace(t, pattern, "", Opts);
                }

                t = TrimPunctuation(t);

                if (t.Length == 0)
                {
                    return "";
                }

                return char.ToUpper(t[0]) + t.Substring(1);
            }

            // Works out which task "complete task 2" / "delete the enable 2FA task" refers to
            private static string ExtractReference(string text)
            {
                string r = Regex.Replace(text.Trim(), @"\s+", " ");

                // The command verb at the start
                r = Regex.Replace(r,
                    "^\\s*" + Polite +
                    @"(?:i(?:'ve|\s+have)?\s+)?(?:mark|completed|complete|finished|finish|done\s+with|tick\s+off|tick|check\s+off|delete|remove|cancel|clear|erase|scrap)\b\s*",
                    "", Opts);

                // Filler words such as "the", "my", "task", "number", "#"
                r = Regex.Replace(r,
                    @"^(?:(?:the|my|this|that|task|reminder|to-?do|todo|item|number|no)\b\.?\s*|#\s*)+",
                    "", Opts);

                // Words at the end such as "as done" or "from my list"
                r = Regex.Replace(r, @"\s+(?:as\s+)?(?:done|complete|completed|finished)\W*$", "", Opts);
                r = Regex.Replace(r, @"\s+from\s+(?:my|the)\s+(?:list|tasks)\W*$", "", Opts);
                r = Regex.Replace(r, @"\s+(?:task|reminder)\W*$", "", Opts);

                return TrimPunctuation(r);
            }

            private static string TrimPunctuation(string text)
            {
                return text.Trim(' ', '.', ',', '!', '?', ';', ':', '-', '"', '\'');
            }

            private static bool Test(string text, string pattern, out string keyword)
            {
                Match match = Regex.Match(text, pattern, Opts);
                keyword = match.Success ? match.Value.Trim() : "";
                return match.Success;
            }

            private static NlpResult Done(NlpResult result, NlpIntent intent, string keyword)
            {
                result.Intent = intent;
                result.MatchedKeyword = keyword;
                return result;
            }
        }
    }


