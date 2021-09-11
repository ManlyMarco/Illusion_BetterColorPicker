using BetterColorPicker;
using System.Reflection;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(Info.Prefix + "_" + Info.PluginName + " (" + Info.GUID + ")")]
[assembly: AssemblyProduct(Info.Prefix + "_" + Info.PluginName)]
[assembly: AssemblyCopyright("Copyright © - ManlyMarco 2018-2021")]
[assembly: AssemblyVersion(Info.Version)]
[assembly: AssemblyFileVersion(Info.Version)]

#endregion Assembly attributes

namespace BetterColorPicker
{
    /// <summary>
    /// The main meta-data of the plug-in.
    /// This information is used for BepInEx plug-in meta-data.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// Human-readable name of the plug-in.
        /// </summary>
        public const string PluginName = "Better Color Picker";

        /// <summary>
        /// Unique ID of the plug-in.
        /// </summary>
        public const string GUID = "marco.better_color_picker";

        /// <summary>
        /// Version of the plug-in. Must be in form /<major/>./<minor/>./<build/>./<revision/>.
        /// Major and minor versions are mandatory, but build and revision can be left unspecified.
        /// </summary>
        public const string Version = "2.0.2.0";

#if KK
        internal const string Prefix = "KK";
#elif KKS
        internal const string Prefix = "KKS";
#endif

    }
}
