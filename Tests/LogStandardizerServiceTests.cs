using LogStandardizer.Lib;

namespace Tests;

public class LogStandardizerServiceTests
{
    private static LogStandardizerOptions DefaultOptions => new()
    {
        DateFormat = "yyyy-MM-dd",
        SkipEmptyLines = true
    };

    private static (string Output, string Problems) Process(
        string input,
        LogStandardizerOptions? options = null)
    {
        var service = new LogStandardizerService(options ?? DefaultOptions);

        using var reader = new StringReader(input);
        using var output = new StringWriter();
        using var problems = new StringWriter();

        service.Process(reader, output, problems);

        return (
            output.ToString().TrimEnd('\r', '\n'),
            problems.ToString().TrimEnd('\r', '\n'));
    }

    [Fact]
    public void Format1_ValidLine_ConvertsToStandardFormat()
    {
        var input =
            "10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'";

        var (output, problems) = Process(input, DefaultOptions);

        Assert.Equal(
            "2025-03-10\t15:14:49.523\tINFO\tDEFAULT\tВерсия программы: '3.4.0.48729'",
            output);

        Assert.Equal(string.Empty, problems);
    }

    [Fact]
    public void Format1_ValidLine_WithDateFormatFromSpec_ConvertsToDdMmYyyy()
    {
        var input =
            "10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'";

        var options = new LogStandardizerOptions
        {
            DateFormat = "dd-MM-yyyy",
            SkipEmptyLines = true
        };

        var (output, _) = Process(input, options);

        Assert.Equal(
            "10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tВерсия программы: '3.4.0.48729'",
            output);
    }

    [Fact]
    public void Format2_ValidLine_CopiesMethod()
    {
        var input =
            "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: '@MINDEO-M40-D-410244015546'";

        var (output, problems) = Process(input, DefaultOptions);

        Assert.Equal(
            "2025-03-10\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tКод устройства: '@MINDEO-M40-D-410244015546'",
            output);

        Assert.Equal(string.Empty, problems);
    }

    [Theory]
    [InlineData("INFORMATION", "INFO")]
    [InlineData("INFO", "INFO")]
    [InlineData("WARNING", "WARN")]
    [InlineData("WARN", "WARN")]
    [InlineData("ERROR", "ERROR")]
    [InlineData("DEBUG", "DEBUG")]
    public void Levels_AreMappedCorrectly(string inputLevel, string expectedLevel)
    {
        var input =
            $"10.03.2025 15:14:49.523 {inputLevel} Some message";

        var (output, _) = Process(input, DefaultOptions);

        Assert.Equal(
            $"2025-03-10\t15:14:49.523\t{expectedLevel}\tDEFAULT\tSome message",
            output);
    }

    [Fact]
    public void Format2_EmptyMethod_UsesDefault()
    {
        var input =
            "2025-03-10 15:14:51.5882|INFO|11|| Some message";

        var (output, _) = Process(input, DefaultOptions);

        Assert.Equal(
            "2025-03-10\t15:14:51.5882\tINFO\tDEFAULT\tSome message",
            output);
    }

    [Fact]
    public void Format2_MessageWithPipe_PreservesPipeInsideMessage()
    {
        var input =
            "2025-03-10 15:14:51.5882|INFO|11|MobileComputer.GetDeviceId| msg | extra";

        var (output, _) = Process(input, DefaultOptions);

        Assert.Equal(
            "2025-03-10\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tmsg | extra",
            output);
    }

    [Fact]
    public void InvalidLine_WritesToProblems()
    {
        var input = "not a log line";

        var (output, problems) = Process(input, DefaultOptions);

        Assert.Equal(string.Empty, output);
        Assert.Equal("not a log line", problems);
    }

    [Fact]
    public void UnknownLevel_WritesToProblems()
    {
        var input =
            "10.03.2025 15:14:49.523 TRACE Some message";

        var (output, problems) = Process(input, DefaultOptions);

        Assert.Equal(string.Empty, output);
        Assert.Equal(input, problems);
    }

    [Fact]
    public void EmptyLine_IsSkippedByDefault()
    {
        var input = string.Empty;

        var (output, problems) = Process(input, DefaultOptions);

        Assert.Equal(string.Empty, output);
        Assert.Equal(string.Empty, problems);
    }

    [Fact]
    public void MixedLines_ValidAndInvalid_AreSeparated()
    {
        var input = string.Join(
            Environment.NewLine,
            "10.03.2025 15:14:49.523 INFORMATION msg1",
            "bad line",
            "2025-03-10 15:14:51.5882|INFO|11|Method| msg2");

        var (output, problems) = Process(input, DefaultOptions);

        var outputLines = output.Split(Environment.NewLine);

        Assert.Equal(2, outputLines.Length);

        Assert.Equal(
            "2025-03-10\t15:14:49.523\tINFO\tDEFAULT\tmsg1",
            outputLines[0]);

        Assert.Equal(
            "2025-03-10\t15:14:51.5882\tINFO\tMethod\tmsg2",
            outputLines[1]);

        Assert.Equal("bad line", problems);
    }
}

