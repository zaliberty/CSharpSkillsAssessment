using System.Globalization;

namespace LogStandardizer.Lib
{
    public sealed class LogStandardizerService
    {
        private readonly LogLineParser _parser = new();
        private readonly LogStandardizerOptions _options;

        public LogStandardizerService(LogStandardizerOptions? options = null)
        {
            _options = options ?? new LogStandardizerOptions();
        }

        public ProcessResult Process(TextReader input, TextWriter output, TextWriter problems)
        {
            long validCount = 0;
            long invalidCount = 0;

            string? line;

            while ((line = input.ReadLine()) != null)
            {
                if (_options.SkipEmptyLines && string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (_parser.TryParse(line, out var entry))
                {
                    output.WriteLine(Format(entry));
                    validCount++;
                }
                else
                {
                    problems.WriteLine(line);
                    invalidCount++;
                }
            }

            return new ProcessResult(validCount, invalidCount);
        }

        private string Format(LogEntry entry)
        {
            var date = entry.Date.ToString(_options.DateFormat, CultureInfo.InvariantCulture);

            return string.Join(
                "\t",
                date,
                entry.Time,
                entry.Level,
                entry.Method,
                entry.Message);
        }
    }
}
