using LogStandardizer.Lib;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogStandardizer;
public sealed class LogLineParser
{
    private const string TimePattern =
        @"(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d\.\d+";

    private static readonly Regex Format1Regex = new(
        $@"^(?<date>\d{{2}}\.\d{{2}}\.\d{{4}})\s+(?<time>{TimePattern})\s+(?<level>[A-Za-z]+)\s+(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex Format2Regex = new(
        $@"^(?<date>\d{{4}}-\d{{2}}-\d{{2}})\s+(?<time>{TimePattern})\s*\|\s*(?<level>[^|]*?)\s*\|\s*(?<id>[^|]*?)\s*\|\s*(?<method>[^|]*?)\s*\|\s*(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool TryParse(string line, [NotNullWhen(true)] out LogEntry? entry)
    {
        entry = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var normalizedLine = line.Trim();

        var match = Format1Regex.Match(normalizedLine);
        if (match.Success && TryParseFormat1(match, out entry))
        {
            return true;
        }

        match = Format2Regex.Match(normalizedLine);
        if (match.Success && TryParseFormat2(match, out entry))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseFormat1(Match match, [NotNullWhen(true)] out LogEntry? entry)
    {
        entry = null;

        var rawDate = match.Groups["date"].Value;
        var time = match.Groups["time"].Value;
        var rawLevel = match.Groups["level"].Value;
        var message = match.Groups["message"].Value.Trim();

        if (!DateTime.TryParseExact(
                rawDate,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return false;
        }

        if (!TryNormalizeLevel(rawLevel, out var level))
        {
            return false;
        }

        entry = new LogEntry
        {
            Date = date,
            Time = time,
            Level = level,
            Method = "DEFAULT",
            Message = message
        };

        return true;
    }

    private static bool TryParseFormat2(Match match, [NotNullWhen(true)] out LogEntry? entry)
    {
        entry = null;

        var rawDate = match.Groups["date"].Value;
        var time = match.Groups["time"].Value;
        var rawLevel = match.Groups["level"].Value;
        var method = match.Groups["method"].Value.Trim();
        var message = match.Groups["message"].Value.Trim();

        if (!DateTime.TryParseExact(
                rawDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return false;
        }

        if (!TryNormalizeLevel(rawLevel, out var level))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            method = "DEFAULT";
        }

        entry = new LogEntry
        {
            Date = date,
            Time = time,
            Level = level,
            Method = method,
            Message = message
        };

        return true;
    }

    private static bool TryNormalizeLevel(string rawLevel, out string level)
    {
        level = rawLevel.Trim().ToUpperInvariant();

        switch (level)
        {
            case "INFO":
            case "INFORMATION":
                level = "INFO";
                return true;

            case "WARN":
            case "WARNING":
                level = "WARN";
                return true;

            case "ERROR":
                level = "ERROR";
                return true;

            case "DEBUG":
                level = "DEBUG";
                return true;

            default:
                return false;
        }
    }
}

