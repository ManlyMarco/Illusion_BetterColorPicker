using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using ChaCustom;
using HarmonyLib;
using Illusion.Component.UI.ColorPicker;
using IllusionUtility.GetUtility;
using KKAPI.Utilities;
using Studio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterColorPicker
{
    [BepInPlugin(GUID, "Better Color Picker", Version)]
    public class BetterColorPicker : BaseUnityPlugin
    {
        public const string GUID = "marco.better_color_picker";
        public const string Version = "2.0";

        private const string BtnText = "Pick color from desktop";
        private const string BtnTextActive = "* Press any key to finish *";

        private static Action<Color> _pickerSliderInput;
        private static TextMeshProUGUI _textMeshPro;

        private static bool _capturing;
        private static bool Capturing
        {
            get => _capturing;
            set
            {
                _capturing = value;
                if (_textMeshPro != null)
                    _textMeshPro.text = _capturing ? BtnTextActive : BtnText;
            }
        }

        public ConfigEntry<bool> ColorAdjust { get; private set; }

        /// <summary>
        /// Maker color picker
        /// </summary>
        [HarmonyPatch(typeof(CvsColor), "Start")]
        [HarmonyPostfix]
        public static void AddPickerButton(CvsColor __instance)
        {
            var originalBtn = GameObject.Find("Button/textDef").transform.parent;
            var newBtn = Instantiate(originalBtn, __instance.transform.Find("menuSlider"), true);
            newBtn.name = "CursorPickBtn";

            var rt = newBtn.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(-369, 10);
            rt.offsetMax = new Vector2(-20, -175);

            _textMeshPro = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            _textMeshPro.text = BtnText;

            var b = newBtn.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => Capturing = !Capturing);

            var input = __instance.GetComponentInChildren<PickerSliderInput>();
            if (input == null) throw new ArgumentNullException(nameof(input));
            _pickerSliderInput = color => input.color = color;
        }

        /// <summary>
        /// Studio color picker
        /// </summary>
        [HarmonyPatch(typeof(ColorPalette), "Awake")]
        [HarmonyPostfix]
        public static void AddPickerButtonStudio(ColorPalette __instance)
        {
            IEnumerator DelayedStudioInit()
            {
                yield return null;

                var originalBtn = __instance.transform.FindLoop("btnAllDelete");
                var slidersTop = __instance.transform.FindLoop("menuSlider");
                var newBtn = Instantiate(originalBtn, slidersTop.transform, true);
                newBtn.name = "CursorPickBtn";

                var rt = newBtn.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(1, 0);
                rt.offsetMin = new Vector2(15, 4);
                rt.offsetMax = new Vector2(-15, 35);

                _textMeshPro = newBtn.GetComponentInChildren<TextMeshProUGUI>();
                _textMeshPro.text = BtnText;

                var b = newBtn.GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => Capturing = !Capturing);

                var input = __instance.GetComponentInChildren<SampleColor>();
                if (input == null) throw new ArgumentNullException(nameof(input));
                _pickerSliderInput = color => input.UpdatePresetsColor(color);
            }

            ThreadingHelper.Instance.StartCoroutine(DelayedStudioInit());
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Capturing = false;
        }

        private void Start()
        {
            HarmonyWrapper.PatchAll(typeof(BetterColorPicker));

            _lut = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _lut.LoadImage(ResourceUtils.GetEmbeddedResource("lookuptexture.png"));

            ColorAdjust = Config.Bind("", "Adjust color to saturation filter", true, "When using default saturation filter the game colors are different than actual colors. " +
                                                                                       "Use this setting to adjust the color you capture to make it look correct under the saturation filter. " +
                                                                                       "If you do not use the saturation filter, disable this option to get the true color.");
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
            if (_pickerSliderInput != null)
            {
                var color = MouseColour.Get();
                color = ColorAdjust.Value ? LookupColor(color) : color;
                _pickerSliderInput(color);
            }
            else
            {
                Capturing = false;
            }
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

        private static Texture2D _lut;
    }
}
