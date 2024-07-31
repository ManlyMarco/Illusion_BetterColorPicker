using System;
using System.Collections;
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

namespace BetterColorPicker
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess(KKAPI.KoikatuAPI.GameProcessName)]
    [BepInProcess(KKAPI.KoikatuAPI.StudioProcessName)]
#if KK
    [BepInProcess(KKAPI.KoikatuAPI.GameProcessNameSteam)]
#endif
    public class BetterColorPicker : BaseUnityPlugin
    {
        public const string PluginName = "Better Color Picker";
        public const string GUID = "marco.better_color_picker";
        // Version of the plug-in. Must be in form: major.minor[.build][.revision]
        public const string Version = "3.1.1";

        private const string BtnText = "Pick color from desktop";
        private const string BtnTextActive = "* Press any key to finish *";

        private static Texture2D _lut;
        private static Action<Color> _setColor;
        private static TextMeshProUGUI _buttonText;
        private static BetterColorPicker _instance;

        private static bool _capturing;
        private static bool Capturing
        {
            get => _capturing;
            set
            {
                if (_capturing != value)
                {
                    _capturing = value;

                    if (_buttonText != null)
                        _buttonText.text = _capturing ? BtnTextActive : BtnText;

                    if (_capturing)
                        _instance.StartCoroutine(_instance.CaptureCo());
                    else
                        _instance.StopAllCoroutines();
                }
            }
        }

        public ConfigEntry<bool> ColorAdjust { get; private set; }

        private void Awake()
        {
            _instance = this;

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

        private IEnumerator CaptureCo()
        {
            while (Capturing)
            {
                yield return null;
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
        [HarmonyPatch(typeof(PickerSliderInput), nameof(PickerSliderInput.Start))]
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

            // Spawn a HEX textbox next to the HSV/RGB toggles
            var textboxCopy = GameObject.Instantiate(__instance.inputR.gameObject, __instance.transform.Find("ColorMode"), false);
            textboxCopy.name = "HexTextbox";

            var textboxRt = textboxCopy.GetComponent<RectTransform>();
            textboxRt.localPosition = new Vector3(195, 4, 0);
            textboxRt.sizeDelta = new Vector2(150, 28);

            var textbox = textboxCopy.GetComponent<TMP_InputField>();
            textbox.characterLimit = 9;
            textbox.characterValidation = TMP_InputField.CharacterValidation.None;
            textbox.contentType = TMP_InputField.ContentType.Standard;
            textbox.onValueChanged.ActuallyRemoveAllListeners();
            textbox.onValueChanged.AddListener(OnTextboxTextChanged);
            textbox.onSubmit.AddListener(OnTexboxSubmit);
            textbox.onDeselect.AddListener(OnTexboxSubmit);

            __instance.updateColorAction += UpdateTextboxHexText;

            var copyBtn = Instantiate(originalBtn, __instance.transform.Find("ColorMode"), false);
            copyBtn.name = "CopyHexButton";
            copyBtn.SetActive(true);

            var copyBtnRt = copyBtn.GetComponent<RectTransform>();
            copyBtnRt.localPosition = new Vector3(130, -24, 0);
            copyBtnRt.sizeDelta = new Vector2(65, 28);

            copyBtnRt.GetComponentInChildren<TextMeshProUGUI>().text = "Copy";

            var copyB = copyBtn.GetComponent<Button>();
            copyB.onClick.RemoveAllListeners();
            copyB.onClick.AddListener(() =>
            {
                if(Input.GetKey(KeyCode.LeftShift))
                    GUIUtility.systemCopyBuffer = textbox.text.Trim('#');
                else
                    GUIUtility.systemCopyBuffer = textbox.text.Substring(1, textbox.text.Length - 3);
            });

            // Probably unnecessary, but just in case
            UpdateTextboxHexText(__instance.color);

            bool VerifyColor(string hexStr, bool submit = false)
            {
                string altHexStr = "#" + hexStr.TrimStart('#');
                if (ColorUtility.TryParseHtmlString(hexStr, out var resultColor))
                {
                    if (submit) _setColor(resultColor);
                    return true;
                }
                else if (ColorUtility.TryParseHtmlString(altHexStr, out var resultColor2))
                {
                    if(submit) _setColor(resultColor2);
                    return true;
                }
                return false;
            }
            void OnTextboxTextChanged(string hexStr)
            {
                if(VerifyColor(hexStr))
                    textbox.targetGraphic.color = Color.white;
                else
                    textbox.targetGraphic.color = Color.red;
            }
            void UpdateTextboxHexText(Color newColor)
            {
                textbox.text = "#" + ColorUtility.ToHtmlStringRGBA(newColor);
            }
            void OnTexboxSubmit(string hexStr)
            {
                VerifyColor(hexStr, true);
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
    }
}
