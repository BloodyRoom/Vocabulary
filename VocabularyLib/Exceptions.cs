namespace VocabularyLib;

public class VocabularyAlreadyExist : Exception
{
    public VocabularyAlreadyExist() : base() {}
    public VocabularyAlreadyExist(string? message) : base(message) {}
}

public class VocabularyNotExist : Exception
{
    public VocabularyNotExist() : base() { }
    public VocabularyNotExist(string? message) : base(message) { }
}

public class VocabularyNotCreatedOrLoaded : Exception
{
    public VocabularyNotCreatedOrLoaded() : base() { }
    public VocabularyNotCreatedOrLoaded(string? message) : base(message) { }
}


public class WordIsAlreadyExist : Exception
{
    public WordIsAlreadyExist() : base() { }
    public WordIsAlreadyExist(string? message) : base(message) { }
}

public class WordIsNotExist : Exception
{
    public WordIsNotExist() : base() { }
    public WordIsNotExist(string? message) : base(message) { }
}

public class TranslationIsNotExist : Exception
{
    public TranslationIsNotExist() : base() { }
    public TranslationIsNotExist(string? message) : base(message) { }
}

public class TranslationsCanBeNull : Exception
{
    public TranslationsCanBeNull() : base() { }
    public TranslationsCanBeNull(string? message) : base(message) { }
}

public class Nullable : Exception
{
    public Nullable() : base() { }
    public Nullable(string? message) : base(message) { }
}