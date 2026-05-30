using AudioMate.Core.Tts;

namespace AudioMate.Core.Tests;

public sealed class NarrationTextCondensorTests
{
    [Fact]
    public void Condense_CollapsesWhitespace()
    {
        var result = NarrationTextCondensor.Condense("AudioMate\r\n  queue\tready");

        Assert.Equal("AudioMate queue ready", result);
    }

    [Fact]
    public void Condense_TruncatesLongText()
    {
        var result = NarrationTextCondensor.Condense("1234567890", maxLength: 6);

        Assert.Equal("12345…", result);
    }
}
