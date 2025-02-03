using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace ColorizeIndent
{
    internal class Option : BaseOptionModel<Option>
    {
        [Category("Color")]
        [DisplayName("ErrorColor")]
        [Description("Indent color for when there is an error in the indentation, for example if you have your tabs set to 2 spaces but the indent is 3 spaces.")]
        public Color ErrorColor { get; set; } = Color.FromArgb(0x30, 0xff, 0x00, 0x00);

        [Category("Color")]
        [DisplayName("Colors")]
        [Description("An array which are used as colors, can be any length.")]
        public Color[] Colors { get; set; } = Enumerable.Range(0, 7).Select(i => Color.FromArgb(0x20, 0x00, (byte)(35 * i), 0xff)).ToArray();

        [Category("Default")]
        [DisplayName("Indente Size")]
        [Description("When the indent depth is multiple of this number, Colors sets the background color. Otherwise, it will be ErrorColor.")]
        public byte IndentSize { get; set; } = 4;
    }
}
