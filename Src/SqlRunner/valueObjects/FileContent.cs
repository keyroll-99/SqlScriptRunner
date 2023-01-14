namespace SqlRunner.valueObjects;

public class FileContent
{
    public string Value { get; }

    public FileContent(string value)
    {
        Value = value;
    }

    public static implicit operator string(FileContent fileContent) => fileContent.Value;
    public static implicit operator FileContent(string value) => new FileContent(value);
}