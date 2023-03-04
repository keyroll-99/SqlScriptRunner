namespace SqlRunner.valueObjects;

internal class FileName
{
    public string Value { get; }

    public FileName(string value)
    {
        Value = value;
    }
    
    public static implicit operator string(FileName fileName) => fileName.Value;
    public static implicit operator FileName(string value) => new FileName(value);
}