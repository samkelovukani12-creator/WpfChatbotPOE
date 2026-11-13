using System.Text;
using System.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfChatbotPOE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private string name = "";
        private readonly Random random = new Random();

        // Tracks whether the user is currently playing the quiz
        private bool isQuizActive = false;

        // Keeps track of the user's score
        private int quizScore = 0;

        // Keeps track of the current question
        private int currentQuestionIndex = 0;

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


        private readonly string[,] TipsDatabase = new string[3, 3]
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


        public MainWindow()
        {
            InitializeComponent();
            SoundPlayer player = new SoundPlayer("Startup.wav");
            player.Play();

            AddMessage("Hi! Welcome to Cyber Chat. What's your name?", false);
            txtInput.Focus();
        }


        private class QuizQuestion
        {
            public string Question { get; set; }
            public string[] Options { get; set; }
            public string CorrectAnswer { get; set; }
            public string Explanation { get; set; }

            public QuizQuestion(
                string question,
                string[] options,
                string correctAnswer,
                string explanation)
            {
                Question = question;
                Options = options;
                CorrectAnswer = correctAnswer;
                Explanation = explanation;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = txtInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                AddMessage("Please enter a question or message.", false);
                return;
            }

            AddMessage(userMessage, true);

            string botResponse;

            // Exit the quiz
            if (isQuizActive &&
                userMessage.Equals("quit quiz",
                StringComparison.OrdinalIgnoreCase))
            {
                isQuizActive = false;

                botResponse = "You have exited the quiz. Your final score was " +
                              quizScore + "/" + quizQuestions.Count +
                              ". Type 'quiz' to play again.";
            }

            // Process quiz answers
            else if (isQuizActive)
            {
                botResponse = ProcessQuizAnswer(userMessage);
            }

            // Start the quiz
            else if (userMessage.Equals("quiz",
                     StringComparison.OrdinalIgnoreCase) ||
                     userMessage.Equals("start quiz",
                     StringComparison.OrdinalIgnoreCase))
            {
                botResponse = StartQuiz();
            }

            // Normal chatbot conversation
            else
            {
                botResponse = GetBotResponse(userMessage);

                // Keep your sentiment detection here if already implemented
                string sentiment = DetectSentiment(userMessage);

                if (!string.IsNullOrEmpty(sentiment))
                {
                    botResponse = sentiment +
                                  Environment.NewLine +
                                  Environment.NewLine +
                                  botResponse +
                                  Environment.NewLine +
                                  Environment.NewLine +
                                  "Here is an additional safety tip:" +
                                  Environment.NewLine +
                                  GetRelevantTip(userMessage);
                }
            }

            AddMessage(botResponse, false);

            txtInput.Clear();
            txtInput.Focus();
        }




        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e);
            }
        }

        // This method adds a message to the chat panel, either from the user or the chatbot.

        private void AddMessage(string message, bool isUser)
        {
            Grid messageGrid = new Grid
            {
                Margin = new Thickness(5, 8, 5, 8)
            };

            messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Image profileImage = new Image
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = Stretch.Uniform
            };

            string imagePath = isUser ? "user.jpg" : "chatbot.png";

            try
            {
                profileImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            catch (UriFormatException)
            {

            }

            TextBlock messageText = new TextBlock
            {
                Text = (isUser ? "User: " : "ChatBot: ") + message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Thickness(8),
                Padding = new Thickness(8),
                Background = isUser ? Brushes.LightCyan : Brushes.WhiteSmoke,
                VerticalAlignment = VerticalAlignment.Top
            };

            if (isUser)
            {
                Grid.SetColumn(messageText, 1);
                Grid.SetColumn(profileImage, 2);
                messageGrid.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                Grid.SetColumn(profileImage, 0);
                Grid.SetColumn(messageText, 1);
                messageGrid.HorizontalAlignment = HorizontalAlignment.Left;
            }

            messageGrid.Children.Add(profileImage);
            messageGrid.Children.Add(messageText);

            ChatPanel.Children.Add(messageGrid);

            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }



        // Joins several lines into one multi-line message
        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private string GetBotResponse(string input)
        {
            // The first message is the user's name
            if (string.IsNullOrWhiteSpace(name))
            {
                name = input.Trim();

                return Lines(
                    $"Nice to meet you, {name}!",
                    "I can help you with passwords, phishing, malware, hacking, 2FA, social media, Wi-Fi and more. What would you like to know?");
            }

            string message = input.Trim().ToLower();


            if (message == "exit" || message == "quit" || message == "bye")
            {
                txtInput.IsEnabled = false;
                sendMessage.IsEnabled = false;

                return Lines(
                    $"Goodbye {name}!",
                    "Remember to stay safe online.");
            }


            if (message == "hello" || message == "hi" || message == "hey")
            {
                return $"Hello {name}! How can I help you today?";
            }

            if (message.Contains("how are you"))
            {
                return Lines(
                    "I'm doing great!",
                    "I'm ready to help you with cybersecurity.");
            }

            if (message.Contains("my name"))
            {
                return $"Your name is {name}.";
            }

            // Strong password
            if (message.Contains("strong password") ||
                message.Contains("create a password") ||
                message.Contains("make a password"))
            {
                return Lines(
                    "A strong password should:",
                    "1. Be at least 12–16 characters long.",
                    "2. Use uppercase and lowercase letters.",
                    "3. Include numbers and symbols.",
                    "4. Be unique for every account.",
                    "5. Avoid personal information.");
            }

            // Password
            if (message.Contains("password"))
            {
                int randomIndex = random.Next(0, 3);

                return Lines(
                    "Password security is very important.",
                    "",
                    "Here is a password security tip:",
                    TipsDatabase[0, randomIndex],
                    "",
                    "You can also ask me how to create a strong password.");
            }

            // Identify phishing (must be checked BEFORE the general phishing check)
            if (message.Contains("identify phishing") ||
                message.Contains("signs of phishing") ||
                message.Contains("phishing signs"))
            {
                return Lines(
                    "Some common signs of phishing include:",
                    "- Suspicious links.",
                    "- Urgent or threatening messages.",
                    "- Requests for passwords.",
                    "- Unexpected attachments.",
                    "- Spelling and grammar mistakes.",
                    "- Messages pretending to be from trusted companies.");
            }

            // Phishing
            if (message.Contains("phishing") || message.Contains("phish"))
            {
                int randomIndex = random.Next(0, 3);

                return Lines(
                    "Phishing is a cyberattack where criminals pretend to be a trusted person or organisation to steal sensitive information.",
                    "",
                    "Here is a phishing safety tip:",
                    TipsDatabase[1, randomIndex],
                    "",
                    "You can also ask me how to identify phishing.");
            }

            // Malware
            if (message.Contains("malware") || message.Contains("virus"))
            {
                int randomIndex = random.Next(0, 3);

                return Lines(
                    "Malware is malicious software designed to damage systems, steal information or gain unauthorised access.",
                    "",
                    "Here is a malware safety tip:",
                    TipsDatabase[2, randomIndex]);
            }

            // Hacking
            if (message.Contains("hacking") || message.Contains("hacker"))
            {
                return Lines(
                    "Hacking involves gaining access to a computer system, network or account.",
                    "",
                    "You can protect yourself by:",
                    "- Using strong passwords.",
                    "- Enabling two-factor authentication.",
                    "- Keeping your software updated.",
                    "- Avoiding suspicious links.");
            }

            // 2FA
            if (message.Contains("two factor") ||
                message.Contains("two-factor") ||
                message.Contains("2fa"))
            {
                return Lines(
                    "Two-factor authentication, or 2FA, adds an additional layer of security.",
                    "",
                    "It requires something more than just your password to access your account.",
                    "",
                    "I recommend enabling 2FA on important accounts.");
            }

            // Social media
            if (message.Contains("social media") ||
                message.Contains("instagram") ||
                message.Contains("facebook") ||
                message.Contains("tiktok"))
            {
                return Lines(
                    "To stay safe on social media:",
                    "- Keep your account private where appropriate.",
                    "- Avoid sharing sensitive information.",
                    "- Use strong passwords.",
                    "- Enable two-factor authentication.",
                    "- Be careful when accepting friend requests.",
                    "- Avoid suspicious links.");
            }

            // Wi-Fi
            if (message.Contains("wifi") ||
                message.Contains("wi-fi") ||
                message.Contains("public wi-fi"))
            {
                return Lines(
                    "Public Wi-Fi can be risky.",
                    "",
                    "When using public Wi-Fi:",
                    "- Avoid accessing sensitive accounts.",
                    "- Make sure websites use HTTPS.",
                    "- Keep your firewall enabled.",
                    "- Avoid suspicious Wi-Fi networks.");
            }

            // Antivirus
            if (message.Contains("antivirus") || message.Contains("anti-virus"))
            {
                return Lines(
                    "Antivirus software helps detect, block and remove certain types of malware.",
                    "",
                    "Make sure your antivirus software is updated regularly.");
            }

            // Privacy
            if (message.Contains("privacy") || message.Contains("personal information"))
            {
                return Lines(
                    "Protecting your personal information is very important.",
                    "",
                    "Never share sensitive information such as:",
                    "- Passwords",
                    "- PINs",
                    "- Banking details",
                    "- Identity information",
                    "- Security codes");
            }

            // Software updates
            if (message.Contains("software update") ||
                message.Contains("software updates") ||
                message.Contains("update"))
            {
                return Lines(
                    "Software updates are important because they often contain security fixes.",
                    "",
                    "Keep your operating system, browser and applications updated.");
            }

            // Tips / help
            if (message.Contains("tip") ||
                message.Contains("advice") ||
                message.Contains("help"))
            {
                return GiveRandomTip();
            }

            // Thanks
            if (message.Contains("thank you") || message.Contains("thanks"))
            {
                return Lines(
                    $"You're welcome, {name}!",
                    "I'm always happy to help.");
            }

            // Default
            return Lines(
                "I'm not sure I understand that.",
                "",
                "You can ask me about:",
                "- Passwords",
                "- Phishing",
                "- Malware",
                "- Hacking",
                "- 2FA",
                "- Social media",
                "- Wi-Fi",
                "- Antivirus",
                "- Online privacy",
                "- Software updates");
        }


        private string DetectSentiment(string input)
        {
            string message = input.ToLower();

            // Detect worried sentiment
            if (message.Contains("worried") ||
                message.Contains("scared") ||
                message.Contains("afraid") ||
                message.Contains("anxious") ||
                message.Contains("concerned"))
            {
                return Lines(
                    "I understand that cybersecurity can sometimes feel worrying.",
                    "Don't worry! I'm here to help you stay safe online.",
                    "Let's work through your concern together."
                );
            }

            // Detect curious sentiment
            if (message.Contains("curious") ||
                message.Contains("interested") ||
                message.Contains("wonder") ||
                message.Contains("learn more"))
            {
                return Lines(
                    "That's great! Being curious about cybersecurity is a good thing.",
                    "Learning more about online safety can help you protect yourself.",
                    "I'm happy to help you explore this topic."
                );
            }

            // Detect frustrated sentiment
            if (message.Contains("frustrated") ||
                message.Contains("confused") ||
                message.Contains("annoyed") ||
                message.Contains("difficult") ||
                message.Contains("don't understand"))
            {
                return Lines(
                    "I understand that this can sometimes be frustrating.",
                    "Don't worry! Let's take things one step at a time.",
                    "I'll try to explain things in a simpler way."
                );
            }

            // No sentiment detected
            return "";
        }


        private string StartQuiz()
        {
            isQuizActive = true;

            // Reset the quiz
            quizScore = 0;
            currentQuestionIndex = 0;

            return Lines(
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
                DisplayQuestion()
            );
        }


        private string DisplayQuestion()
        {
            QuizQuestion question =
                quizQuestions[currentQuestionIndex];

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

        private string ProcessQuizAnswer(string userAnswer)
        {
            userAnswer = userAnswer.Trim().ToUpper();

            // Get the current question
            QuizQuestion question =
                quizQuestions[currentQuestionIndex];

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

                feedback = Lines(
                    "Correct! Well done!",
                    question.Explanation
                );
            }
            else
            {
                feedback = Lines(
                    "Incorrect! Don't worry, this is a learning opportunity.",
                    "The correct answer was " + question.CorrectAnswer + ".",
                    question.Explanation
                );
            }

            // Move to the next question
            currentQuestionIndex++;

            // Check whether the quiz has ended
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                isQuizActive = false;

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

            return Lines(
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
                "You can also ask me normal cybersecurity questions."
            );
        }


        private string GetRelevantTip(string input)
        {
            string message = input.ToLower();

            if (message.Contains("password"))
            {
                return TipsDatabase[0, random.Next(0, 3)];
            }

            if (message.Contains("phishing") ||
                message.Contains("scam") ||
                message.Contains("email"))
            {
                return TipsDatabase[1, random.Next(0, 3)];
            }

            if (message.Contains("malware") ||
                message.Contains("virus"))
            {
                return TipsDatabase[2, random.Next(0, 3)];
            }

            // General cybersecurity tip
            return GiveRandomTip();
        }

        // Returns a random tip from the TipsDatabase
        private string GiveRandomTip()
        {
            int category = random.Next(0, 3);
            int tip = random.Next(0, 3);

            string categoryName = category == 0 ? "Password Security"
                                : category == 1 ? "Phishing"
                                : "Malware";

            return Lines(
                $"Here is a random {categoryName} tip:",
                TipsDatabase[category, tip]);
        }
    }
}

        

        