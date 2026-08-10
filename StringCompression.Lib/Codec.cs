using System.Globalization;
using System.Text;

namespace StringCompression.Lib;

/// <summary>
/// Реализует run-length encoding для строк, состоящих только
/// из строчных латинских букв.
/// </summary>
public static class Codec
{
    /// <summary>
    /// Сжимает строку, заменяя группы одинаковых символов
    /// символом и количеством повторений.
    /// Если группа состоит из одного символа, количество не указывается.
    /// </summary>
    /// <param name="source">Исходная строка из строчных латинских букв.</param>
    /// <returns>Сжатая строка.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если передан null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Возникает, если строка содержит не только строчные латинские буквы.
    /// </exception>
    public static string Compress(string source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder(source.Length);

        for (var i = 0; i < source.Length;)
        {
            var current = source[i];

            if (!IsLatinLowercase(current))
            {
                throw new ArgumentException(
                    "Input string must contain only lowercase Latin letters.",
                    nameof(source));
            }

            var count = 1;
            i++;

            while (i < source.Length && source[i] == current)
            {
                count++;
                i++;
            }

            result.Append(current);

            if (count > 1)
            {
                result.Append(count.ToString(CultureInfo.InvariantCulture));
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Восстанавливает исходную строку из сжатого представления.
    /// </summary>
    /// <param name="compressed">Сжатая строка.</param>
    /// <returns>Исходная строка.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если передан null.
    /// </exception>
    /// <exception cref="FormatException">
    /// Возникает, если сжатая строка имеет некорректный формат.
    /// </exception>
    public static string Decompress(string compressed)
    {
        if (compressed is null)
        {
            throw new ArgumentNullException(nameof(compressed));
        }

        if (compressed.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder(compressed.Length);

        for (var i = 0; i < compressed.Length;)
        {
            var current = compressed[i];

            if (!IsLatinLowercase(current))
            {
                throw new FormatException(
                    $"Invalid character at position {i}. Expected lowercase Latin letter.");
            }

            i++;

            var count = 0;
            var hasExplicitCount = false;

            while (i < compressed.Length && IsAsciiDigit(compressed[i]))
            {
                hasExplicitCount = true;
                var digit = compressed[i] - '0';

                if (count > (int.MaxValue - digit) / 10)
                {
                    throw new FormatException(
                        $"Run count near position {i} is too large.");
                }

                count = count * 10 + digit;
                i++;
            }

            if (!hasExplicitCount)
            {
                count = 1;
            }
            else if (count == 0)
            {
                throw new FormatException(
                    $"Run count near position {i} cannot be zero.");
            }

            // Защита от переполнения итоговой длины строки.
            if (count > int.MaxValue - result.Length)
            {
                throw new FormatException(
                    "Decompressed string length exceeds Int32.MaxValue.");
            }

            result.Append(current, count);
        }

        return result.ToString();
    }

    private static bool IsLatinLowercase(char c)
    {
        return c >= 'a' && c <= 'z';
    }

    private static bool IsAsciiDigit(char c)
    {
        return c >= '0' && c <= '9';
    }
}