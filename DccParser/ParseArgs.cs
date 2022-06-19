namespace DccParser;

public record ParseArgs
{
    /// <summary>
    /// Base64 of file content
    /// </summary>
    public string Buffer { get; init; }
}

public record ParseResponse
{
    /// <summary>
    /// Base64 of file content
    /// </summary>
    public string Dcc { get; init; }
}