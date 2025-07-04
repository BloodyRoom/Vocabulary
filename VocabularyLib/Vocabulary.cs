namespace VocabularyLib;

public class Vocabulary
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Translations = new Dictionary<string, List<string>>();
}
