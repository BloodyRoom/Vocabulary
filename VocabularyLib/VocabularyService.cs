using Newtonsoft.Json;
using System.ComponentModel;
using System.Transactions;

namespace VocabularyLib;

public class VocabularyService
{
    public readonly string directoryVocabularies = string.Empty;
    public readonly string exportWordDirectory = string.Empty;
    private Vocabulary Vocabulary = new Vocabulary();

    public VocabularyService()
    {
        directoryVocabularies = "Vocabularies";
        exportWordDirectory = "ExportedWords";
    }

    public async Task CreateVocabulary(string type)
    {
        Directory.CreateDirectory(directoryVocabularies);

        if (File.Exists($"{directoryVocabularies}/{type}.json"))
        {
            throw new VocabularyAlreadyExist();
        }
        
        Vocabulary.Type = type;
        await File.WriteAllTextAsync($"{directoryVocabularies}/{type}.json", JsonConvert.SerializeObject(Vocabulary));
    }

    public async Task LoadVocabulary(string type)
    {
        Directory.CreateDirectory(directoryVocabularies);

        if (!File.Exists($"{directoryVocabularies}/{type}.json"))
        {
            throw new VocabularyNotExist();
        }

        Vocabulary = JsonConvert.DeserializeObject<Vocabulary>(await File.ReadAllTextAsync($"{directoryVocabularies}/{type}.json")) ?? new Vocabulary();
    }

    public async Task SaveVocabulary()
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        Directory.CreateDirectory(directoryVocabularies);
        await File.WriteAllTextAsync($"{directoryVocabularies}/{Vocabulary.Type}.json", JsonConvert.SerializeObject(Vocabulary));
    }

    public string GetLoadedVocabulary()
    {
        return Vocabulary.Type;
    }

    public async Task AddWord(string word, List<string> translations)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null || translations == null || translations.Count == 0)
        {
            throw new Nullable();
        }

        if (Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsAlreadyExist();
        }

        translations = translations.Select(t => t.ToLower()).ToList();
        Vocabulary.Translations.Add(word.ToLower(), translations);
        await SaveVocabulary();
    }

    public async Task AddTranslations(string word, List<string> translations)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null || translations == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        }

        translations = translations.Select(t => t.ToLower()).ToList();
        Vocabulary.Translations[word.ToLower()].AddRange(translations);
        await SaveVocabulary();
    }

    public async Task EditWord(string oldWord, string newWord)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (oldWord == null || newWord == null)
        {
            throw new Nullable();
        }

        if (Vocabulary.Translations.ContainsKey(newWord.ToLower()))
        {
            throw new WordIsAlreadyExist();
        }

        var tmp = Vocabulary.Translations[(oldWord.ToLower())];
        Vocabulary.Translations.Remove(oldWord.ToLower());
        Vocabulary.Translations.Add(newWord.ToLower(), tmp);

        await SaveVocabulary();
    }

    public async Task EditTranslate(string word, string oldTranslation, string newTranslation)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null || oldTranslation == null || newTranslation == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        } 
        else if (!Vocabulary.Translations[word.ToLower()].Contains(oldTranslation.ToLower()))
        {
            throw new TranslationIsNotExist();
        }

        Vocabulary.Translations[word.ToLower()].Remove(oldTranslation.ToLower());
        Vocabulary.Translations[word.ToLower()].Add(newTranslation.ToLower());

        await SaveVocabulary();
    }

    public async Task DeleteWord(string word)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        }

        Vocabulary.Translations.Remove(word.ToLower());
        await SaveVocabulary();
    }

    public async Task DeleteTranslation(string word, string translation)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null || translation == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        }
        else if (!Vocabulary.Translations[word.ToLower()].Contains(translation.ToLower()))
        {
            throw new TranslationIsNotExist();
        }
        else if (Vocabulary.Translations[word.ToLower()].Count == 1)
        {
            throw new TranslationsCanBeNull();
        }

        Vocabulary.Translations[word.ToLower()].Remove(translation);
        await SaveVocabulary();
    }

    public List<string> SearchTranslations(string word)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        }

        return Vocabulary.Translations[word.ToLower()];
    }

    public async Task ExportWord(string word)
    {
        if (Vocabulary.Type == string.Empty) throw new VocabularyNotCreatedOrLoaded();

        if (word == null)
        {
            throw new Nullable();
        }

        if (!Vocabulary.Translations.ContainsKey(word.ToLower()))
        {
            throw new WordIsNotExist();
        }

        ExportWord eWord = new ExportWord() { 
            Type = Vocabulary.Type, 
            Word = word.ToLower(), 
            Translations = Vocabulary.Translations[word.ToLower()] 
        };

        Directory.CreateDirectory(exportWordDirectory);
        await File.WriteAllTextAsync($"{exportWordDirectory}/{Vocabulary.Type}-{word.ToLower()}.json", JsonConvert.SerializeObject(eWord));
    }
}
