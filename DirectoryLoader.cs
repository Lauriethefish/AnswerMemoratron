namespace AnswerMematron;

using AnswerMap = Dictionary<string, Dictionary<string, string>>;

public class DirectoryLoader : ILoader
{
    private readonly string _path;

    public DirectoryLoader(string path)
    {
        _path = path;
    }

    public AnswerMap LoadAnswers()
    {
        var result = new AnswerMap();
        
        foreach (var category in Directory.EnumerateDirectories(_path))
        {
            var categoryMap = new Dictionary<string, string>();
            
            foreach (var answerFileName in Directory.EnumerateFiles(category))
            {
                categoryMap[Path.GetFileNameWithoutExtension(answerFileName)] = File.ReadAllText(answerFileName);
            }

            result[Path.GetRelativePath(_path, category)] = categoryMap;
        }

        return result;
    }
}