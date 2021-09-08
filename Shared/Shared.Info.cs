using BetterColorPicker;
using System.Reflection;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(Constants.Prefix + "_" + Info.PluginName + " (" + Info.GUID + ")")]
[assembly: AssemblyProduct(Constants.Prefix + "_" + Info.PluginName)]
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
        /// Human-readable name of the plug-in. In general, it should be short and concise.
        /// This is the name that is shown to the users who run BepInEx and to modders that inspect BepInEx logs.
        /// </summary>
        public const string PluginName = "Better Color Picker";

        /// <summary>
        /// Unique ID of the plug-in.
        /// This must be a unique string that contains only characters a-z, 0-9 underscores (_) and dots (.)
        /// Prefer using the reverse domain name notation: https://eqdn.tech/reverse-domain-notation/
        ///
        /// When creating Harmony patches, prefer using this ID for Harmony instances as well.
        /// </summary>
        public const string GUID = "marco.better_color_picker";

        /// <summary>
        /// Version of the plug-in. Must be in form /<major/>./<minor/>./<build/>./<revision/>.
        /// Major and minor versions are mandatory, but build and revision can be left unspecified.
        /// </summary>
        public const string Version = "2.0.2.0"; // Bump version inner working a bit different
    }
}