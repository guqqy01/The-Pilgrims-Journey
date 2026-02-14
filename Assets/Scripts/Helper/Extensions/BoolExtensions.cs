public static class BoolExtensions
{
    public static bool Negative(this int value)
    {
        return (value < 0);
    }

    public static bool Positive(this int value)
    {
        return (value > 0);
    }
}
