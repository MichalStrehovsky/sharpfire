// Tiny and not-so-great random number generator.
// We want to avoid System.Random.
struct MiniRandom
{
    private uint _val;

    public MiniRandom(uint seed)
    {
        _val = seed;
    }

    public uint Next() => _val = (1103515245 * _val + 12345) % 2147483648;
}
