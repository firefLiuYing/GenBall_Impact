namespace Yueyn.Main
{
    /// <summary>
    /// Global time-scale manager. Multiple sources can request slowdown/pause.
    /// The actual time scale is determined by the most restrictive active request.
    /// </summary>
    public interface ITimeScaleSystem : ISystem
    {
        /// <summary>Request a time scale. Higher priority wins on conflict.</summary>
        /// <returns>A handle that must be passed to ReleaseRequest.</returns>
        object Request(object source, float targetScale, int priority = 0);

        /// <summary>Release a previously requested time scale.</summary>
        void ReleaseRequest(object handle);

        /// <summary>Current effective time scale (1.0 = normal).</summary>
        float EffectiveScale { get; }
    }
}
