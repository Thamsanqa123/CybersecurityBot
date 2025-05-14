using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Xml.Linq;

namespace CybersecurityBot { 
class Program
{
        // Stores user-specific data (last topic discussed). This allows the bot to recall past interactions
        static Dictionary<string, string> userMemory = new Dictionary<string, string>();

        // used dictionaries because its easy to add new topics (just update responseLibrary). 
        // Holds randomized responses for each cybersecurity topic. Ensures the bot does repeat the same answer
        static Dictionary<string, List<string>> responseLibrary = new Dictionary<string, List<string>>()
    {
        { "phishing", new List<string>()
            {
                "Be cautious of emails asking for personal info. Scammers mimic trusted organizations.",
                "Check sender addresses carefully—phishing emails often have slight misspellings.",
                "Never click links in unexpected emails. Visit the company’s website directly instead."
            }
        },
        { "password", new List<string>()
            {
                "Use a mix of uppercase, numbers, and symbols (e.g., 'S@f3P@ss!').",
                "Avoid common words like 'password123'. Try a passphrase: 'BlueCoffeeMug@2024!'",
                " Enable two-factor authentication (2FA) for extra security."
            }
        },
         { "privacy", new List<string>()
         {
               "always review app permissions and use VPNs on public Wi-Fi!"
            }
        },
            { "malware", new List<string>()
            {
                "Although backups don’t provide that much protection, getting into the habit of regularly backing up your critical files locally and in the cloud will help you restore your systems after an attack and minimize downtime costs.",
                "Regularly update your software",
                "Implement multifactor authentication (MFA"
            }
        }
    };



        // Maps keywords to empathetic responses.
        static Dictionary<string, string> sentimentResponses = new Dictionary<string, string>()
    {
        { "worried", "It’s normal to feel overwhelmed. Let’s tackle this step by step." },
        { "frustrated", "Cybersecurity can be tricky, but you’re doing great by learning!" },
        { "curious", "That’s a great question! Here’s what you need to know..." },
             { "thanks", "That's great to hear, /nHow else can I help?" } 

    };
        static void Main(string[] args)
    {
 //The following methods set up a welcoming environment before interaction begins
        
            PlayWelcomeSound();// Plays audio greeting


            ShowAsciiArt();// Displays cybersecurity logo


            string name = GetUserName();// Asks for the user's name
            WelcomeUser(name);

            //Keeps the chat active until the user says "no" to continue.
            while (true)
        {
            string query = GetUserQuery(name);//gets input
            HandleQuery(query, name);// process input

           
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\nDo you want to continue? (yes/no): ");
            Console.ResetColor();

            string continueChoice = Console.ReadLine().ToLower();
            if (continueChoice != "yes")
            {
                PrintWithColor($"\nStay safe, {name}! Bye!", ConsoleColor.Green);
                break;
            }
        }
    }

        // Keyword Recognition + Memory
        static void HandleQuery(string query, string name)
        {

            // Sentiment detection 
            foreach (var sentiment in sentimentResponses)
            {
                if (query.Contains(sentiment.Key))/* Checks if the input contains words like "worried" or "frustrated".
                                                   * Example: "I’m worried about hackers."
                                                   */
                {
                    PrintWithColor($"{sentiment.Value}", ConsoleColor.Magenta);
                    break;
                }
            }
            // Keyword Recognition, where you can search for topics
            bool topicFound = false;
            foreach (var topic in responseLibrary)
            {
                if (query.Contains(topic.Key))
                {
                    string response = topic.Value[new Random().Next(topic.Value.Count)];
                    PrintWithColor($"{name}, {response}", topic.Key == "phishing" ? ConsoleColor.Red : ConsoleColor.Yellow);
                    userMemory["LastTopic"] = topic.Key; // Remember topic for follow-ups
                    topicFound = true;
                    break;
                }
            }

            // If the user asks "remember", the bot recalls their last topic.
            if (query.Contains("remember") && userMemory.ContainsKey("LastTopic"))
        {
            PrintWithColor($" You were learning about {userMemory["LastTopic"]}. Want to dive deeper?", ConsoleColor.Blue);
    topicFound = true;
        }
            

          // handles unrecognized inputs. i still dont know why it keeps popping up, after the sentiment
            if (!topicFound)
            {
                PrintWithColor($"I’m not sure I understand. Try asking about 'passwords', 'phishing', 'malware', or 'privacy', {name}.", ConsoleColor.DarkGray);
            }
        }

        // Voice greeting implemented
        public static void PlayWelcomeSound()
    {
        try
        {
            SoundPlayer player = new SoundPlayer("welcome.wav");
           
            player.PlaySync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Audio error: " + ex.Message);
        }
    }

   //The ASCII art display
    static void ShowAsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(@"
       ___     _  _   _                                                                 _      _       _  _             ___             _     
  / __|   | || | | |__     ___      _ _    ___     ___     __     _  _      _ _    (_)    | |_    | || |    o O O  | _ )    ___    | |_   
 | (__     \_, | | '_ \   / -_)    | '_|  (_-<    / -_)   / _|   | +| |    | '_|   | |    |  _|    \_, |   o       | _ \   / _ \   |  _|  
  \___|   _|__/  |_.__/   \___|   _|_|_   /__/_   \___|   \__|_   \_,_|   _|_|_   _|_|_   _\__|   _|__/   TS__[O]  |___/   \___/   _\__|  
_|""""""""""|_| """"""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_|""""""""""|_| """"""""| {======|_|""""""""""|_|""""""""""|_|""""""""""| 
""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'""`-0-0-'./o--000'""`-0-0-'""`-0-0-'""`-0-0-' 
        ");
        Console.ResetColor();
        Thread.Sleep(1000); // Pause for effect
    }

  
    static string GetUserName()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\nEnter your name: ");
        Console.ResetColor();

        string name;
        while (true)
        {
            name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                break;
            PrintWithColor("Please enter a valid name!", ConsoleColor.Red);
        }
        return name;
    }

    // 4. Personalized welcome
    static void WelcomeUser(string name)
    {
        PrintWithColor($"\nWelcome, {name}!", ConsoleColor.Green);
        PrintWithColor("I'm here to help you stay safe online.", ConsoleColor.Cyan);
        PrintWithColor("You can ask me about:", ConsoleColor.Yellow);

        Console.WriteLine("----------------------------------------");
        Console.WriteLine("- 'Phishing' (How to spot fake emails)");
        Console.WriteLine("- 'Passwords' (Creating strong passwords)");
        Console.WriteLine("- 'Privacy' (Safe internet practices)");
        Console.WriteLine("- 'Malware' (Avoiding viruses)");
        Console.WriteLine("----------------------------------------");
    }

    // 5. Handle user queries
 

    // Gets and validates user query
    static string GetUserQuery(string name)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"\n{name}, Message CybersecurityBot ");
        Console.ResetColor();

        string input;
        while (true)
        {
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input;

            PrintWithColor("Please enter a valid question!", ConsoleColor.Red);
        }
    }

    // Helper method for colored text
    static void PrintWithColor(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
}