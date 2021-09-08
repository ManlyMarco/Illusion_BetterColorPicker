/// <summary>
/// Useful constants definitions controlled by conditional compilation
/// </summary>

internal static class Constants
{
#if KK
    internal const string Prefix = "KK";
    internal const string GameName = "Koikatsu";
    internal const string StudioProcessName = "CharaStudio";
    internal const string MainGameProcessName = "Koikatu";
    internal const string MainGameProcessNameSteam = "Koikatsu Party";
    internal const string VRProcessName = "KoikatuVR";
    internal const string VRProcessNameSteam = "Koikatsu Party VR";
#elif KKS
    internal const string Prefix = "KKS";
    internal const string GameName = "Koikatsu Sunshine";
    internal const string MainGameProcessName = "KoikatsuSunshine";
#endif
}
