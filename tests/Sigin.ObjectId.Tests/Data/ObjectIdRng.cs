namespace Sigin.ObjectId.Tests.Data;

public class ObjectIdRng
{
    private const long A = 25214903917;
    private const long C = 11;
    private long _seed;

    public ObjectIdRng(long seed)
    {
        if (seed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seed), seed, $"'{seed}' must be a non-negative value.");
        }

        _seed = seed;
    }

    private int Next(int bits) // helper
    {
        _seed = (_seed * A + C) & ((1L << 48) - 1);
        return (int) (_seed >> (48 - bits));
    }

    public unsafe int Next()
    {
        var resultDouble = (((long) Next(26) << 27) + Next(27)) / (double) (1L << 53);
        var resultDoublePtr = &resultDouble;
        var resultInt32Ptr = (int*) resultDoublePtr;
        var hi = resultInt32Ptr[0];
        var lo = resultInt32Ptr[1];
        var result = hi ^ lo;
        return result;
    }
}
