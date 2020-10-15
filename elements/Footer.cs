using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class Footer : Panel {
            private Label w;
            private Dictionary<string, int> counts = new Dictionary<string, int> ();

            public Footer() {
                this.Height = Style.FooterSize;
                this.BackColor = Style.Colors.FooterBG;
                this.ForeColor = Style.Colors.Text;

                this.w = new Label();
                w.AutoSize = true;
                w.ForeColor = Style.Colors.Text;
                this.Controls.Add(w);
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                Font M = new Font(Style.Fonts.UI, Style.MenuTextSize);

                e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);

                StringFormat f = new StringFormat();
                f.LineAlignment = StringAlignment.Center;
                f.Alignment = StringAlignment.Center;

                Rectangle StatRect = this.ClientRectangle;

                int i = 0, l = counts.Count;
                foreach (KeyValuePair<string, int> _c in this.counts) {
                    e.Graphics.DrawString(
                        $"{_c.Key}: {_c.Value}",
                        M,
                        new SolidBrush(this.ForeColor),
                        new Rectangle(StatRect.Width / l * i, 0, StatRect.Width / l, StatRect.Height),
                        f
                    );
                    i++;
                }
            }

            public void UpdateWordCount(int _count, int _chars, int _charsnospace) {
                this.counts["Words"] = _count;
                this.counts["Chars"] = _chars;
                this.counts["Chars (Without Spaces)"] = _charsnospace;
                this.Invalidate();
            }
        }
    }
}