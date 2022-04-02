using System.Text.Json;

namespace AnswerMematron;

using AnswerMap = Dictionary<string, Dictionary<string, string>>;

public class JsonLoader : ILoader
{
    private readonly string _path;

    public JsonLoader(string path)
    {
        _path = path;
    }

    public AnswerMap LoadAnswers()
    {
        using var fileStream = File.OpenRead(_path);
        return JsonSerializer.Deserialize<AnswerMap>(fileStream) ?? throw new FormatException("Answers file contained a null object");
    }
}