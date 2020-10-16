using System;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class CheckBox : Label {
            public bool Checked;

            public CheckBox(Tuple<Func<bool>,Func<bool,bool>>_Var, string _text = "", bool _checked = false, Color? _fg = null) {
                this.AutoSize = true;
                this.Padding = new Padding(Style.PaddingSize / 2);
                this.ForeColor = _fg is Color ? (Color)_fg : Style.Colors.Text;
                this.Text = _text + "     ";
                this.Checked = _checked;
                this.Cursor = Cursors.Hand;
                this.Checked = _Var.Item1();
                this.Click += (s, e) => {
                    this.Checked = this.Checked ? false : true;
                    _Var.Item2(this.Checked);
                    this.Invalidate();
                };
            }

            protected override void OnPaint(PaintEventArgs e) {
                Font M = this.Font is Font ? this.Font : new Font(Style.Fonts.UI, Style.MenuTextSize);

                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, new Rectangle(
                        this.ClientRectangle.Height,
                        0,
                        this.ClientRectangle.Width,
                        this.ClientRectangle.Height
                    ), this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                int rs = Style.CheckBoxStroke;
                Rectangle rr = new Rectangle(
                    this.ClientRectangle.Height / 4,
                    this.ClientRectangle.Height / 4,
                    this.ClientRectangle.Height / 2,
                    this.ClientRectangle.Height / 2
                );

                Pen p = new Pen(new SolidBrush(this.BackColor), rs);

                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Style.Colors.TextDark), rs), rr);
                if (this.Checked) {
                    e.Graphics.FillRectangle(new SolidBrush(Style.Colors.TextDark), rr);
                    e.Graphics.DrawLine(
                        p,
                        new Point(rr.X + (int)(this.ClientRectangle.Height / 2.6), rr.Y + (int)(this.ClientRectangle.Height / 7.9)),
                        new Point(rr.X + (int)(this.ClientRectangle.Height / 5),   rr.Y + (int)(this.ClientRectangle.Height / 3))
                    );
                    e.Graphics.DrawLine(
                        p,
                        new Point(rr.X + (int)(this.ClientRectangle.Height / 5),   rr.Y + (int)(this.ClientRectangle.Height / 3)),
                        new Point(rr.X + (int)(this.ClientRectangle.Height / 10),  rr.Y + (int)(this.ClientRectangle.Height / 4.2))
                    );
                }
            }
        }

        public class But : Button {
            private bool _hovering;
            private Color HoverColor;
            public bool Selected;
            private bool isToggle;
            private Color SelectColor;

            public But(string _text = "", Color? _b = null, Color? _f = null, Color? _h = null, Size? _s = null, Point? _p = null, EventHandler _c = null, bool _toggle = false, Color? _select = null) {
                this.BackColor = _b is Color ? (Color)_b : Style.Colors.Light;
                this.ForeColor = _f is Color ? (Color)_f : Style.Colors.Text;
                this.HoverColor = _h is Color ? (Color)_h : Style.Colors.Lighter;
                this.AutoSize = true;
                this.Text = _text;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.MouseEnter += (s, e) => this._hovering = true;
                this.MouseLeave += (s, e) => this._hovering = false;
                if (_p is Point) this.Location = (Point)_p;
                if (_s is Size) this.Size = (Size)_s;
                if (_c != null) this.Click += _c;
                isToggle = _toggle;
                if (_select != null) this.SelectColor = (Color)_select;
                if (_toggle) this.Click += (s, e) => this.Selected = this.Selected ? false : true;
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                Font M = this.Font is Font ? this.Font : new Font(Style.Fonts.UI, Style.MenuTextSize);

                e.Graphics.FillRectangle(new SolidBrush(this.isToggle && this.Selected ? this.SelectColor : this._hovering ? this.HoverColor : this.BackColor), this.ClientRectangle);
                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }
    }
}