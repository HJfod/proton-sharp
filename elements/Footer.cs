using System;
using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Footer : Panel {
            private Label w;

            public Footer() {
                this.Height = Style.FooterSize;
                this.BackColor = Style.Colors.FooterBG;

                this.w = new Label();
                w.AutoSize = true;
                w.ForeColor = Style.Colors.Text;
                this.Controls.Add(w);
            }

            public void UpdateWordCount(int _count) {
                w.Text = $"Words: {_count}";
            }
        }
    }
}