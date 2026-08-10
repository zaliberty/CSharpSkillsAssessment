using StringCompression.Lib;

namespace Tests;
public class CodecTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("aaabbcccdde", "a3b2c3d2e")]
    [InlineData("abc", "abc")]
    [InlineData("aaaaa", "a5")]
    public void Compress_ValidInput_ReturnsExpected(string input, string expected)
    {
        var actual = Codec.Compress(input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compress_LongRun_WritesMultiDigitCount()
    {
        var input = new string('x', 123);

        var actual = Codec.Compress(input);

        Assert.Equal("x123", actual);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("a3b2c3d2e", "aaabbcccdde")]
    [InlineData("abc", "abc")]
    [InlineData("a5", "aaaaa")]
    public void Decompress_ValidInput_ReturnsExpected(string input, string expected)
    {
        var actual = Codec.Decompress(input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Decompress_MultiDigitCount_ReturnsOriginalRun()
    {
        var expected = new string('x', 123);

        var actual = Codec.Decompress("x123");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Decompress_NonCanonicalExplicitOne_ReturnsSingleChar()
    {
        // Кодировщик никогда не должен писать "a1",
        // но декодер можно сделать устойчивым к такому входу.
        var actual = Codec.Decompress("a1");

        Assert.Equal("a", actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("aaabbcccdde")]
    [InlineData("abc")]
    [InlineData("aaaaaaaaaa")]
    public void RoundTrip_CompressThenDecompress_ReturnsOriginal(string original)
    {
        var compressed = Codec.Compress(original);
        var decompressed = Codec.Decompress(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void RoundTrip_LongString_ReturnsOriginal()
    {
        var original =
            new string('a', 15) +
            "b" +
            new string('c', 2) +
            new string('z', 100);

        var compressed = Codec.Compress(original);
        var decompressed = Codec.Decompress(compressed);

        Assert.Equal("a15bc2z100", compressed);
        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void Compress_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Codec.Compress(null!));
    }

    [Fact]
    public void Decompress_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Codec.Decompress(null!));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("a1")]
    [InlineData("ab cd")]
    [InlineData("абв")]
    public void Compress_InputOutsideAllowedAlphabet_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(
            () => Codec.Compress(input));
    }

    [Theory]
    [InlineData("3a")]
    [InlineData("a0")]
    [InlineData("A3")]
    [InlineData("a3B")]
    [InlineData("a-1")]
    [InlineData("a999999999999999999999999999")]
    public void Decompress_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(
            () => Codec.Decompress(input));
    }
}

