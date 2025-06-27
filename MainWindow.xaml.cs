using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Cybersecurity_chatbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Task Management
        private ObservableCollection<CyberTask> _tasks = new ObservableCollection<CyberTask>();
        public ObservableCollection<CyberTask> tasks => _tasks;

        // Quiz System
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private readonly Dictionary<string, string> _userMemory = new();
        
        // Activity Log
        private List<string> activityLog = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            PlayWelcomeSound();
                InitializeQuiz();
            TaskGrid.ItemsSource = tasks;
            AddToChat("Bot", "Welcome to your Cybersecurity Assistant! Try:\n- 'Add task: Update passwords'\n- 'Start quiz'\n- 'Show activity log'");
        }

        static void PlayWelcomeSound()
        {
            
                SoundPlayer player = new SoundPlayer("welcome.wav"); // Corrected filename
                player.PlaySync();
           
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AddToChat("You", input);
            ProcessCommand(input);
            UserInput.Clear();
        }

        private void OnEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage(sender, e);
        }

        private void AddToChat(string sender, string message)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run($"{sender}: ") { FontWeight = FontWeights.Bold });
            paragraph.Inlines.Add(new Run(message));
            ChatHistory.Document.Blocks.Add(paragraph);
            ChatHistory.ScrollToEnd();
        }

        // ======================
        // NLP-INSPIRED COMMAND PROCESSING
        // ======================
        private void ProcessCommand(string input)
        {
            string lowerInput = input.ToLower();
            HandleSecurityQuery(input);

            // Task Management
            if (lowerInput.Contains("add task") || lowerInput.Contains("create task"))
            {
                string taskTitle = ExtractPhraseAfter(input, new[] { "task", "remind" });
                if (!string.IsNullOrEmpty(taskTitle))
                {
                    tasks.Add(new CyberTask { Title = taskTitle });
                    TaskGrid.Items.Refresh();
                    AddToChat("Bot", $"Task added: '{taskTitle}'. Set a reminder with 'Remind me in 7 days'");
                    AddToLog($"Task added: {taskTitle}");
                }
            }
            else if (lowerInput.Contains("remind me") && tasks.Any())
            {
                ProcessReminderCommand(input, tasks.Last());
                if (tasks.Count == 0) return;

                var lastTask = tasks.Last();
                if (lowerInput.Contains("tomorrow"))
                {
                    lastTask.DueDate = DateTime.Now.AddDays(1);
                }
                else if (lowerInput.Contains("days"))
                {
                    int days = ExtractNumber(input);
                    if (days > 0) lastTask.DueDate = DateTime.Now.AddDays(days);
                }

                if (lastTask.DueDate.HasValue)
                {
                    AddToChat("Bot", $"⏰ Reminder set for {lastTask.DueDate.Value.ToShortDateString()}");
                    AddToLog($"Reminder set for '{lastTask.Title}'");
                }
                TaskGrid.Items.Refresh();
            }

            // Quiz System
            else if (lowerInput.Contains("start quiz") || lowerInput.Contains("begin quiz"))
            {
                StartQuiz();
            }

            // Activity Log
            else if (lowerInput.Contains("activity log") || lowerInput.Contains("what have you done"))
            {
                ShowActivityLog();
            }

            // Fallback
            else
            {
                AddToChat("Bot", "I didn't understand that. Try:\n- 'Add task: Update passwords'\n- 'Start quiz'\n- 'Show my tasks'");
            }
        }

        // Helper for NLP-like parsing
        private string ExtractPhraseAfter(string input, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                int index = input.ToLower().IndexOf(keyword);
                if (index >= 0)
                {
                    return input.Substring(index + keyword.Length).Trim(' ', ':', '.');
                }
            }
            return null;
        }

        private int ExtractNumber(string input)
        {
            var words = input.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int number)) return number;
            }
            return 0;
        }

        // ======================
        // TASK MANAGEMENT
        // ======================
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskGrid.SelectedItem is CyberTask task)
            {
                tasks.Remove(task);
                TaskGrid.Items.Refresh();
                AddToLog($"Task deleted: {task.Title}");
                AddToChat("Bot", $"Task '{task.Title}' removed.");
            }
        }
        // Add Task Command
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTaskInput.Text)) return;

            var newTask = new CyberTask
            {
                Title = NewTaskInput.Text,
                Description = "Cybersecurity maintenance task",
                DueDate = null
            };

            tasks.Add(newTask);
            AddToLog($"Task added: {newTask.Title}");
            NewTaskInput.Clear();

            // NLP-style confirmation
            AddToChat("Bot", $"Task '{newTask.Title}' created. Say 'remind me in X days' to set a deadline.");
        }

        // ======================
        // QUIZ SYSTEM
        // ======================
        private void InitializeQuiz()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion(
                    "What's the best practice for passwords?",
                    new List<string> { "Use the same password everywhere", "Use a mix of letters, numbers, and symbols", "Write them on sticky notes" },
                    1),
                new QuizQuestion(
                    "What should you do with suspicious emails?",
                    new List<string> { "Reply to verify", "Delete them", "Forward to IT" },
                    2),
               new QuizQuestion(
                    "What are the three core principles of the CIA triad in cybersecurity, and why are they important?",
                    new List<string> { "Confidentiality, Integrity, Availability", "Encryption, Authentication, Firewalls", "Prevention, Detection, Response" },
                    3),
               new QuizQuestion(
                    "How does multi-factor authentication (MFA) enhance security, and what are some common MFA methods?",
                    new List<string> { "Using only passwords", " Blocking all remote logins", "None of the above" },
                    4),
               new QuizQuestion(
                    "A phishing attack targeting a CEO is called",
                    new List<string> { "Spear phishing", "Whaling", "Spoofing" },
                    5),
               new QuizQuestion(
                    "How does a ransomware attack work, and what are some best practices to prevent it?",
                    new List<string> { "Regular backups and user training", "Disabling all firewalls", "Paying the hackers quickly" },
                    6),
               new QuizQuestion(
                    "Explain how a VPN (Virtual Private Network) enhances security for remote workers.",
                    new List<string> { " Slowing down internet speed", " Encrypting traffic and hiding IP addresses", "Sharing passwords securely" },
                    7),
               new QuizQuestion(
                    "Symmetric encryption uses?",
                    new List<string> { "A single shared key for encryption/decryption", "Only hashing algorithms", "None of the above" },
                    8),
               new QuizQuestion(
                    "How can zero-trust architecture improve an organization’s security posture?",
                    new List<string> { "Reply to verify", "Delete them", "Forward to IT" },
                    9),
               new QuizQuestion(
                    "IoT devices are often insecure due to?",
                    new List<string> { " Weak default passwords and unpatched firmware", "Overuse of encryption", "Lack of internet connectivity" },
                    10)
            };
        }

        private void StartQuiz()
        {
            currentQuestionIndex = 0;
            quizScore = 0;
            ShowQuestion(quizQuestions[currentQuestionIndex]);
            MainTabControl.SelectedIndex = 2; // Switch to quiz tab
        }

        private void ShowQuestion(QuizQuestion question)
        {
            QuizQuestion.Text = question.Text;
            QuizOptions.Children.Clear();

            for (int i = 0; i < question.Options.Count; i++)
            {
                var button = new Button
                {
                    Content = $"{i + 1}. {question.Options[i]}",
                    Tag = i,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                button.Click += (s, e) => CheckAnswer(question, (int)((Button)s).Tag);
                QuizOptions.Children.Add(button);
            }

            NextQuestionBtn.Visibility = Visibility.Collapsed;
        }

        private void CheckAnswer(QuizQuestion question, int selectedIndex)
        {
            bool isCorrect = (selectedIndex == question.CorrectIndex);
            if (isCorrect) quizScore++;

            foreach (Button btn in QuizOptions.Children)
            {
                btn.IsEnabled = false;
                if ((int)btn.Tag == question.CorrectIndex)
                    btn.Background = System.Windows.Media.Brushes.LightGreen;
                else if ((int)btn.Tag == selectedIndex && !isCorrect)
                    btn.Background = System.Windows.Media.Brushes.Salmon;
            }

            NextQuestionBtn.Visibility = Visibility.Visible;
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            if (currentQuestionIndex < quizQuestions.Count)
            {
                ShowQuestion(quizQuestions[currentQuestionIndex]);
            }
            else
            {
                string result = $"Quiz complete! Score: {quizScore}/{quizQuestions.Count}";
                AddToChat("Bot", result);
                AddToLog(result);
                MainTabControl.SelectedIndex = 0; // Return to chat
            }
        }

        // ACTIVITY LOG
        
        private void AddToLog(string action)
        {
            activityLog.Add($"{DateTime.Now:HH:mm}: {action}");
            if (activityLog.Count > 10) activityLog.RemoveAt(0);
            StatusText.Text = $"Last action: {action}";
        }

        private void ShowActivityLog()
        {
            string log = "Recent Actions:\n" + string.Join("\n",
                activityLog.Select((a, i) => $"{i + 1}. {a}").Reverse());
            AddToChat("Bot", log);
        }
        private string AnalyzeSentiment(string input)
        {
            var negativeWords = new[] { "worried", "scared", "frustrated" };
            var positiveWords = new[] { "thanks", "helpful", "good" };

            if (negativeWords.Any(w => input.Contains(w)))
                return "I understand this can be stressful. Let's break it down step by step.";
            if (positiveWords.Any(w => input.Contains(w)))
                return "Glad I could help! What else can I assist with?";
            return null;
        }
        private void ProcessReminderCommand(string input, CyberTask task)
        {
            if (input.Contains("remind me"))
            {
                if (input.Contains("tomorrow"))
                {
                    task.DueDate = DateTime.Now.AddDays(1);
                }
                else if (input.Contains("days"))
                {
                    int days = ExtractNumber(input);
                    if (days > 0) task.DueDate = DateTime.Now.AddDays(days);
                }

                if (task.DueDate.HasValue)
                {
                    AddToChat("Bot", $"⏰ Reminder set for {task.DueDate.Value.ToShortDateString()}");
                    AddToLog($"Reminder set for '{task.Title}'");
                }
            }
        }

        // 2. Keyword Responses (from Part 2)
        private readonly Dictionary<string, List<string>> _securityTips = new()
{
    { "phishing", new List<string>
        {
            "Never click links in unexpected emails - go to the website directly",
            "Check for spelling mistakes in sender addresses",
            "Legit companies will never ask for passwords via email"
        }
    },
    { "password", new List<string>
        {
            "Use a mix of uppercase, numbers, and symbols (e.g., S@f3P@ss!)",
            "Enable two-factor authentication where possible",
            "Consider using a password manager like Bitwarden"
        }
    }
};
        private void HandleSecurityQuery(string input)
        {
            // Sentiment detection first
            var sentimentResponse = AnalyzeSentiment(input);
            if (sentimentResponse != null)
            {
                AddToChat("Bot", sentimentResponse);
                return;
            }

            // Keyword recognition
            foreach (var topic in _securityTips)
            {
                if (input.Contains(topic.Key))
                {
                    var response = topic.Value[new Random().Next(topic.Value.Count)];

                    // Personalize if we know the user's name
                    string prefix = _userMemory.ContainsKey("UserName")
                        ? $"{_userMemory["UserName"]}, " : "";

                    AddToChat("Bot", $"{prefix}💡 {response}");

                    // Remember topic for follow-ups
                    _userMemory["LastTopic"] = topic.Key;
                    return;
                }
            }

            // Memory recall
            if (input.Contains("remember") && _userMemory.ContainsKey("LastTopic"))
            {
                AddToChat("Bot", $"You were learning about {_userMemory["LastTopic"]}. Want more tips?");
                return;
            }
        }
        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            string topic = ((Button)sender).Tag.ToString();
            HandleSecurityQuery(topic); // Forces a response on that topic
        }
    }
    
}