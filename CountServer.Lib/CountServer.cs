public static class CountServer
{
    private static int _count;

    private static readonly ReaderWriterLockSlim _sync =
        new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    public static int GetCount()
    {
        _sync.EnterReadLock();
        try
        {
            return _count;
        }
        finally
        {
            _sync.ExitReadLock();
        }
    }

    public static void AddToCount(int value)
    {
        _sync.EnterWriteLock();
        try
        {
            _count = checked(_count + value);
        }
        finally
        {
            _sync.ExitWriteLock();
        }
    }


    public static void ResetForTests()
    {
        _sync.EnterWriteLock();
        try
        {
            _count = 0;
        }
        finally
        {
            _sync.ExitWriteLock();
        }
    }
}

