namespace VocabularyLib;

public class ExportWord
{
    public string Type { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public List<string> Translations { get; set; } = new List<string>();
}
