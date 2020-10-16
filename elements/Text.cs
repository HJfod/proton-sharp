using System;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class NewLine : Label {
            public NewLine() {
                this.Size = new Size(Style.PaddingSize, Style.PaddingSize);
            }
        }
        public class Text : Label {
            public Text(string _text = "", bool _header = false, int _size = -1, bool _dark = false) {
                this.Text = _text;
                this.ForeColor = _dark ? Style.Colors.TextDark : Style.Colors.Text;
                this.AutoSize = true;
                this.FlatStyle = FlatStyle.Flat;
                this.Font = new Font(_header ? Style.Fonts.UIHead : Style.Fonts.UI, _size < 1 ? Style.UITextSize : _size);
            }
        }
    }
}