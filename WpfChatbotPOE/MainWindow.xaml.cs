using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfChatbotPOE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// This class only handles the user interface. All chatbot logic lives in ChatBotEngine
    /// and the classes it uses (NlpProcessor, TaskManager, QuizManager, ActivityLog ...).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ChatBotEngine chatBot = new ChatBotEngine();

        public MainWindow()
        {
            InitializeComponent();

            PlayStartupSound();

            // Lets the user press Enter to send a message
            txtInput.KeyDown += txtInput_KeyDown;

            AddMessage(ChatBotEngine.WelcomeMessage, false);
            txtInput.Focus();
        }

        private void PlayStartupSound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("Startup.wav");
                player.Play();
            }
            catch (Exception)
            {
                // The chatbot still works if the sound file is missing
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

            string botResponse = chatBot.GetResponse(userMessage);

            AddMessage(botResponse, false);

            // The user said goodbye
            if (chatBot.ShouldExit)
            {
                txtInput.IsEnabled = false;
                sendMessage.IsEnabled = false;
                return;
            }

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

            string imagePath = isUser ? "user.png" : "chatbot.png";

            try
            {
                profileImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            catch (Exception)
            {
                // Show the message even if the picture is missing
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
    }
}
