using System.Diagnostics;
using AnswerMematron;

void Open(string folder)
{
    if (!Directory.Exists(folder))
    {
        throw new DirectoryNotFoundException(folder);
    }
    
    Process.Start(new ProcessStartInfo
    {
        FileName = Path.GetFullPath(folder),
        UseShellExecute = true,
        Verb = "open"
    });
}

void Run(ILoader loader, LearningHandler handler, bool doLearn, bool doTest)
{
    var random = new Random();
    
    Console.WriteLine("Please type the categories you would like to learn from");
    var answers = loader.LoadAnswers();
    
    Console.Write("Categories available: ");
    bool first = true;
    foreach(var category in answers.Keys)
    {
        if (first)
        {
            first = false;
        }
        else
        {
            Console.Write(", ");
        }
        Console.Write(category);
    }
    Console.WriteLine();

    var chosenQuestions = new List<QuestionPair>();
    
    Console.WriteLine("Type \">done\" when you want to start learning");
    while (true)
    {
        string selected = Console.ReadLine() ?? throw new NullReferenceException("Null console input");

        if (selected == ">done")
        {
            break;
        }
        
        if (!answers.ContainsKey(selected))
        {
            Console.Error.WriteLine("That category does not exist");
            continue;
        }
        
        chosenQuestions.AddRange(answers[selected].Select(pair => new QuestionPair(selected, pair.Key)));
    }

    if (chosenQuestions.Count == 0)
    {
        Console.Error.WriteLine("No chosen questions!");
        return;
    }
    
    Console.Clear();

    while (true)
    {
        var randomisedQuestions = new List<QuestionPair>();
        var questionsClone = chosenQuestions.ToList();
        while (questionsClone.Count > 0)
        {
            var idx = random.Next(questionsClone.Count);
            
            randomisedQuestions.Add(questionsClone[idx]);
            questionsClone.RemoveAt(idx);
        }
        
        foreach (var question in randomisedQuestions)
        {
            try
            {
                if (doLearn)
                {
                    handler.Learn(question);
                }

                if (doTest)
                {
                    handler.Test(question);
                }
            }
            catch (LearningHandler.ExitException)
            {
                return;
            }
            catch (LearningHandler.SkipException)
            {
            }
            Console.Clear();
        }
        
        Console.WriteLine("Wraparound (all of the questions in the set have been shown to you, so we're going through them again)");
    }
}

string jsonAnswersPath = "answers.json";
string directoryAnswersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Answer Memoratron");

ILoader loader;
if (File.Exists(jsonAnswersPath))
{
    Console.WriteLine("Using JSON answers");
    loader = new JsonLoader(jsonAnswersPath);
}
else
{
    if (!Directory.Exists(directoryAnswersPath))
    {
        Directory.CreateDirectory(directoryAnswersPath);
        Console.WriteLine("No answers added yet! Opening answers folder . . .");
        Open(directoryAnswersPath);
        Console.WriteLine("Press any key once you've added your answers");
        Console.ReadKey();
    }
    
    loader = new DirectoryLoader(directoryAnswersPath);

}
var learningHandler = new LearningHandler(loader);

while (true)
{
    Console.WriteLine("Please choose an option:");
    Console.WriteLine("1) Learn answers");
    Console.WriteLine("2) Test answers");
    Console.WriteLine("3) Learn then test answers (most effective in Laurie's experience)");
    Console.WriteLine("4) Open answers folder");
    Console.WriteLine("5) Quit");
    if (!Int32.TryParse(Console.ReadLine(), out int choice))
    {
        Console.Error.WriteLine("Invalid choice!");
        continue;
    }
    switch(choice)
    {
        case 1:
            Run(loader, learningHandler, true, false);
            break;
        case 2:
            Run(loader, learningHandler, false, true);
            break;
        case 3:
            Run(loader, learningHandler, true, true);
            break;
        case 4:
            Open(directoryAnswersPath);
            break;
        case 5:
            return;
        default:
            Console.Error.WriteLine("Invalid choice!");
            break;
    }

}
