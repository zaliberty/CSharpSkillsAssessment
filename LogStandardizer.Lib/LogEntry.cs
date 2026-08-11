namespace LogStandardizer.Lib;
public class LogEntry
{
    public DateTime Date { get; init; }

    public string Time { get; init; } = string.Empty;

    public string Level { get; init; } = string.Empty;

    public string Method { get; init; } = "DEFAULT";

    public string Message { get; init; } = string.Empty;
}