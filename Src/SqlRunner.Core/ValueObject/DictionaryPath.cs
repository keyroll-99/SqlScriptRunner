namespace SqlRunner.Core.ValueObject;

public class DictionaryPath
{
    public string Value { get; }

    public DictionaryPath(string value)
    {
        Value = value;
    }

    public static implicit operator string(DictionaryPath filePatch) => filePatch.Value;
    public static implicit operator DictionaryPath(string value) => new DictionaryPath(value);
}