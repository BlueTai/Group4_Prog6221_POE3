using System.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SecBot
{
    
    public partial class MainWindow
    {
        // Path to the greeting audio file. Ensure this is correct for your system.
        private const string GreetingSoundPath = @"C:\Users\bluet\source\repos\Group4_Prog6221_POE3\Secbot\greeting.wav";

        // ASCII Art for the chatbot's visual identity.
        internal static string _asciiArt = """

                                                 .--"--.
                                                /________\
                                               | __________ |
                                               | |        | |
                                               |_|________|_|
                                                 \________/
                                                   '------'

                                                   S E C B O T
                                                   
                                           """;

        // Sound player for audio feedback.
        private readonly SoundPlayer _soundPlayer = new SoundPlayer();

        // Dictionary for Keyword Recognition and Random Responses.
        private static readonly Dictionary<string, List<string>> KeywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {"password", [
                    "A strong password typically includes a mix of uppercase and lowercase letters, numbers, and special characters. Aim for a length of at least 12 characters. Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                    "To create a robust password, combine letters (both cases), numbers, and symbols. The longer and more complex, the better. Never reuse passwords across different services.",
                    "Think of a passphrase rather than a password: a series of unrelated words that are easy for you to remember but hard for others to guess. For example, 'correct horse battery staple'. Consider using a password manager!"
                ]
            },
            {"security", [
                    "Cybersecurity involves protecting systems, networks, and programs from digital attacks. This often includes protecting sensitive data and ensuring system integrity.",
                    "Good security practices include using strong passwords, enabling multi-factor authentication, being wary of phishing attempts, and regularly updating your software. Always be vigilant!",
                    "Staying secure online means being vigilant about what you click, where you share information, and always considering the source of digital communications."
                ]
            },
            {"privacy", [
                    "Online privacy refers to the level of anonymity and protection of your personal information while using the internet. Be mindful of what data you share online.",
                    "To enhance your privacy, review privacy settings on social media and other accounts, use privacy-focused browsers, and be cautious about granting app permissions.",
                    "Remember that data you share online can often be collected and used. Consider what information is truly necessary to share before you post or input it. Think before you share!"
                ]
            },
            {"phishing", [
                    "Phishing is a type of cybercrime where attackers impersonate legitimate institutions to trick individuals into revealing sensitive information. Be cautious of unsolicited emails or messages asking for personal details.",
                    "Always verify the sender of an email or message before clicking on links or downloading attachments, especially if it asks for personal information. Phishing attempts often contain grammar errors or unusual sender addresses.",
                    "If you suspect a phishing attempt, do not reply, click any links, or download attachments. Instead, report it to the appropriate authorities or your IT department. Stay alert for red flags!"
                ]
            },
            {"safe Browse", [
                    "Practice safe Browse by ensuring websites have 'HTTPS' in the address bar, being cautious of suspicious links, and keeping your browser updated.",
                    "Look for the padlock icon in the address bar to confirm a website uses HTTPS, indicating a secure connection. This helps protect your data during transmission.",
                    "Avoid clicking on pop-up ads or unfamiliar links, as they can lead to malicious websites. Always type website addresses directly if you're unsure. Trust your instincts!"
                ]
            },
            // Default responses for unknown inputs.
            {"general_inquiry", [
                    "I'm not sure I understand. Can you try rephrasing?",
                    "That's an interesting question! I'm constantly learning, but I don't have enough information on that topic yet. Could you ask me about something else?",
                    "My apologies, I'm currently focused on cybersecurity topics. Can I help you with something related to online safety?",
                    "I'm here to assist with security queries. You can ask me about strong passwords, phishing, or safe Browse habits.",
                    "Please tell me more about what you'd like to know regarding cybersecurity."
                ]
            }
        };

        // Dictionary to store user-specific information for Memory and Recall.
        private static readonly Dictionary<string, string> UserData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Random number generator for responses.
        private static readonly Random Random = new Random();

        // --- Task Assistant & Activity Log Features ---

        // Class to represent a cybersecurity task.
        public class CybersecurityTask
        {
            public string? Title { get; set; }
            public required string Description { get; set; }
            public DateTime? ReminderDate { get; set; }
            public bool IsCompleted { get; set; }
        }

        // List to store all user tasks.
        private readonly List<CybersecurityTask> _tasks = [];

        // Timer to check for reminders every minute.
        private readonly DispatcherTimer _reminderTimer;

        // Class to represent a log entry.
        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string? Description { get; set; }
        }

        // List to store the activity log.
        private List<LogEntry> _activityLog = new List<LogEntry>();
        private const int MaxLogEntries = 10; // Limits the log to the last 10 entries.

        // --- Quiz Game Feature ---

        // Enum to manage chatbot state.
        public enum ChatState
        {
            Normal,
            Quiz
        }

        private ChatState _currentState = ChatState.Normal;
        private int _currentQuestionIndex = 0;
        private readonly List<(string question, string answer, string hint)> _quizQuestions =
        [
            ("What is the main goal of phishing?", "to trick users into revealing sensitive information",
                "It starts with 't' and ends with 'n'."),
            ("What is the recommended minimum length for a strong password?", "12 characters",
                "A number followed by the word 'characters'."),
            ("What does 'MFA' stand for in cybersecurity?", "Multi-Factor Authentication",
                "It's a way to add an extra layer of security."),
            ("What is a common type of malware that encrypts files and demands payment?", "Ransomware",
                "It holds your data for a 'ransom'."),
            ("Which protocol ensures a secure connection for Browse the web?", "HTTPS",
                "Look for the 'S' in the address bar."),
            ("What is the best way to protect against data loss from hardware failure or malware?",
                "Regularly backing up your data", "Think of it as making a copy of your files.")
        ];

        // Constructor for the MainWindow class.
        public MainWindow()
        {
            InitializeComponent();

            // Display initial greeting message and play sound.
            DisplaySecBotMessage("Hello! What's your name?");

            // Log the startup event.
            AddLogEntry("SecBot started.");

            // Initialize and start the reminder timer.
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60) // Check every minute.
            };
            _reminderTimer.Tick += ReminderTimer_Tick;
            _reminderTimer.Start();

            // Attempt to play the greeting sound.
            try
            {
                _soundPlayer.SoundLocation = GreetingSoundPath;
                _soundPlayer.Load();
                _soundPlayer.Play();
            }
            catch (FileNotFoundException)
            {
                // Display error message in the chat history.
                DisplaySecBotMessage($"Error: Greeting audio file not found at '{GreetingSoundPath}'. Please ensure the file exists and the path is correct.");
            }
            catch (Exception ex)
            {
                DisplaySecBotMessage($"Error playing greeting sound: {ex.Message}");
            }
        }

        public static object? AsciiArt { get; } = new();

        // Event handler for the Send Button Click.
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        // Event handler for the Enter key press in the TextBox.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        // Main logic to process user input and manage chatbot state.
        private void ProcessUserInput()
        {
            string userInput = UserInputTextBox.Text;
            if (string.IsNullOrWhiteSpace(userInput))
            {
                DisplaySecBotMessage("Please enter some text.");
                return;
            }

            DisplayUserMessage(userInput);
            UserInputTextBox.Clear();

            string lowerInput = userInput.ToLower();
            string response;

            // State machine for the quiz.
            if (_currentState == ChatState.Quiz)
            {
                // Handle quiz answer and determine the next action.
                response = HandleQuizAnswer(lowerInput);
            }
            else // Normal conversation mode.
            {
                // Use the enhanced NLP-simulated response logic.
                response = GetSecBotResponse(lowerInput);

                // If the user initiates the quiz, change the state and prepare the first question.
                if (ContainsAny(lowerInput, "start quiz", "take a quiz", "test my knowledge", "quiz me"))
                {
                    _currentState = ChatState.Quiz;
                    _currentQuestionIndex = 0;
                    response = $"Ready to test your knowledge! Let's begin the quiz. Type 'exit quiz' to leave at any time.\n\nHere is the first question:\n{_quizQuestions[_currentQuestionIndex].question}";
                    AddLogEntry("Quiz started.");
                }
            }

            DisplaySecBotMessage(response);
        }

        // Handles the user's answer during the quiz state.
        private string HandleQuizAnswer(string lowerInput)
        {
            // Allow user to exit the quiz.
            if (lowerInput == "exit quiz" || lowerInput == "quit quiz" || lowerInput == "bye")
            {
                _currentState = ChatState.Normal;
                _currentQuestionIndex = 0;
                AddLogEntry("Quiz exited by user.");
                return "You have exited the quiz. Feel free to ask me about other cybersecurity topics.";
            }

            // Provide a hint if requested.
            if (ContainsAny(lowerInput, "hint", "help", "clue"))
            {
                return $"Here's a hint: {_quizQuestions[_currentQuestionIndex].hint}";
            }

            // Check if the user's answer is correct.
            string correctAnswer = _quizQuestions[_currentQuestionIndex].answer.ToLower();
            if (lowerInput.Contains(correctAnswer))
            {
                // Correct answer.
                var response = "Correct! That's a great answer.";
                _currentQuestionIndex++;

                // Check for the next question.
                if (_currentQuestionIndex < _quizQuestions.Count)
                {
                    response += $"\n\nNext question:\n{_quizQuestions[_currentQuestionIndex].question}";
                }
                else
                {
                    // Quiz complete.
                    response += "\n\nQuiz complete! You did great. We can return to our normal conversation now.";
                    _currentState = ChatState.Normal;
                    _currentQuestionIndex = 0;
                    AddLogEntry("Quiz completed successfully.");
                }
                return response;
            }
            else
            {
                // Incorrect answer.
                return $"That's not quite right. Try again, or type 'hint' for a clue.";
            }
        }

        // Simulates NLP by checking if the input contains any of a set of keywords.
        private static bool ContainsAny(string input, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        // Determines SecBot's response based on user input, implementing flexible recognition and memory.
        private string GetSecBotResponse(string lowerInput)
        {
            // Flexible recognition for adding tasks and reminders.
            if (ContainsAny(lowerInput, "add task", "create task", "set a reminder", "remind me to", "i need to", "add 2fa", "create a 2fa task"))
            {
                string taskTitle = "a new cybersecurity task";
                if (lowerInput.Contains("to "))
                {
                    taskTitle = lowerInput.Substring(lowerInput.IndexOf("to ", StringComparison.Ordinal) + 3).Trim();
                }
                else if (lowerInput.Contains("about "))
                {
                    taskTitle = lowerInput.Substring(lowerInput.IndexOf("about ", StringComparison.Ordinal) + 6).Trim();
                }

                // Guide the user to the GUI.
                AddLogEntry($"Recognized intent to add task: '{taskTitle}'.");
                return $"It sounds like you want to add a task about '{taskTitle}'. Please use the 'Task Assistant' tab on the right to add a new task with all the details and an optional reminder.";
            }
            // Flexible recognition for viewing tasks.
            else if (ContainsAny(lowerInput, "show tasks", "list my tasks", "what are my tasks", "view tasks"))
            {
                // Update the GUI display and log the action.
                UpdateTaskListDisplay();
                AddLogEntry("User viewed task list.");
                return _tasks.Count == 0
                    ? "You don't have any tasks on your list yet. You can add one using the Task Assistant on the right."
                    : $"I've updated the task list on the right. You have {_tasks.Count} tasks in total.";
            }
            // Flexible recognition for managing tasks.
            else if (ContainsAny(lowerInput, "complete", "mark as done", "finish", "done with", "delete", "remove", "get rid of"))
            {
                // Guide the user to the GUI.
                AddLogEntry($"Recognized intent to manage tasks.");
                return "You can manage your tasks, including marking them as complete or deleting them, using the controls in the 'Task Assistant' tab on the right.";
            }
            // Flexible recognition for viewing the activity log.
            else if (ContainsAny(lowerInput, "show activity log", "what have you done", "show me the log", "show log", "recent actions"))
            {
                // Update the GUI display and log the action.
                UpdateActivityLogDisplay();
                AddLogEntry("User viewed activity log.");
                return "I've updated the 'Activity Log' tab with a record of my recent actions.";
            }
            // Flexible recognition for general conversational phrases.
            else if (ContainsAny(lowerInput, "how are you", "how are you doing"))
            {
                return "As a security-focused program, I am functioning optimally. Thank you for asking.";
            }
            else if (ContainsAny(lowerInput, "your name", "who are you"))
            {
                var namePart = UserData.TryGetValue("name", out var value) ? $"I'm SecBot, and your name is {value}." : "My name is SecBot, your helpful security assistant.";
                return namePart;
            }
            else if (ContainsAny(lowerInput, "what can i ask", "what can you do", "help"))
            {
                return "I can help with topics like strong passwords, phishing, safe Browse habits, and online privacy. I can also help you manage your cybersecurity tasks with reminders or test your knowledge with a quiz!";
            }
            else if (ContainsAny(lowerInput, "hello", "hi", "hey"))
            {
                var namePart = UserData.TryGetValue("name", out var value) ? value + " " : "";
                return $"Greetings {namePart}! How can I assist you with security today?";
            }
            // Memory and Recall: Name capture.
            else if (!UserData.ContainsKey("name") && (ContainsAny(lowerInput, "my name is", "i'm ", "i am ")))
            {
                var nameStartIndex = lowerInput.Contains("my name is") ? lowerInput.IndexOf("my name is", StringComparison.Ordinal) + "my name is".Length : lowerInput.Contains("i'm ") ? lowerInput.IndexOf("i'm ", StringComparison.Ordinal) + "i'm ".Length : lowerInput.IndexOf("i am ") + "i am ".Length;

                var potentialName = lowerInput[nameStartIndex..].Trim();
                if (string.IsNullOrWhiteSpace(potentialName)) return "I apologize, an unexpected error occurred.";
                potentialName = char.ToUpper(potentialName[0]) + potentialName.Substring(1);
                UserData["name"] = potentialName.Split(' ')[0];
                AddLogEntry($"User's name identified as {UserData["name"]}.");
                return $"Nice to meet you, {UserData["name"]}! How can I help you with cybersecurity today?";
            }
            // Keyword Recognition with Random Responses.
            else if (ContainsAny(lowerInput, "password", "passphrase", "login credentials"))
            {
                return GetRandomResponse("password");
            }
            else if (ContainsAny(lowerInput, "security", "cybersecurity", "online safety", "stay safe"))
            {
                return GetRandomResponse("security");
            }
            else if (ContainsAny(lowerInput, "privacy", "data privacy", "private information"))
            {
                return GetRandomResponse("privacy");
            }
            else if (ContainsAny(lowerInput, "phishing", "scam email", "suspicious link", "fake message"))
            {
                return GetRandomResponse("phishing");
            }
            else if (ContainsAny(lowerInput, "safe browse", "browse safely", "secure website", "https"))
            {
                return GetRandomResponse("safe Browse");
            }
            // Fallback for unrecognised input.
            else
            {
                var namePart = UserData.TryGetValue("name", out var value) ? value + ", " : "";
                return $"{namePart}{GetRandomResponse("general_inquiry")}";
            }
        }

        // Helper method to get a random response from a list for a given keyword.
        private static string GetRandomResponse(string keyword)
        {
            if (!KeywordResponses.TryGetValue(keyword, out var responses))
                return "I apologize, but an internal error occurred while trying to generate a response.";
            var index = Random.Next(responses.Count);
            return responses[index];
        }

        // Displays a message from the user in the chat history.
        private void DisplayUserMessage(string message)
        {
            var userMessage = new TextBlock
            {
                Text = $"You: {message}",
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = Brushes.DarkBlue,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            ChatHistoryStackPanel.Children.Add(userMessage);
        }

        // Displays a message from the chatbot in the chat history.
        private void DisplaySecBotMessage(string message)
        {
            var secBotMessage = new TextBlock
            {
                Text = $"SecBot: {message}",
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = Brushes.ForestGreen,
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            ChatHistoryStackPanel.Children.Add(secBotMessage);
            // Auto-scroll to the bottom of the chat history.
            if (VisualTreeHelper.GetParent(ChatHistoryStackPanel) is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        // Event handler for the "Add Task" button click.
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var title = TaskTitleTextBox.Text.Trim();
            var description = TaskDescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime? reminderDate = null;
            if (int.TryParse(ReminderDaysTextBox.Text, out int reminderDays) && reminderDays > 0)
            {
                reminderDate = DateTime.Now.AddDays(reminderDays);
            }

            var newTask = new CybersecurityTask
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false
            };

            _tasks.Add(newTask);

            // Log the action for the activity log.
            AddLogEntry($"Task added: '{newTask.Title}' with a reminder for {newTask.ReminderDate?.ToShortDateString() ?? "no specific date"}.");

            // Clear input fields and update display.
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            ReminderDaysTextBox.Clear();
            UpdateTaskListDisplay();

            // Provide consistent chatbot feedback.
            DisplaySecBotMessage($"I've added the task '{newTask.Title}' to your list. You can view it in the 'Task Assistant' tab.");
        }

        // Updates the UI to display the list of tasks.
        private void UpdateTaskListDisplay()
        {
            TaskListStackPanel.Children.Clear(); // Clear the previous list

            if (_tasks.Count == 0)
            {
                TaskListStackPanel.Children.Add(new TextBlock { Text = "No tasks have been added yet.", FontStyle = FontStyles.Italic, Margin = new Thickness(5) });
            }

            foreach (var task in _tasks.ToList())
            {
                var taskPanel = new Border
                {
                    Background = task.IsCompleted ? Brushes.LightGreen : Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 5, 0, 5),
                    Child = new Grid()
                };

                var grid = taskPanel.Child as Grid;
                grid?.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) }); // Checkbox column
                if (grid != null)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(1, GridUnitType.Star) }); // Details column
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(60) }); // Delete button column

                    // Checkbox to mark as completed.
                    var checkBox = new CheckBox
                    {
                        IsChecked = task.IsCompleted,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(checkBox, 0);
                    checkBox.Click += (s, e) =>
                    {
                        task.IsCompleted = checkBox.IsChecked ?? false;
                        UpdateTaskListDisplay(); // Refresh the display.
                        if (task.IsCompleted)
                        {
                            DisplaySecBotMessage($"Great job! You've marked '{task.Title}' as completed.");
                            AddLogEntry($"Task marked as completed: '{task.Title}'.");
                        }
                    };

                    // Task details (title, description, reminder).
                    var detailsStackPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    var titleText = new TextBlock
                    {
                        Text = task.Title,
                        FontWeight = FontWeights.Bold,
                        TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null,
                        Foreground = task.IsCompleted ? Brushes.Gray : Brushes.Black,
                        TextWrapping = TextWrapping.Wrap
                    };

                    var descriptionText = new TextBlock
                    {
                        Text = task.Description,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        TextWrapping = TextWrapping.Wrap
                    };

                    var reminderText = new TextBlock
                    {
                        Text = task.ReminderDate.HasValue
                            ? $"Reminder: {task.ReminderDate.Value:yyyy-MM-dd}"
                            : "No reminder set",
                        Foreground = Brushes.DarkRed,
                        FontWeight = FontWeights.SemiBold
                    };

                    detailsStackPanel.Children.Add(titleText);
                    if (!string.IsNullOrWhiteSpace(task.Description))
                    {
                        detailsStackPanel.Children.Add(descriptionText);
                    }

                    detailsStackPanel.Children.Add(reminderText);
                    Grid.SetColumn(detailsStackPanel, 1);

                    // Delete button.
                    var deleteButton = new Button
                    {
                        Content = "Delete",
                        Width = 60,
                        Height = 25,
                        Margin = new Thickness(10, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = task, // Store the task object.
                        Style = (Style)FindResource("AddTaskButtonStyle") // Reuse style.
                    };
                    Grid.SetColumn(deleteButton, 2);
                    deleteButton.Click += (s, e) =>
                    {
                        var deletedTask = (CybersecurityTask)((Button)s).Tag;
                        _tasks.Remove(deletedTask);
                        UpdateTaskListDisplay();
                        DisplaySecBotMessage($"Okay, I've removed the task '{deletedTask.Title}' from your list.");
                        AddLogEntry($"Task deleted: '{deletedTask.Title}'.");
                    };

                    grid.Children.Add(checkBox);
                    grid.Children.Add(detailsStackPanel);
                    grid.Children.Add(deleteButton);
                }

                TaskListStackPanel.Children.Add(taskPanel);
            }

            // Update the status text.
            TaskStatusTextBlock.Text = $"Active Tasks: {_tasks.Count(t => !t.IsCompleted)} / Total: {_tasks.Count}";
        }

        // Timer tick event to check for reminders.
        private void ReminderTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var task in _tasks.ToList())
            {
                if (task.IsCompleted || !task.ReminderDate.HasValue || task.ReminderDate.Value > DateTime.Now) continue;
                // Display a pop-up reminder.
                MessageBox.Show($"REMINDER: It's time to complete your task:\n\nTitle: {task.Title}\nDescription: {task.Description}", "Task Reminder", MessageBoxButton.OK, MessageBoxImage.Information);

                // Log the reminder action.
                AddLogEntry($"Reminder triggered for task: '{task.Title}'.");

                // To prevent repeated pop-ups for the same task in the same session, you can nullify the reminder date.
                task.ReminderDate = null;
                UpdateTaskListDisplay();
            }
        }

        // Adds a new entry to the activity log and limits its size.
        private void AddLogEntry(string description)
        {
            _activityLog.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Description = description
            });

            // Limit the log to the last `MaxLogEntries` entries.
            if (_activityLog.Count > MaxLogEntries)
            {
                _activityLog = _activityLog.Skip(_activityLog.Count - MaxLogEntries).ToList();
            }
            // Update the log display whenever a new entry is added.
            UpdateActivityLogDisplay();
        }

        // Updates the UI to display the activity log.
        private void UpdateActivityLogDisplay()
        {
            ActivityLogStackPanel.Children.Clear();

            if (_activityLog.Count == 0)
            {
                ActivityLogStackPanel.Children.Add(new TextBlock { Text = "No recent activity.", FontStyle = FontStyles.Italic, Margin = new Thickness(5) });
            }
            else
            {
                // Display the log entries in reverse chronological order (most recent first).
                foreach (var entry in _activityLog.OrderByDescending(e => e.Timestamp))
                {
                    var logEntryText = new TextBlock
                    {
                        Text = $"[{entry.Timestamp:HH:mm:ss}] {entry.Description}",
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new FontFamily("Consolas")
                    };
                    ActivityLogStackPanel.Children.Add(logEntryText);
                }
            }
        }
    }
}