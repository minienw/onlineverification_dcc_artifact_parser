namespace DccParser;

public class CombinedParser
{
    private readonly ILogger<CombinedParser> _Logger;

    public CombinedParser(ILogger<CombinedParser> logger)
    {
        _Logger = logger;
    }

    public string Parse(byte[] buffer)
    {
        string result;
        if (TryParseAsPdf(buffer, out result))
        {
            _Logger.LogDebug($"Found in pdf: {result}.");
            return result;
        }

        using var file = new MemoryStream(buffer);
        if (TryParseAsImage(file, buffer.Length, out result))
        {
            _Logger.LogDebug($"Found in image: {result}.");
            return result;
        }

        return null;
    }

    private bool TryParseAsImage(Stream stream, long size, out string result)
    {
        _Logger.LogDebug("Try decoding as image...");
        stream.Position = 0;
        var p = new ImageParser();
        return p.TryParse(stream, size, out result);
    }

    private bool TryParseAsPdf(byte[] buffer, out string result)
    {
        _Logger.LogDebug("Try decoding as PDF...");
        var p = new PdfParser(buffer);
        return p.TryParse(out result);
    }
}