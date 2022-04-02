namespace AnswerMematron;

using AnswerMap = Dictionary<string, Dictionary<string, string>>;

public class QuestionPair
{
    public QuestionPair(string category, string question)
    {
        Category = category;
        Question = question;
    }

    public string Category { get; }
    
    public string Question { get; }
}


public class LearningHandler
{
    public class SkipException : Exception { }
    
    public class ExitException : Exception { }

    private class AnswerMissingException : Exception { }
    
    private class ReloadException : Exception { }

    
    private readonly ILoader _loader;

    private AnswerMap _answerMap;

    public LearningHandler(ILoader loader)
    {
        _loader = loader;
        _answerMap = loader.LoadAnswers();
    }

    private string Get(QuestionPair pair)
    {
        try
        {
            return _answerMap[pair.Category][pair.Question];
        }
        catch (KeyNotFoundException)
        {
            throw new AnswerMissingException();
        }
    }

    private string[] GetSentences(QuestionPair pair)
    {
        return Get(pair).Split('.')
            .Select(s => s.Trim())
            .Where(s => s != "")
            .Select(s => s.EndsWith('.') ? s : s + ".")
            .ToArray();
    }

    public void Learn(QuestionPair pair)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(pair.Question);
        Console.ResetColor();

        if (PromptYesNo("Would you like to see the full answer first"))
        {
            Console.WriteLine(Get(pair));
            PromptAnyKey();
        }
        
        Console.Clear();

        while (true)
        {
            string[] sentences = GetSentences(pair);
            try
            {
                int incorrect = 0;
                foreach (string sentence in sentences)
                {
                    int firstSpace = sentence.IndexOf(' ');
                    string firstWord = sentence[..firstSpace];
                    string expected = sentence[firstSpace..].Trim();

                    string given = Prompt(firstWord + "...?: ").Trim();
                    if (given.Last() != '.')
                    {
                        given += ".";
                    }

                    if (!expected.EqualsVague(given))
                    {
                        Console.WriteLine($"Incorrect, the right answer was: {sentence}\n"); 
                        PromptAnyKey();
                        incorrect++;
                    }

                    Console.Clear();
                }

                if (incorrect == 0)
                {
                    if (!PromptYesNo("Would you like to learn this answer again"))
                    {
                        break;
                    }
                }
            }
            catch (ReloadException)
            {
                Console.WriteLine("Answers reloaded");
            }
        }
    }

    public void Test(QuestionPair pair)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(pair.Question);
        Console.ResetColor();

        while (true)
        {
            try
            {
                foreach (var sentence in GetSentences(pair))
                {

                    string given = Prompt("");
                    if (!given.EndsWith('.'))
                    {
                        given += '.';
                    }

                    if (!sentence.EqualsVague(given))
                    {
                        Console.WriteLine("Incorrect: for next time");
                        Console.WriteLine(sentence + "\n");
                    }
                }

                if (!PromptYesNo("Test again"))
                {
                    break;
                }
            }
            catch (ReloadException)
            {
                Console.WriteLine("Reload detected, restarting test");
                throw;
            }
        }

        Console.Clear();
    }


    private string Prompt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string result = Console.ReadLine() ?? throw new FormatException("Invalid console input");

            if (result.TryTrim(">", out var command))
            {
                command = command.ToLower();
                if (command == "reload")
                {
                    _answerMap = _loader.LoadAnswers();
                    throw new ReloadException();
                }   else if (command == "skip")
                {
                    throw new SkipException();
                }   else if (command == "exit")
                {
                    throw new ExitException();
                }
            }
            else
            {
                return result;
            }
        }
    }

    private bool PromptYesNo(string prompt)
    {
        while (true)
        {
            var result = Prompt(prompt + " (y/n)?: ");
            if (result == "y")
            {
                return true;
            }

            if (result == "n")
            {
                return false;
            }
        }
    }

    private void PromptAnyKey()
    {
        Console.Write("Press ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("any");
        Console.ResetColor();
        Console.Write(" key");
        Console.ReadKey();
    }
}