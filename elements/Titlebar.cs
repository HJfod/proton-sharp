using System;
using System.Drawing;
using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Titlebar : Panel {
            public Titlebar() {}

            public void AddControlButton (string _Text, EventHandler _Click) {
                Button C = new Button();
                C.Text = _Text;
                C.Anchor = (AnchorStyles.Right);
                C.Dock = DockStyle.Right;
                C.ForeColor = Style.Colors.Text;
                C.Size = new Size((int)((double)Style.TitlebarSize * 1.5), Style.TitlebarSize);
                C.Font = new Font("Segoe UI", Style.TitlebarSize / (int)(3F * Style.Scale));
                C.FlatStyle = FlatStyle.Flat;
                C.FlatAppearance.BorderSize = 0;

                if (_Click != null) C.Click += _Click;

                this.Controls.Add(C);
            }
        }
    }
}