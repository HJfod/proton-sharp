using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Titlebar : Panel {
            public static string bc = "";
            private int[] grad = new int[] {-1, 1};

            public Titlebar() {}

            public Brush GetTitlebarBG() {
                if (bc != "")
                    return new LinearGradientBrush(
                        new Rectangle(
                            this.ClientRectangle.X + grad[0],
                            this.ClientRectangle.Y,
                            this.ClientRectangle.Width - grad[0] - grad[1],
                            this.ClientRectangle.Height
                        ),
                        ColorTranslator.FromHtml(bc.Substring(0, bc.IndexOf(">")).Trim()),
                        ColorTranslator.FromHtml($"#{bc.Substring(bc.IndexOf(">") + 2).Trim()}"),
                        0F
                    );
                return new SolidBrush(this.BackColor);
            }

            public static Color GetTitlebarColor(bool _right = false) {
                if (bc != "")
                    return _right ? ColorTranslator.FromHtml($"#{bc.Substring(bc.IndexOf(">") +2).Trim()}") :
                    ColorTranslator.FromHtml(bc.Substring(0, bc.IndexOf(">")).Trim());
                return Style.Colors.TitlebarBG;
            }

            protected override void OnPaintBackground(PaintEventArgs e) {
                e.Graphics.FillRectangle(new SolidBrush(GetTitlebarColor(true)), new Rectangle(
                    this.ClientRectangle.X + this.ClientRectangle.Width / 2,
                    this.ClientRectangle.Y,
                    this.ClientRectangle.Width / 2,
                    this.ClientRectangle.Height
                ));
                e.Graphics.FillRectangle(new SolidBrush(GetTitlebarColor()), new Rectangle(
                    this.ClientRectangle.X,
                    this.ClientRectangle.Y,
                    this.ClientRectangle.Width / 2,
                    this.ClientRectangle.Height
                ));
                e.Graphics.FillRectangle(GetTitlebarBG(), new Rectangle(
                    this.ClientRectangle.X + grad[0],
                    this.ClientRectangle.Y,
                    this.ClientRectangle.Width - grad[0] - grad[1],
                    this.ClientRectangle.Height
                ));
            }

            public void AddControlButton (string _Text, Form _par, EventHandler _Click, Color? _ch = null) {
                Elements.But C = new Elements.But(_Text, GetTitlebarColor(true), Style.Colors.Text, _ch is Color ? (Color)_ch : Style.Colors.TitlebarHover);
                C.Anchor = (AnchorStyles.Right);
                C.Dock = DockStyle.Right;
                C.Size = new Size((int)((double)Style.TitlebarSize * 1.5), Style.TitlebarSize);
                C.Font = new Font(Style.Fonts.UI, Style.TitlebarSize / (int)(3F * Style.Scale));
                C.FlatStyle = FlatStyle.Flat;
                C.FlatAppearance.BorderSize = 0;

                if (_Click != null) C.Click += _Click;

                _par.Activated += (s, e) => C.ForeColor = Style.Colors.Text;
                _par.Deactivate += (s, e) => C.ForeColor = Style.Colors.TextDark;

                this.Controls.Add(C);

                this.grad[1] += C.ClientRectangle.Width;
            }

            public void AddTitle(string _title, Form _par) {
                Label t = new Label();
                t.MinimumSize = new Size(10, Style.TitlebarSize);
                t.AutoSize = true;
                t.Location = new Point(Style.PaddingSize / 2, 0);
                t.Text = _title;
                t.ForeColor = Style.Colors.Text;
                t.BackColor = GetTitlebarColor();
                t.Font = new Font(Style.Fonts.UI, Style.UITextSize / 1.1F);
                t.TextAlign = ContentAlignment.MiddleLeft;

                _par.Activated += (s, e) => t.ForeColor = Style.Colors.Text;
                _par.Deactivate += (s, e) => t.ForeColor = Style.Colors.TextDark;

                this.Controls.Add(t);

                this.grad[0] += t.ClientRectangle.Width;
            }

            public Button AddMenuButton (string _Name, Form _par) {
                Elements.But C = new Elements.But(_Name, GetTitlebarColor(), Style.Colors.Text, Style.Colors.TitlebarHover);
                C.Anchor = (AnchorStyles.Left);
                C.Dock = DockStyle.Left;
                C.Size = new Size(10, Style.TitlebarSize);
                C.AutoSize = true;
                C.FlatStyle = FlatStyle.Flat;
                C.FlatAppearance.BorderSize = 0;

                _par.Activated += (s, e) => C.ForeColor = Style.Colors.Text;
                _par.Deactivate += (s, e) => C.ForeColor = Style.Colors.TextDark;;

                this.Controls.Add(C);

                this.grad[0] += C.ClientRectangle.Width;

                return C;
            }
        }
    }
}