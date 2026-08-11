namespace LogStandardizer.Lib;

public class LogStandardizerOptions
{
    /// <summary>
    /// В ТЗ есть противоречие:
    /// текст требует dd-MM-yyyy, а примеры показывают yyyy-MM-dd.
    ///
    /// Значение по умолчанию можно поменять здесь
    /// или передать через аргумент командной строки --date-format.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Пустые строки обычно не являются записями лога.
    /// Если нужно считать их невалидными и писать в problems.txt,
    /// установите false.
    /// </summary>
    public bool SkipEmptyLines { get; set; } = true;
}
