using System.IO;
using System.Text.RegularExpressions;
using DccParser;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using Xunit;

namespace CheckInQrWebTests;

public class CombinedParserTests
{
    private static string GetProjectFileName(string name)
    {
        return Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath()), name);
    }

    [ExclusivelyUses("files")]
    [InlineData("incomplete vacc 1_4_2022.pdf")]
    [InlineData("bob bouwer 2012_onvolledig dob.pdf")]
    [InlineData("Bobby.JPG")]
    [InlineData("Bob_Bouwer_1-1-1960_3_3.JPG")]
    [InlineData("Erikea Musermann 12-8-1964.png")]
    //[InlineData("bobby_bouwer is ouwer - fails AF.png")] Dodgy!
    //[InlineData("bobby_bouwer is ouwer - cropped.png")] Dodgy!
    [InlineData("Something dodgy VAC_DE.png")]
    [InlineData("bobby_bower_2021_minor.png")]
    [InlineData("ron de boer.png")]
    [InlineData("Bob_bouwer_2021_minor.pdf")]
    [InlineData("Bobby Bouwer 1960 oudere vacc.pdf")]
    [InlineData("Test Cert SK 2022-01.pdf")]
    [InlineData("Vacc Cert SK 2022-01.pdf")]
    [Theory]
    public void Final(string name)
    {
        var filename = GetProjectFileName(name);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<CombinedParser>();
        var l = File.ReadAllBytes(filename).LongLength;
        var buffer = File.ReadAllBytes(filename);
        var parser = new CombinedParser(logger);
        Assert.StartsWith("HC1:", parser.Parse(buffer));
    }
}



