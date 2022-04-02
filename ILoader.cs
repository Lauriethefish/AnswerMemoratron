namespace AnswerMematron;

using AnswerMap = Dictionary<string, Dictionary<string, string>>;

public interface ILoader
{
    AnswerMap LoadAnswers();
}