using System.ComponentModel;
using System.Reflection;
using BepInEx;
using ChaCustom;
using Harmony;
using Illusion.Component.UI.ColorPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterColorPicker
{
    [BepInDependency("koikoi.happy.nu.color_adjuster")]
    [BepInProcess("Koikatu")]
    [BepInPlugin(GUID, "Better Color Picker", Version)]
    public class BetterColorPicker : BaseUnityPlugin
    {
        public const string GUID = "marco.better_color_picker";
        public const string Version = "1.0";

        private const string BtnText = "Pick color from desktop";
        private const string BtnTextActive = "Press any button to accept";

        private static PickerSliderInput _pickerSliderInput;
        private static TextMeshProUGUI _textMeshPro;

        private static bool capturing;
        public static bool Capturing
        {
            get => capturing;
            set
            {
                capturing = value;
                if (_textMeshPro != null)
                    _textMeshPro.text = capturing ? BtnTextActive : BtnText;
            }
        }

        [DisplayName("Adjust color to saturation filter")]
        [Description(
            "When using default saturation filter the game colors are different than actual colors. " +
            "Use this setting to adjust the color you capture to make it look correct under the saturation filter. " +
            "If you do not use the saturation filter, disable this option to get the true color.")]
        public ConfigWrapper<bool> ColorAdjust { get; private set; }

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

            _pickerSliderInput = (PickerSliderInput) __instance.GetType().GetField("cmpPickerSliderI", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Capturing = false;
        }

        private void Start()
        {
            ColorAdjust = new ConfigWrapper<bool>(nameof(ColorAdjust), this, true);

            HarmonyInstance.Create(GUID).PatchAll(typeof(BetterColorPicker));
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
                color = ColorAdjust.Value ? ColorAdjuster.ColorAdjuster.Lookup(color) : color;
                _pickerSliderInput.color = color;
            }
            else
            {
                Capturing = false;
            }
        }
    }
}
