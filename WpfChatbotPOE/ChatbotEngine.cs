using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WpfChatbotPOE
{
    /// <summary>
    /// The "brain" of the chatbot. It receives the user's text, asks the NLP processor what
    /// the user wants, and passes the work to the right class (tasks, quiz, log, knowledge base).
    /// The window (MainWindow) only deals with the user interface.
    /// </summary>
    public class ChatBotEngine
    {
        private const RegexOptions Opts = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        // What the chatbot is waiting for after asking the user a question
        private enum PendingAction
        {
            None,
            AskReminderYesNo,   // "Would you like to set a reminder for this task?"
            AskReminderDate,    // "When should I remind you?"
            AskReminderTitle,   // "What should I remind you about?"
            AskTaskTitle        // "What task would you like to add?"
        }

        public const string WelcomeMessage = "Hi! Welcome to Cyber Chat. What's your name?";

        private readonly Random random = new Random();
        private readonly ActivityLog activityLog;
        private readonly TipsDatabase tipsDatabase;
        private readonly TaskManager taskManager;
        private readonly QuizManager quizManager;
        private readonly SentimentDetector sentimentDetector;
        private readonly NlpProcessor nlpProcessor;
        private readonly CyberKnowledgeBase knowledgeBase;

        private string userName = "";

        private PendingAction pending = PendingAction.None;
        private TaskItem pendingTask;
        private string pendingTitle = "";
        private DateTime? pendingDate;

        // Set to true when the user says goodbye so the window can lock the input
        public bool ShouldExit { get; private set; }

        public ChatBotEngine()
        {
            activityLog = new ActivityLog();
            tipsDatabase = new TipsDatabase(random);
            taskManager = new TaskManager(activityLog);
            quizManager = new QuizManager(activityLog);
            sentimentDetector = new SentimentDetector();
            nlpProcessor = new NlpProcessor();
            knowledgeBase = new CyberKnowledgeBase(tipsDatabase);
        }

        // ------------------------------------------------------------------
        //  Main entry point
        // ------------------------------------------------------------------

        public string GetResponse(string input)
        {
            string text = input.Trim();

            // The first message is the user's name
            if (string.IsNullOrWhiteSpace(userName))
            {
                return CaptureName(text);
            }

            // While the quiz is running, every message belongs to the quiz
            if (quizManager.IsActive)
            {
                return QuizManager.IsQuitCommand(text)
                    ? quizManager.Quit()
                    : quizManager.ProcessAnswer(text);
            }

            DateTime now = DateTime.Now;
            NlpResult nlp = nlpProcessor.Analyse(text, now);

            // Did the chatbot ask the user a question last time?
            string followUp;
            if (TryHandlePending(text, nlp, now, out followUp))
            {
                return AppendDueReminders(followUp);
            }

            return AppendDueReminders(HandleIntent(nlp, text));
        }

        private string CaptureName(string text)
        {
            string cleaned = Regex.Replace(
                text,
                @"^\s*(?:my\s+name\s+is|i\s+am|i'm|im|it'?s|call\s+me|this\s+is)\s+",
                "", Opts).Trim(' ', '.', '!', ',');

            if (cleaned.Length == 0)
            {
                cleaned = text;
            }

            userName = cleaned;

            return TextHelper.Lines(
                "Nice to meet you, " + userName + "!",
                "I can help you with passwords, phishing, malware, hacking, 2FA, social media, Wi-Fi and more.",
                "You can also add tasks, set reminders, take a quiz or ask to see my activity log. What would you like to do?");
        }

        // ------------------------------------------------------------------
        //  Acting on what the NLP processor understood
        // ------------------------------------------------------------------

        private string HandleIntent(NlpResult nlp, string text)
        {
            switch (nlp.Intent)
            {
                case NlpIntent.Exit:
                    ShouldExit = true;
                    return TextHelper.Lines(
                        "Goodbye " + userName + "!",
                        "Remember to stay safe online.");

                case NlpIntent.Greeting:
                    return "Hello " + userName + "! How can I help you today?";

                case NlpIntent.HowAreYou:
                    return TextHelper.Lines(
                        "I'm doing great!",
                        "I'm ready to help you with cybersecurity.");

                case NlpIntent.MyName:
                    return "Your name is " + userName + ".";

                case NlpIntent.ShowLog:
                    return activityLog.GetSummary(false);

                case NlpIntent.ShowMore:
                    return activityLog.GetSummary(true);

                case NlpIntent.ListTasks:
                    return taskManager.ListTasks();

                case NlpIntent.CompleteTask:
                    return HandleCompleteTask(nlp);

                case NlpIntent.DeleteTask:
                    return HandleDeleteTask(nlp);

                case NlpIntent.SetReminder:
                    return HandleSetReminder(nlp);

                case NlpIntent.AddTask:
                    return HandleAddTask(nlp);

                case NlpIntent.StartQuiz:
                    return quizManager.Start();

                default:
                    return HandleGeneralChat(text);
            }
        }

        // "Add a task to enable two-factor authentication."
        private string HandleAddTask(NlpResult nlp)
        {
            if (string.IsNullOrWhiteSpace(nlp.Title))
            {
                pending = PendingAction.AskTaskTitle;
                return "Sure! What task would you like to add? (for example: \"Enable two-factor authentication\")";
            }

            return CreateTask(nlp.Title);
        }

        private string CreateTask(string title)
        {
            TaskItem existing = taskManager.FindOpenByTitle(title);

            if (existing != null)
            {
                return "'" + existing.Title + "' is already on your task list.";
            }

            TaskItem task = taskManager.AddTask(title);

            // Offer a reminder, as in the example interaction
            pending = PendingAction.AskReminderYesNo;
            pendingTask = task;
            pendingTitle = task.Title;
            pendingDate = null;

            return "Task added: '" + task.Title + "'. Would you like to set a reminder for this task?";
        }

        // "Remind me to update my password tomorrow."
        private string HandleSetReminder(NlpResult nlp)
        {
            if (string.IsNullOrWhiteSpace(nlp.Title))
            {
                pending = PendingAction.AskReminderTitle;
                pendingTask = null;
                pendingDate = nlp.Date;
                return "Sure! What would you like me to remind you about?";
            }

            if (!nlp.Date.HasValue)
            {
                return AskForReminderDate(null, nlp.Title);
            }

            return ApplyReminder(null, nlp.Title, nlp.Date.Value);
        }

        private string AskForReminderDate(TaskItem task, string title)
        {
            pending = PendingAction.AskReminderDate;
            pendingTask = task;
            pendingTitle = title;

            return "When would you like to be reminded about '" + title + "'? " +
                   "(for example: \"tomorrow\", \"in 3 days\" or \"next Monday\")";
        }

        // Saves the reminder - on an existing task, or on a new one
        private string ApplyReminder(TaskItem task, string title, DateTime date)
        {
            if (task != null)
            {
                taskManager.SetReminder(task, date);
            }
            else
            {
                task = taskManager.AddReminder(title, date);
            }

            ClearPending();

            return "Reminder set for '" + task.Title + "' for " +
                   Dateparser.Describe(date, DateTime.Now) + ".";
        }

        private string HandleCompleteTask(NlpResult nlp)
        {
            if (string.IsNullOrWhiteSpace(nlp.Reference))
            {
                return "Which task have you finished? For example: \"complete task 1\".\n\n" + taskManager.ListTasks();
            }

            TaskItem task = taskManager.FindTask(nlp.Reference);

            if (task == null)
            {
                return "I couldn't find a task matching '" + nlp.Reference + "'. Type \"show my tasks\" to see your list.";
            }

            if (task.IsCompleted)
            {
                return "'" + task.Title + "' is already marked as completed.";
            }

            taskManager.CompleteTask(task);
            return "Well done! Task completed: '" + task.Title + "'.";
        }

        private string HandleDeleteTask(NlpResult nlp)
        {
            if (string.IsNullOrWhiteSpace(nlp.Reference))
            {
                return "Which task should I remove? For example: \"delete task 1\".\n\n" + taskManager.ListTasks();
            }

            TaskItem task = taskManager.FindTask(nlp.Reference);

            if (task == null)
            {
                return "I couldn't find a task matching '" + nlp.Reference + "'. Type \"show my tasks\" to see your list.";
            }

            taskManager.DeleteTask(task);
            return "Task removed: '" + task.Title + "'.";
        }

        // ------------------------------------------------------------------
        //  Follow-up questions ("Would you like to set a reminder?")
        // ------------------------------------------------------------------

        private bool TryHandlePending(string text, NlpResult nlp, DateTime now, out string response)
        {
            response = null;

            if (pending == PendingAction.None)
            {
                return false;
            }

            DateTime parsedDate;
            string remaining;
            bool hasDate = Dateparser.TryExtract(text, now, out parsedDate, out remaining);

            bool isNo = Regex.IsMatch(text,
                @"^\s*(?:(?:no|nope|nah|not\s+now)\b|n\W*$|(?:cancel|skip|never\s*mind)\W*$)", Opts);

            bool isYes = Regex.IsMatch(text,
                @"^\s*(?:yes|yeah|yep|yup|y|sure|ok|okay|please\s+do|definitely|of\s+course)\b", Opts);

            // A brand new request (e.g. "Add a task ...") instead of an answer to my question.
            // "Remind me tomorrow" has no title, so it still counts as an answer.
            bool newRequest = nlp.Intent != NlpIntent.Unknown &&
                              !(nlp.Intent == NlpIntent.SetReminder && string.IsNullOrWhiteSpace(nlp.Title));

            if (isNo)
            {
                string title = pendingTitle;
                PendingAction was = pending;
                ClearPending();

                response = was == PendingAction.AskReminderYesNo
                    ? "Okay, no reminder for '" + title + "'."
                    : "Okay, I've cancelled that.";
                return true;
            }

            switch (pending)
            {
                case PendingAction.AskReminderYesNo:
                    if ((isYes || hasDate) && !newRequest)
                    {
                        response = hasDate
                            ? ApplyReminder(pendingTask, pendingTitle, parsedDate)
                            : AskForReminderDate(pendingTask, pendingTitle);
                        return true;
                    }
                    break;

                case PendingAction.AskReminderDate:
                    if (!newRequest)
                    {
                        response = hasDate
                            ? ApplyReminder(pendingTask, pendingTitle, parsedDate)
                            : "I couldn't work out the date. Try \"tomorrow\", \"in 3 days\" or \"next Monday\" (or say \"no\" to cancel).";
                        return true;
                    }
                    break;

                case PendingAction.AskReminderTitle:
                    if (nlp.Intent == NlpIntent.Unknown)
                    {
                        string title = NlpProcessor.CleanTitle(hasDate ? remaining : text, false);
                        DateTime? date = hasDate ? parsedDate : pendingDate;

                        if (title.Length > 0)
                        {
                            response = date.HasValue
                                ? ApplyReminder(null, title, date.Value)
                                : AskForReminderDate(null, title);
                            return true;
                        }
                    }
                    break;

                case PendingAction.AskTaskTitle:
                    if (nlp.Intent == NlpIntent.Unknown)
                    {
                        string title = NlpProcessor.CleanTitle(text, true);

                        if (title.Length > 0)
                        {
                            ClearPending();
                            response = CreateTask(title);
                            return true;
                        }
                    }
                    break;
            }

            // The user moved on to something else
            ClearPending();
            return false;
        }

        private void ClearPending()
        {
            pending = PendingAction.None;
            pendingTask = null;
            pendingTitle = "";
            pendingDate = null;
        }

        // ------------------------------------------------------------------
        //  General conversation (keywords, sentiment, tips)
        // ------------------------------------------------------------------

        private string HandleGeneralChat(string text)
        {
            string sentimentLabel;
            string sentiment = sentimentDetector.Detect(text, out sentimentLabel);

            string reply;
            string topic;
            string keyword;
            bool recognised = knowledgeBase.TryRespond(text, out reply, out topic, out keyword);

            if (recognised)
            {
                activityLog.Add(ActivityCategory.NLP,
                    "Topic recognised: " + topic + " (keyword '" + keyword + "').");
            }

            // Keep the sentiment detection from the original chatbot
            if (!string.IsNullOrEmpty(sentiment))
            {
                activityLog.Add(ActivityCategory.NLP,
                    "Sentiment detected: " + sentimentLabel + ".");

                if (!recognised)
                {
                    reply = "Tell me more, or ask about a topic like passwords, phishing or malware.";
                }

                return sentiment +
                       Environment.NewLine + Environment.NewLine +
                       reply +
                       Environment.NewLine + Environment.NewLine +
                       "Here is an additional safety tip:" +
                       Environment.NewLine +
                       tipsDatabase.GetRelevantTip(text);
            }

            if (recognised)
            {
                // Greet the user by name when they say thanks
                if (topic == "Thanks")
                {
                    reply = "You're welcome, " + userName + "!" + Environment.NewLine + "I'm always happy to help.";
                }

                return reply;
            }

            // Not understood - keep this to a minimum by suggesting things the bot CAN do
            activityLog.Add(ActivityCategory.NLP,
                "Could not interpret: \"" + TextHelper.Shorten(text) + "\".");

            return TextHelper.Lines(
                "I didn't quite understand that. Could you rephrase?",
                "",
                "You can ask me about passwords, phishing, malware, hacking, 2FA, social media, Wi-Fi, antivirus, privacy or software updates.",
                "",
                "You can also say things like:",
                "- \"Add a task to enable 2FA\"",
                "- \"Remind me to update my password tomorrow\"",
                "- \"Show my tasks\"",
                "- \"Start the quiz\"",
                "- \"Show activity log\"");
        }

        // ------------------------------------------------------------------
        //  Reminders that have come due
        // ------------------------------------------------------------------

        private string AppendDueReminders(string response)
        {
            if (quizManager.IsActive)
            {
                return response;
            }

            List<TaskItem> due = taskManager.CollectDueReminders(DateTime.Now);

            if (due.Count == 0)
            {
                return response;
            }

            StringBuilder sb = new StringBuilder(response);
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("Reminder due:");

            foreach (TaskItem task in due)
            {
                sb.AppendLine();
                sb.Append("- " + task.Title);
            }

            return sb.ToString();
        }
    }
}