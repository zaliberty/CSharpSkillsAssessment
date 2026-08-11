using LogStandardizer.Lib;
using System.Text;

namespace LogStandardizer;
public static class Program
{
    // RFC 5424 (Syslog) и большинство стандартов логирования не предусматривают BOM.
    // Логи должны быть "чистым" текстом.
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        if (args[0] is "-h" or "--help")
        {
            PrintUsage();
            return 0;
        }

        if (args.Length < 2)
        {
            PrintUsage();
            return 1;
        }

        var inputPath = args[0];
        var outputPath = args[1];
        var problemsPath = "problems.txt";
        var dateFormat = "yyyy-MM-dd";

        for (var i = 2; i < args.Length; i++)
        {
            if (args[i].StartsWith("--date-format=", StringComparison.OrdinalIgnoreCase))
            {
                dateFormat = args[i].Substring("--date-format=".Length);
            }
            else
            {
                problemsPath = args[i];
            }
        }

        try
        {
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Input file not found: {inputPath}");
                return 2;
            }

            if (AreSamePath(inputPath, outputPath) ||
                AreSamePath(inputPath, problemsPath) ||
                AreSamePath(outputPath, problemsPath))
            {
                Console.Error.WriteLine("Input file, output file and problems file must be different.");
                return 3;
            }

            EnsureDirectoryExists(outputPath);
            EnsureDirectoryExists(problemsPath);

            var options = new LogStandardizerOptions
            {
                DateFormat = dateFormat,
                SkipEmptyLines = true
            };

            var service = new LogStandardizerService(options);

            using var reader = new StreamReader(inputPath, Utf8NoBom);
            using var output = new StreamWriter(outputPath, false, Utf8NoBom);
            using var problems = new StreamWriter(problemsPath, false, Utf8NoBom);

            var result = service.Process(reader, output, problems);

            Console.WriteLine($"Processing finished. Valid: {result.ValidCount}, invalid: {result.InvalidCount}.");
            Console.WriteLine($"Output file: {Path.GetFullPath(outputPath)}");
            Console.WriteLine($"Problems file: {Path.GetFullPath(problemsPath)}");

            return 0;
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is ArgumentException ||
            ex is NotSupportedException ||
            ex is FormatException)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 4;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine();
        Console.WriteLine("LogStandardizer <inputFile> <outputFile> [problemsFile] [--date-format=yyyy-MM-dd]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("LogStandardizer input.log output.log");
        Console.WriteLine("LogStandardizer input.log output.log problems.txt");
        Console.WriteLine("LogStandardizer input.log output.log problems.txt --date-format=dd-MM-yyyy");
        Console.WriteLine();
        Console.WriteLine("Default problems file: problems.txt");
        Console.WriteLine("Default date format: yyyy-MM-dd");
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static bool AreSamePath(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}