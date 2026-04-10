public enum NationCode
{
    ko,
    ja,
    zh,
    en
}

public static class LanguageManager
{
    private static NationCode _nationCode;

    static LanguageManager()
    {
        _nationCode = NationCode.ko;
    }

    public static string GetNationCode()
    {
        return _nationCode.ToString();
    }

    public static void SetNationCode(NationCode code)
    {
        _nationCode = code;
    }
}