namespace Tests;

public class CountServerTests
{
    public CountServerTests()
    {
        CountServer.ResetForTests();
    }

    [Fact]
    public void GetCount_Initially_ReturnsZero()
    {
        var actual = CountServer.GetCount();

        Assert.Equal(0, actual);
    }

    [Fact]
    public void AddToCount_PositiveValue_IncreasesCount()
    {
        CountServer.AddToCount(5);

        Assert.Equal(5, CountServer.GetCount());
    }

    [Fact]
    public void AddToCount_NegativeValue_DecreasesCount()
    {
        CountServer.AddToCount(10);
        CountServer.AddToCount(-3);

        Assert.Equal(7, CountServer.GetCount());
    }

    [Fact]
    public void AddToCount_MultipleCalls_AccumulatesValues()
    {
        CountServer.AddToCount(1);
        CountServer.AddToCount(2);
        CountServer.AddToCount(3);

        Assert.Equal(6, CountServer.GetCount());
    }

    [Fact]
    public void AddToCount_ConcurrentWriters_NoLostUpdates()
    {
        const int writers = 16;
        const int iterations = 50000;

        Parallel.For(0, writers, _ =>
        {
            for (var i = 0; i < iterations; i++)
            {
                CountServer.AddToCount(1);
            }
        });

        Assert.Equal(writers * iterations, CountServer.GetCount());
    }

    [Fact]
    public async Task GetCount_ConcurrentWithWrites_ReturnsValidValues()
    {
        const int writes = 100000;
        var readOperations = 0;

        var writer = Task.Run(() =>
        {
            for (var i = 0; i < writes; i++)
            {
                CountServer.AddToCount(1);
            }
        });

        var readers = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() =>
            {
                do
                {
                    var value = CountServer.GetCount();

                    Assert.InRange(value, 0, writes);

                    Interlocked.Increment(ref readOperations);
                }
                while (!writer.IsCompleted);
            }))
            .ToArray();

        await writer;
        await Task.WhenAll(readers);

        Assert.Equal(writes, CountServer.GetCount());
        Assert.True(readOperations > 0);
    }
}


