namespace WpfChatbotPOE
{
    /// <summary>
    /// One quiz question with its answer options, correct letter and explanation.
    /// </summary>
    public class QuizQuestion
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
}
