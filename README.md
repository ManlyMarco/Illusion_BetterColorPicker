# Better Color Picker for Koikatsu!
Plugin for some games by Illusion that adds ability to pick color from anywhere on desktop in character maker and studio. The selected color is adjusted to look correct under the game's saturation filter. This allows you to quickly and accurately transfer colors from reference image to your character.

![preview](https://user-images.githubusercontent.com/39247311/50303116-694b8400-048c-11e9-8ca0-e58175926cc1.PNG)

![preview animation](https://user-images.githubusercontent.com/39247311/50300415-a7dd4080-0484-11e9-89bb-b0483dcf9cd7.gif)

## How to use
0. Get latest version of BepInEx 5.x and ModdingAPI if you don't have them already.
1. Download the latest release and extract dll files to your BepInEx\plugins directory.
2. Start the main game and open character maker or start studio.
3. Edit any color to make color picker appear, then enter the "Sliders" tab and click on the "Pick color from desktop" button.
4. Hover your mouse over the color you want to use and press any button to finish picking. Pressing shift is recommended.

**Note:** If the game window loses focus the capture stops. You can't Alt-Tab to a different window. Open the reference image next to the game instead. If you are running the game in full screen and have only 1 display, use the ImageLoader plugin to view a reference image inside character maker to pick the colors.

**Note 2:** If you are not using the saturation filter or just don't want to use the color adjustment function, go to plugin settings and disable the "Adjust color to saturation filter" setting.

Inspired by the [Color Picker](https://koikoi.happy.nu/#!plugin_color_picker.md) plugin. Uses code from the [ColorAdjuster](https://koikoi.happy.nu/#!plugin_color_adjuster.md) plugin.
