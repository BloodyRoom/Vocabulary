using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using VocabularyLib;

namespace Vocabulary;

internal class Program
{
    private static VocabularyService vocabularyService = new VocabularyService();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string mainMenuAction = "0";
        List<string> errors = new List<string>();
        List<string> success = new List<string>();
        do
        {
            VisualHelper.PrintTitle("Головне меню");

            string[] actions = new string[0];
            string[] additionalInfo = new string[0];

            if (vocabularyService.GetLoadedVocabulary() == string.Empty)
            {
                actions = new string[] { "Створити словник", "Завантажити словник", "Вихід" };
            }
            else
            {
                actions = new string[] { "Створити словник", "Завантажити інший словник", "Пошук перекладу", "Редагування слів", "Експорт слова", "Вихід" };
                additionalInfo = new string[] { $"Словник: {vocabularyService.GetLoadedVocabulary()}" };
            }

            VisualHelper.MenuPrint(actions, additionalInfo, errors, success);
            errors.Clear();
            success.Clear();

            Console.Write("|   Вибір: ");
            mainMenuAction = Console.ReadLine() ?? "0";
            Console.Clear();

            int res = 0;
            string resString = string.Empty;
            switch (mainMenuAction)
            {
                case "1":
                    res = await CreateVocabularyMenu();

                    if (res == 1)
                    {
                        success.Add("Словник створенно успішно");
                    }

                    break;
                case "2":
                    res = await LoadVocabularyMenu();

                    if (res == 1)
                    {
                        success.Add("Словник завантажено успішно");
                    }

                    break;
                case "3":
                    if (vocabularyService.GetLoadedVocabulary() != string.Empty)
                    {
                        SearchTranslationMenu();
                    }
                    break;
                case "4":
                    if (vocabularyService.GetLoadedVocabulary() != string.Empty)
                    {
                        await EditWordsMenu();
                    }
                    break;
                case "5":
                    if (vocabularyService.GetLoadedVocabulary() != string.Empty)
                    {
                        resString = await ExportWordMenu();

                        success.Add($"Слово {resString} було екпортовано до /{vocabularyService.exportWordDirectory}/{vocabularyService.GetLoadedVocabulary()}-{resString}.json");
                    }
                    break;
                default:
                    errors.Add("Невірний пункт меню");
                    break;
            }

        } while (vocabularyService.GetLoadedVocabulary() == string.Empty ? mainMenuAction != "3" : mainMenuAction != "6");
    }


    static async Task<int> CreateVocabularyMenu()
    {
        List<string> errors = new List<string>();
        string vocabularyType = string.Empty;

        do
        {
            VisualHelper.PrintTitle("Створення словника");

            VisualHelper.MenuPrint(null, ["Введіть тип словника", "(fromLang_toLang) приклад (uk_en)"], errors, null);
            errors.Clear();

            Console.Write("|   Тип словника (0 щоб вийти): ");

            vocabularyType = Console.ReadLine() ?? string.Empty;
            if (vocabularyType == string.Empty)
            {
                errors.Add("Тип словника обов'язквий");
                Console.Clear();
                continue;
            }
            else if (vocabularyType == "0")
            {
                Console.Clear();
                return 0;
            }

            try
            {
                await vocabularyService.CreateVocabulary(vocabularyType);
                Console.Clear();
                return 1;
            }
            catch (VocabularyAlreadyExist)
            {
                errors.Add($"Такий словник вже існує");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }
        }
        while (true);
    }

    static async Task<int> LoadVocabularyMenu()
    {
        List<string> errors = new List<string>();
        string vocabularyType = string.Empty;

        do
        {
            VisualHelper.PrintTitle("Завантаження словника");

            VisualHelper.MenuPrint(null, ["Введіть тип словника", "(fromLang_toLang) приклад (uk_en)"], errors, null);
            errors.Clear();

            Console.Write("|   Тип словника (0 щоб вийти): ");

            vocabularyType = Console.ReadLine() ?? string.Empty;
            if (vocabularyType == string.Empty)
            {
                errors.Add("Тип словника обов'язквий");
                Console.Clear();
                continue;
            }
            else if (vocabularyType == "0")
            {
                Console.Clear();
                return 0;
            }

            try
            {
                await vocabularyService.LoadVocabulary(vocabularyType);
                Console.Clear();
                return 1;
            }
            catch (VocabularyNotExist)
            {
                errors.Add($"Словника {vocabularyType} не знайдено");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }
        }
        while (true);
    }

    static void SearchTranslationMenu()
    {
        List<string> errors = new List<string>();
        string word = string.Empty;

        do
        {
            VisualHelper.PrintTitle("Пошук перекладу");

            VisualHelper.MenuPrint(null, ["Введіть слово для", "пошуку перекладу"], errors, null);
            errors.Clear();

            Console.Write("|   Слово (0 щоб вийти): ");

            word = Console.ReadLine() ?? string.Empty;
            if (word == string.Empty)
            {
                errors.Add("Слово обо'язкове");
                Console.Clear();
                continue;
            }
            else if (word == "0")
            {
                Console.Clear();
                return;
            }

            try
            {
                var translations = vocabularyService.SearchTranslations(word);

                Console.Clear();
                VisualHelper.PrintTitle($"Переклади слова {word.ToLower()}");
                translations = translations.Select(x => $"-> {x}").ToList();
                translations.Insert(0, word.ToUpper());
                translations.AddRange(["", "Нажміть будь-яку клавішу щоб повернутися"]);
                VisualHelper.MenuPrint(null, translations.ToArray(), null, null);

                Console.ReadKey();
                Console.Clear();
            }
            catch (WordIsNotExist)
            {
                errors.Add($"Слово {word} не знайдено");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }
        }
        while (true);
    }

    static async Task EditWordsMenu()
    {
        List<string> errors = new List<string>();
        List<string> success = new List<string>();
        string action = string.Empty;

        do
        {
            VisualHelper.PrintTitle("Редагування слів");

            VisualHelper.MenuPrint([ "Додати слово", "Редагувати слово / переклад", "Видалити слово", "Назад" ], null, errors, success);

            errors.Clear();

            Console.Write("|   Вибір: ");
            action = Console.ReadLine() ?? string.Empty;

            string res = "0";
            switch (action)
            {
                case "1":
                    res = await AddWordMenu();

                    if (res != "0")
                    {
                        success.Add($"Слово {res} успішно додане");
                    }

                    break;

                case "2":
                    res = await EditWordOrTranslationMenu();

                    if (res != "0")
                    {
                        success.Add($"Слово {res} успішно відредаговане");
                    }

                    break;

                case "3":
                    res = await DeleteWordMenu();

                    if (res != "0")
                    {
                        success.Add($"Слово {res} успішно видалено");
                    }

                    break;
                default:
                    errors.Add("Невірний пункт меню");
                    Console.Clear();
                    break;
            }

        } while (action != "4");
        Console.Clear();
    }

    static async Task<string> AddWordMenu()
    {
        List<string> errors = new List<string>();

        while (true)
        {
            Console.Clear();

            VisualHelper.PrintTitle("Додавання слова");
            VisualHelper.MenuPrint(null, [ "Введіть нове слово", "Потім — переклади через кому (,)", "Приклад:  apple  ->  яблуко, епл" ], errors, null);
            errors.Clear();


            Console.Write("|   Слово (0 щоб повернутися): ");
            string word = Console.ReadLine()?.Trim() ?? string.Empty;

            if (word == "0") 
            { 
                Console.Clear(); 
                return "0"; 
            }
            if (word == string.Empty)
            {
                errors.Add("Слово обовʼязкове");
                Console.Clear(); 
                continue;
            }

            Console.Write("|   Переклади: ");
            var transInput = Console.ReadLine() ?? string.Empty;
            var translations = transInput.Split(',').Select(t => t.Trim()).Where(t => t != string.Empty).ToList();

            if (translations.Count == 0)
            {
                errors.Add("Потрібен хоча б один переклад");
                Console.Clear(); 
                continue;
            }

            try
            {
                await vocabularyService.AddWord(word, translations);
                Console.Clear();
                return word.ToLower();
            }
            catch (WordIsAlreadyExist)
            {
                errors.Add($"Слово {word.ToLower()} уже існує у словнику");
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
            }
            Console.Clear();
        }
    }

    static async Task<string> EditWordOrTranslationMenu()
    {
        List<string> errors = new List<string>();
        string word = string.Empty;

        do
        {
            Console.Clear();
            VisualHelper.PrintTitle("Редагування слова / перекладу");
            VisualHelper.MenuPrint(null, ["Введіть слово для", "редагування"], errors, null);
            errors.Clear();

            Console.Write("|   Слово (0 щоб повернутися): ");

            word = Console.ReadLine() ?? string.Empty;
            if (word == string.Empty)
            {
                errors.Add("Слово обо'язкове");
                Console.Clear();
                continue;
            }
            else if (word == "0")
            {
                Console.Clear();
                return "0";
            }

            try
            {
                var translations = vocabularyService.SearchTranslations(word);
                List<string> editErrors = new List<string>();
                List<string> editSuccess = new List<string>();

                string editAction = string.Empty;
                do
                {
                    Console.Clear();
                    VisualHelper.PrintTitle($"Редагування слова {word.ToLower()}");
                    var translationsFormated = translations.Select(x => $"-> {x}").ToList();
                    translationsFormated.Insert(0, word.ToUpper());
                    VisualHelper.MenuPrint(["Змінити слово", "Додати переклад", "Змінити переклади", "Видалити переклад", "Назад"], translationsFormated.ToArray(), editErrors, editSuccess);
                    editErrors.Clear();
                    editSuccess.Clear();

                    Console.Write("|   Вибір: ");
                    editAction = Console.ReadLine() ?? string.Empty;

                    switch (editAction)
                    {
                        case "1":
                            string newWord = string.Empty;
                            Console.Write("|\n|   Змінити на (0 щоб відмінити): ");
                            newWord = Console.ReadLine() ?? string.Empty;

                            if (newWord == string.Empty)
                            {
                                editErrors.Add("Нове слово обов'язкове");
                                Console.Clear();
                                continue;
                            }
                            else if (newWord == "0")
                            {
                                Console.Clear();
                                continue;
                            }

                            try
                            {
                                await vocabularyService.EditWord(word, newWord);
                                Console.Clear();
                                word = newWord;
                                editSuccess.Add("Слово зміннено успішно");
                                continue;
                            }
                            catch (WordIsAlreadyExist)
                            {
                                editErrors.Add($"Слово {newWord.ToLower()} вже існує");
                                Console.Clear();
                                continue;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Невідома помилка: {ex.Message}");
                                Console.Clear();
                            }

                            break;
                        case "2":
                            string translation = string.Empty;
                            Console.Write("|\n|   Переклад (0 щоб відмінити): ");
                            translation = Console.ReadLine() ?? string.Empty;

                            if (translation == string.Empty)
                            {
                                editErrors.Add("Новий переклад обов'язковий");
                                Console.Clear();
                                continue;
                            }
                            else if (translation == "0")
                            {
                                Console.Clear();
                                continue;
                            }
                            else if (translations.Contains(translation.ToLower()))
                            {
                                editErrors.Add($"Слово {word} вже має такий переклад");
                                Console.Clear();
                                continue;
                            }

                            try
                            {
                                await vocabularyService.AddTranslations(word, new List<string>() { translation });
                                Console.Clear();
                                editSuccess.Add("Переклад додано успішно");
                                continue;
                            }
                            catch (Exception ex)
                            {
                                editErrors.Add($"Невідома помилка: {ex.Message}");
                                Console.Clear();
                            }

                            break;
                        case "3":
                            string oldTranslation = string.Empty;
                            Console.Write("|\n|   Переклад який змінити (0 щоб відмінити): ");
                            oldTranslation = Console.ReadLine() ?? string.Empty;

                            if (oldTranslation == string.Empty)
                            {
                                editErrors.Add("Переклад який змінити обов'язковий");
                                Console.Clear();
                                continue;
                            }
                            else if (oldTranslation == "0")
                            {
                                Console.Clear();
                                continue;
                            }
                            else if (!translations.Contains(oldTranslation.ToLower()))
                            {
                                editErrors.Add($"Слово {word} не має такого перекладу");
                                Console.Clear();
                                continue;
                            }

                            string newTranslation = string.Empty;
                            Console.Write($"|   {oldTranslation} змінити на (0 щоб відмінити): ");
                            newTranslation = Console.ReadLine() ?? string.Empty;

                            if (newTranslation == string.Empty)
                            {
                                editErrors.Add("Переклад на який змінити обов'язковий");
                                Console.Clear();
                                continue;
                            }
                            else if (newTranslation == "0")
                            {
                                Console.Clear();
                                continue;
                            }

                            try
                            {
                                await vocabularyService.EditTranslate(word, oldTranslation, newTranslation);
                                Console.Clear();
                                translations.Remove(oldTranslation);
                                editSuccess.Add("Переклад зміннено успішно");
                                continue;
                            }
                            catch (Exception ex)
                            {
                                editErrors.Add($"Невідома помилка: {ex.Message}");
                                Console.Clear();
                            }

                            break;
                        case "4":
                            string delTranslation = string.Empty;
                            Console.Write("|\n|   Переклад (0 щоб відмінити): ");
                            delTranslation = Console.ReadLine() ?? string.Empty;

                            if (delTranslation == string.Empty)
                            {
                                editErrors.Add("Переклад для видалення обов'язковий");
                                Console.Clear();
                                continue;
                            }
                            else if (delTranslation == "0")
                            {
                                Console.Clear();
                                continue;
                            }
                            else if (!translations.Contains(delTranslation.ToLower()))
                            {
                                editErrors.Add($"Слово {word} не має такий переклад");
                                Console.Clear();
                                continue;
                            }

                            try
                            {
                                await vocabularyService.DeleteTranslation(word, delTranslation);
                                Console.Clear();
                                editSuccess.Add("Переклад видалено успішно");
                                continue;
                            }
                            catch (TranslationsCanBeNull)
                            {
                                editErrors.Add($"Мусить бути принаймні 1 переклад");
                                Console.Clear();
                            }
                            catch (Exception ex)
                            {
                                editErrors.Add($"Невідома помилка: {ex.Message}");
                                Console.Clear();
                            }

                            break;
                        default:
                            editErrors.Add("Невірний пункт меню");
                            break;
                    }
                }
                while (editAction != "5");
                
            }
            catch (WordIsNotExist)
            {
                errors.Add($"Слово {word} не знайдено");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }
        } while (true);
    }

    static async Task<string> DeleteWordMenu()
    {
        List<string> errors = new List<string>();
        string word = string.Empty;

        do
        {
            Console.Clear();
            VisualHelper.PrintTitle("Видалення слова");
            VisualHelper.MenuPrint(null, ["Введіть слово для", "видалення"], errors, null);
            errors.Clear();

            Console.Write("|   Слово (0 щоб повернутися): ");
            word = Console.ReadLine() ?? string.Empty;

            if (word == string.Empty)
            {
                errors.Add("Слово обо'язкове");
                Console.Clear();
                continue;
            }
            else if (word == "0")
            {
                Console.Clear();
                return "0";
            }

            try
            {
                await vocabularyService.DeleteWord(word);
                Console.Clear();
                return word;
            }
            catch (WordIsNotExist)
            {
                errors.Add($"Слово {word.ToLower()} не знайдено");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }

        } while (true);
    }

    static async Task<string> ExportWordMenu()
    {
        List<string> errors = new List<string>();
        string word = string.Empty;

        do
        {
            Console.Clear();
            VisualHelper.PrintTitle("Експортування слова");
            VisualHelper.MenuPrint(null, ["Введіть слово для", "експортування"], errors, null);
            errors.Clear();

            Console.Write("|   Слово (0 щоб повернутися): ");
            word = Console.ReadLine() ?? string.Empty;

            if (word == string.Empty)
            {
                errors.Add("Слово обо'язкове");
                Console.Clear();
                continue;
            }
            else if (word == "0")
            {
                Console.Clear();
                return "0";
            }

            try
            {
                await vocabularyService.ExportWord(word);
                Console.Clear();
                return word;
            }
            catch (WordIsNotExist)
            {
                errors.Add($"Слово {word.ToLower()} не знайдено");
                Console.Clear();
            }
            catch (Exception ex)
            {
                errors.Add($"Невідома помилка: {ex.Message}");
                Console.Clear();
            }

        } while (true);
    }
}
