using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WpfChatbotPOE
{
    /// <summary>
    /// Runs the cybersecurity mini-game: stores the questions, tracks the score
    /// and records quiz activity in the activity log.
    /// </summary>
    public class QuizManager
    {
        private readonly ActivityLog log;

        // Stores all the quiz questions
        private readonly List<QuizQuestion> quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new string[] {
                    "Reply with your password",
                    "Delete the email",
                    "Report the email as phishing",
                    "Ignore it"
                },
                "C",
                "Reporting phishing emails helps prevent scams."
            ),

            new QuizQuestion(
                "What is the recommended minimum length for a strong password?",
                new string[] {
                    "4 characters",
                    "6 characters",
                    "8 characters",
                    "At least 12 characters"
                },
                "D",
                "Longer passwords are generally harder for attackers to guess."
            ),

            new QuizQuestion(
                "True or False: You should use the same password for all your accounts.",
                new string[] {
                    "True",
                    "False"
                },
                "B",
                "You should use unique passwords to prevent one compromised account from exposing others."
            ),

            new QuizQuestion(
                "What is phishing?",
                new string[] {
                    "A computer game",
                    "An attempt to steal information using fake messages",
                    "A type of antivirus",
                    "A method of backing up files"
                },
                "B",
                "Phishing uses deceptive messages to trick people into revealing sensitive information."
            ),

            new QuizQuestion(
                "What should you do before clicking an unfamiliar link?",
                new string[] {
                    "Click it immediately",
                    "Share it with friends",
                    "Check where the link leads",
                    "Enter your password"
                },
                "C",
                "Always check links carefully before opening them."
            ),

            new QuizQuestion(
                "True or False: Antivirus software can help protect your computer against malware.",
                new string[] {
                    "True",
                    "False"
                },
                "A",
                "Antivirus software can detect and block many types of malicious software."
            ),

            new QuizQuestion(
                "What does 2FA stand for?",
                new string[] {
                    "Two-Factor Authentication",
                    "Two File Applications",
                    "Two Firewall Access",
                    "Two-Folder Authentication"
                },
                "A",
                "Two-factor authentication adds another layer of security to your accounts."
            ),

            new QuizQuestion(
                "What is social engineering?",
                new string[] {
                    "Building social media websites",
                    "Manipulating people into revealing confidential information",
                    "Installing computer software",
                    "Creating strong passwords"
                },
                "B",
                "Social engineering tricks people into sharing information or performing unsafe actions."
            ),

            new QuizQuestion(
                "True or False: Public Wi-Fi networks are always safe for online banking.",
                new string[] {
                    "True",
                    "False"
                },
                "B",
                "Public Wi-Fi can be insecure. Avoid accessing sensitive accounts on untrusted networks."
            ),

            new QuizQuestion(
                "Why should you regularly update your operating system?",
                new string[] {
                    "To delete your files",
                    "To make passwords shorter",
                    "To fix security vulnerabilities",
                    "To disable antivirus software"
                },
                "C",
                "Software updates often contain important security patches."
            ),

            new QuizQuestion(
                "What should you do if someone asks you for your banking OTP?",
                new string[] {
                    "Share it immediately",
                    "Post it online",
                    "Never share it with anyone",
                    "Send it through email"
                },
                "C",
                "Never share your one-time PIN or verification code. Criminals can use it to access your accounts."
            ),

            new QuizQuestion(
                "True or False: Regularly backing up important files helps protect against data loss.",
                new string[] {
                    "True",
                    "False"
                },
                "A",
                "Backups help you recover important information if your device is damaged or affected by ransomware."
            )
        };

        // Keeps track of the user's score
        private int quizScore = 0;

        // Keeps track of the current question
        private int currentQuestionIndex = 0;

        // Tracks whether the user is currently playing the quiz
        public bool IsActive { get; private set; }

        public QuizManager(ActivityLog log)
        {
            this.log = log;
        }

        // True when the user types something like "quit quiz"
        public static bool IsQuitCommand(string input)
        {
            return Regex.IsMatch(
                input,
                @"^\s*(?:quit|exit|stop|end|cancel)\s+(?:the\s+)?(?:quiz|game)\s*$",
                RegexOptions.IgnoreCase);
        }

        public string Start()
        {
            IsActive = true;

            // Reset the quiz
            quizScore = 0;
            currentQuestionIndex = 0;

            log.Add(ActivityCategory.Quiz,
                "Quiz started (" + quizQuestions.Count + " questions).");

            return TextHelper.Lines(
                "Welcome to the Cybersecurity Mini-Game!",
                "",
                "Test your cybersecurity knowledge!",
                "There are " + quizQuestions.Count + " questions.",
                "Some questions are multiple-choice and others are True/False.",
                "",
                "Type A, B, C or D to submit your answer.",
                "For True/False questions, type A for True or B for False.",
                "You can type 'quit quiz' at any time to exit.",
                "",
                DisplayQuestion());
        }

        // Exit the quiz early
        public string Quit()
        {
            IsActive = false;

            log.Add(ActivityCategory.Quiz,
                "Quiz exited - " + currentQuestionIndex + " question(s) answered, score " +
                quizScore + "/" + quizQuestions.Count + ".");

            return "You have exited the quiz. Your final score was " +
                   quizScore + "/" + quizQuestions.Count +
                   ". Type 'quiz' to play again.";
        }

        private string DisplayQuestion()
        {
            QuizQuestion question = quizQuestions[currentQuestionIndex];

            string result = "Question " +
                            (currentQuestionIndex + 1) +
                            " of " + quizQuestions.Count +
                            Environment.NewLine +
                            Environment.NewLine +
                            question.Question +
                            Environment.NewLine;

            string[] letters = { "A", "B", "C", "D" };

            for (int i = 0; i < question.Options.Length; i++)
            {
                result += Environment.NewLine +
                          letters[i] + ") " +
                          question.Options[i];
            }

            result += Environment.NewLine +
                      Environment.NewLine +
                      "Enter your answer:";

            return result;
        }

        public string ProcessAnswer(string userAnswer)
        {
            userAnswer = userAnswer.Trim().ToUpper();

            // Get the current question
            QuizQuestion question = quizQuestions[currentQuestionIndex];

            // Validate the user's answer
            string[] validAnswers =
                question.Options.Length == 2
                ? new string[] { "A", "B" }
                : new string[] { "A", "B", "C", "D" };

            if (!validAnswers.Contains(userAnswer))
            {
                return "Please enter a valid answer: " +
                       string.Join(", ", validAnswers) +
                       ".";
            }

            string feedback;

            // Check if the answer is correct
            if (userAnswer == question.CorrectAnswer)
            {
                quizScore++;

                feedback = TextHelper.Lines(
                    "Correct! Well done!",
                    question.Explanation);
            }
            else
            {
                feedback = TextHelper.Lines(
                    "Incorrect! Don't worry, this is a learning opportunity.",
                    "The correct answer was " + question.CorrectAnswer + ".",
                    question.Explanation);
            }

            // Move to the next question
            currentQuestionIndex++;

            // Check whether the quiz has ended
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                IsActive = false;

                log.Add(ActivityCategory.Quiz,
                    "Quiz completed - " + currentQuestionIndex + " questions answered, final score " +
                    quizScore + "/" + quizQuestions.Count + ".");

                return feedback +
                       Environment.NewLine +
                       Environment.NewLine +
                       FinishQuiz();
            }

            // Display the next question
            return feedback +
                   Environment.NewLine +
                   Environment.NewLine +
                   "Your current score: " +
                   quizScore + "/" + quizQuestions.Count +
                   Environment.NewLine +
                   Environment.NewLine +
                   DisplayQuestion();
        }

        private string FinishQuiz()
        {
            string feedback;

            if (quizScore >= 10)
            {
                feedback = "Excellent work! You're a cybersecurity pro!";
            }
            else if (quizScore >= 7)
            {
                feedback = "Great job! You have good cybersecurity knowledge.";
            }
            else if (quizScore >= 4)
            {
                feedback = "Good effort! Keep learning to improve your skills.";
            }
            else
            {
                feedback = "Keep learning to stay safe online! Practice makes perfect.";
            }

            return TextHelper.Lines(
                "================================",
                "CYBERSECURITY QUIZ COMPLETED!",
                "================================",
                "",
                "Your final score: " +
                quizScore + "/" + quizQuestions.Count,
                "",
                feedback,
                "",
                "Thank you for playing!",
                "Type 'quiz' if you want to play again.",
                "You can also ask me normal cybersecurity questions.");
        }
    }
}