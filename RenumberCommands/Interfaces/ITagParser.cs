namespace RenumberCommands.Interfaces
{
    public interface ITagParser
    {
        (string Prefix, int? Number) Parse(string tagValue);
    }
}
