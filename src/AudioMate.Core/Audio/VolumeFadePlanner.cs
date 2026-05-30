namespace AudioMate.Core.Audio;

public static class VolumeFadePlanner
{
    public static IReadOnlyList<float> BuildProgressSteps(int durationMilliseconds, int frameMilliseconds = 25)
    {
        if (durationMilliseconds <= 0)
        {
            return [1f];
        }

        var stepCount = Math.Clamp(durationMilliseconds / Math.Max(1, frameMilliseconds), 1, 40);
        var steps = new float[stepCount];

        for (var index = 0; index < stepCount; index++)
        {
            steps[index] = (index + 1f) / stepCount;
        }

        return steps;
    }

    public static float Interpolate(float start, float end, float progress)
    {
        return Math.Clamp(start + ((end - start) * Math.Clamp(progress, 0, 1)), 0, 1);
    }
}
