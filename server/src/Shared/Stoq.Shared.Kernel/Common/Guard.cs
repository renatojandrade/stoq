namespace Stoq.Shared.Kernel.Common;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{paramName}' must not be null or whitespace.", paramName);
        }

        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }

    public static Guid AgainstEmptyGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"'{paramName}' must not be an empty GUID.", paramName);
        }

        return value;
    }

    public static int AgainstNegativeOrZero(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be positive.");
        }

        return value;
    }
}
