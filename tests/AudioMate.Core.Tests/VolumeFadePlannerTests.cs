using AudioMate.Core.Audio;

namespace AudioMate.Core.Tests;

public sealed class VolumeFadePlannerTests
{
    [Fact]
    public void BuildProgressSteps_EndsAtOne()
    {
        var steps = VolumeFadePlanner.BuildProgressSteps(100, frameMilliseconds: 25);

        Assert.Equal([0.25f, 0.5f, 0.75f, 1f], steps);
    }

    [Theory]
    [InlineData(1f, 0.2f, 0.5f, 0.6f)]
    [InlineData(0.2f, 1f, 0.5f, 0.6f)]
    [InlineData(0.2f, 1f, 2f, 1f)]
    public void Interpolate_ReturnsClampedLinearValue(float start, float end, float progress, float expected)
    {
        Assert.Equal(expected, VolumeFadePlanner.Interpolate(start, end, progress), precision: 3);
    }
}
