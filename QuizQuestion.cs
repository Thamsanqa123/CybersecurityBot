using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybersecurity_chatbot
{
    public class QuizQuestion
    {
        public string Text { get; }
        public List<string> Options { get; }
        public int CorrectIndex { get; }

        public QuizQuestion(string text, List<string> options, int correctIndex)
        {
            Text = text;
            Options = options;
            CorrectIndex = correctIndex;
        }
    }
}
