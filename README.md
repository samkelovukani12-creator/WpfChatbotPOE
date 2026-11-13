YouTube Link: https://youtu.be/h1jw4RJ5ae8

Instructions on how the chatbot works.
A WPF (C#) desktop chatbot that teaches basic cybersecurity, manages tasks and reminders,
runs a quiz, and keeps an activity log.

## How to Use
1. Type your name when the bot asks.
2. Chat naturally, or click a quick-action button (Quiz, My tasks, Activity log, Safety tip, Help).
3. Press **Enter** or click **Send**.

### Example commands
| Say this | What happens |
|---|---|
| `What is phishing?` / `Tell me about 2FA` | Gets a cybersecurity explanation and tip |
| `Add a task to enable 2FA` | Adds a task and offers to set a reminder |
| `Remind me to update my password tomorrow` | Sets a reminder (also understands "in 3 days", "next Monday", "25 December") |
| `Show my tasks` | Lists all tasks |
| `Complete task 1` / `Delete task 1` | Updates your task list |
| `Quiz` | Starts the 12-question cybersecurity quiz (answer A, B, C or D; `quit quiz` to exit) |
| `Show activity log` / `What have you done for me?` | Shows your last 5 actions; type `show more` for the full history |
| `I'm worried about hackers` | Replies with empathy plus a safety tip |
| `Help` | Lists everything the bot can do |
| `Bye` | Ends the chat |

## Project Structure
| File | Purpose |
|---|---|
| `MainWindow.xaml` / `.xaml.cs` | User interface only |
| `ChatBotEngine.cs` | Coordinates all chatbot logic |
| `NlpProcessor.cs`, `DateParser.cs` | Simulated NLP: understands different wordings and dates |
| `CyberKnowledgeBase.cs`, `TipsDatabase.cs`, `SentimentDetector.cs` | Cybersecurity answers, tips and sentiment |
| `TaskManager.cs`, `TaskItem.cs` | Tasks and reminders |
| `QuizManager.cs`, `QuizQuestion.cs` | Quiz mini-game |
| `ActivityLog.cs` | Timestamped activity log |
| `TextHelper.cs` | Shared text helpers |
