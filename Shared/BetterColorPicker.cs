using System;
using BepInEx;
using BepInEx.Configuration;
using ChaCustom;
using HarmonyLib;
using Illusion.Component.UI.ColorPicker;
using IllusionUtility.GetUtility;
using KKAPI.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using PluginInfo = BetterColorPicker.Info;

namespace BetterColorPicker
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInPlugin(PluginInfo.GUID, "Better Color Picker", PluginInfo.Version)]
    [BepInProcess(KKAPI.KoikatuAPI.GameProcessName)]
#if KK
    [BepInProcess(KKAPI.KoikatuAPI.GameProcessNameSteam)]
    [BepInProcess(KKAPI.KoikatuAPI.StudioProcessName)]
#endif
    public class BetterColorPicker : BaseUnityPlugin
    {
        public const string GUID = "marco.better_color_picker";
        public const string Version = "2.0.2";

        private const string BtnText = "Pick color from desktop";
        private const string BtnTextActive = "* Press any key to finish *";

        private static Texture2D _lut;
        private static Action<Color> _setColor;
        private static TextMeshProUGUI _buttonText;

        private static bool _capturing;
        private static bool Capturing
        {
            get => _capturing;
            set
            {
                _capturing = value;
                if (_buttonText != null)
                    _buttonText.text = _capturing ? BtnTextActive : BtnText;
            }
        }

        public ConfigEntry<bool> ColorAdjust { get; private set; }

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(BetterColorPicker));

            _lut = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _lut.LoadImage(ResourceUtils.GetEmbeddedResource("lookuptexture.png"));

            ColorAdjust = Config.Bind("", "Adjust color to saturation filter", true,
                "When using default saturation filter the game colors are different than actual colors. " +
                "Use this setting to adjust the color you capture to make it look correct under the saturation filter. " +
                "If you do not use the saturation filter, disable this option to get the true color.");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Capturing = false;
        }

        private void Update()
        {
            if (Capturing)
            {
                UpdateColorToPointer();
                if (Input.anyKeyDown)
                    Capturing = false;
            }
        }

        private void UpdateColorToPointer()
        {
            if (_setColor != null)
            {
                var color = MouseColour.Get();
                color = ColorAdjust.Value ? LookupColor(color) : color;
                _setColor(color);
            }
            else
                Capturing = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PickerSliderInput), "Start")]
        public static void AddPickerButton(PickerSliderInput __instance)
        {
            if (!__instance.GetComponentInParent<CvsColor>() && !__instance.GetComponentInParent<Studio.ColorPalette>()) return;

            var colorUiRoot = __instance.transform.parent;

            var originalBtn = colorUiRoot.FindLoop("btnAllDelete");
            var slidersTop = colorUiRoot.FindLoop("menuSlider");
            var newBtn = Instantiate(originalBtn, slidersTop.transform, true);
            newBtn.name = "CursorPickBtn";
            newBtn.SetActive(true);

            var rt = newBtn.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 0);
            rt.offsetMin = new Vector2(15, 4);
            rt.offsetMax = new Vector2(-15, 35);

            _buttonText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            _buttonText.text = BtnText;

            var b = newBtn.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => Capturing = !Capturing);

            _setColor = color => __instance.color = color;
        }

        /// <summary>
        /// Code and lut texture from koikoi.happy.nu.color_adjuster
        /// </summary>
        private static Color LookupColor(Color color)
        {
            var num = color.b * 63f;
            var num2 = Mathf.Floor(Mathf.Floor(num) / 8f);
            var num3 = Mathf.Floor(num) - num2 * 8f;
            var num4 = Mathf.Floor(Mathf.Ceil(num) / 8f);
            var num5 = Mathf.Ceil(num) - num4 * 8f;
            var num6 = num3 * 0.125f + 0.0009765625f + 0.123046875f * color.r;
            var num7 = num2 * 0.125f + 0.0009765625f + 0.123046875f * color.g;
            num7 = 1f - num7;
            var num8 = num5 * 0.125f + 0.0009765625f + 0.123046875f * color.r;
            var num9 = num4 * 0.125f + 0.0009765625f + 0.123046875f * color.g;
            num9 = 1f - num9;
            var pixel = _lut.GetPixel((int)(num6 * 512f), (int)(num7 * 512f));
            var pixel2 = _lut.GetPixel((int)(num8 * 512f), (int)(num9 * 512f));
            return Color.Lerp(pixel, pixel2, Mathf.Repeat(num, 1f));
        }
    }
}
