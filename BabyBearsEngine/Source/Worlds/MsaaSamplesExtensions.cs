namespace BabyBearsEngine.Worlds;

public static class MsaaSamplesExtensions
{
    /// <summary>
    /// Returns the highest <see cref="MsaaSamples"/> value not exceeding the requested value
    /// or the supplied GPU maximum. Used to gracefully degrade an MSAA request that exceeds
    /// what the GPU can do (e.g. requesting 8x on hardware capped at 2x returns
    /// <see cref="MsaaSamples.X2"/>). When <paramref name="requested"/> is
    /// <see cref="MsaaSamples.Disabled"/> the result is always <see cref="MsaaSamples.Disabled"/>
    /// regardless of <paramref name="maxSamples"/>. Otherwise, a <paramref name="maxSamples"/>
    /// below 2 returns <see cref="MsaaSamples.Disabled"/>.
    /// </summary>
    public static MsaaSamples ClampToMax(this MsaaSamples requested, int maxSamples)
    {
        if ((int)requested <= maxSamples)
        {
            return requested;
        }

        MsaaSamples best = MsaaSamples.Disabled;
        foreach (MsaaSamples sample in Enum.GetValues<MsaaSamples>())
        {
            if ((int)sample <= maxSamples && (int)sample > (int)best)
            {
                best = sample;
            }
        }
        return best;
    }
}
