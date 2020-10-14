using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class Option : Button {
            private bool _hovering = false;
            private string _shortcut = "";
            private Color HoverColor;

            public Option(string _txt, Color _c, Color _ch) {
                this.Text = _txt;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.TextAlign = ContentAlignment.BottomLeft;
                this.MouseEnter += (s, e) => this._hovering = true;
                this.MouseLeave += (s, e) => this._hovering = false;
                this.BackColor = _c;
                this.HoverColor = _ch;
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                if (this.Text.Contains("#")) {
                    this._shortcut = this.Text.Substring(this.Text.IndexOf("#") + 1);
                    this.Text = this.Text.Substring(0, this.Text.IndexOf("#"));
                }

                Rectangle r = new Rectangle(Style.MenuPadding, 0, this.Width - Style.MenuPadding * 2, this.Height);

                Font M = new Font(Style.Fonts.UI, Style.MenuTextSize);

                e.Graphics.FillRectangle(new SolidBrush(this._hovering ? this.HoverColor : this.BackColor), this.ClientRectangle);
                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, r, Style.Colors.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                StringFormat sf = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                sf.LineAlignment = StringAlignment.Center;
                if (this._shortcut != "")
                    e.Graphics.DrawString(this._shortcut, M, new SolidBrush(Style.Colors.TextDark), r, sf);
            }
        }

        public class Separator : Panel {
            public Separator(int _width, int _pad = 0, Color? _c = null) {
                this.Width = _width;
                this.Margin = new Padding(_pad);
                this.Height = 1;
                this.BackColor = _c is Color ? (Color)_c : Style.Colors.TextDark;
            }
        }
    }
    public class MenuWindow : TableLayoutPanel {
        public int ParseMenu(dynamic[] _menu, Control _par) {
            int maxLength = 0;
            foreach (dynamic Sub in _menu) {
                try {
                    string T = Sub.Type;
                    switch (T) {
                        case "Separator":
                            this.Controls.Add(new Elements.Separator(this.Width - Style.MenuPadding * 2, Style.MenuPadding, this.HoverColor));
                            break;
                    }
                } catch (Exception) {
                    Elements.Option b = new Elements.Option(Sub.Name, this.BackColor, this.HoverColor);
                    b.Size = new Size(this.Width, Style.MenuOptionSize);
                    b.AutoSize = true;

                    try {
                        dynamic[] subm = Sub.Menu;
                        b.Text = $"{b.Text}#\u2bc8";
                        b.Click += (s, e) => {
                            Main.CloseAllMenus(_par, this.Level + 1);

                            MenuWindow m = new MenuWindow(_par, subm, false,
                                new Point(this.Location.X + this.Width, this.Location.Y + b.Top), this.Level + 1);
                            _par.Controls.Add(m);
                            m.BringToFront();
                        };
                    } catch (Exception) {
                        b.Click += Sub.Click;
                        b.Click += (s, e) => {
                            try {
                                Sub.NoClose = true;
                            } catch (Exception) {
                                Main.CloseAllMenus(_par);
                            }
                        };
                    }

                    if (b.Text.Contains("#")) b.Text = b.Text.Replace("#", "   #");

                    this.Controls.Add(b);

                    if (b.ClientRectangle.Width > maxLength) maxLength = b.ClientRectangle.Width;
                }
            }
            return maxLength;
        }

        public int Level;
        public Color HoverColor;

        public MenuWindow(Control _par, dynamic[] _menu, bool _isTop = false, Point? _loc = null, int _level = 0) {
            this.Name = "__MenuWindow";
            this.BackColor = _isTop ? Style.Colors.TitlebarBG : Style.Colors.MenuBG;
            this.HoverColor = _isTop ? Style.Colors.TitlebarHover : Style.Colors.MenuHover;
            this.ForeColor = Style.Colors.Text;
            this.Level = _level;
            this.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Style.Colors.MenuBorder)), 0, 0, this.Width - 1, this.Height - 1);
            };
            
            this.Location = _loc == null ? new Point(
                Cursor.Position.X - _par.Left,
                Cursor.Position.Y - _par.Top
            ) : (Point)_loc;

            this.Size = new Size(Style.MenuDefWidth, 5);
            this.AutoSize = true;
            this.BringToFront();

            int max = this.ParseMenu(_menu, _par);

            _par.Controls.Add(this);

            foreach (Control b in this.Controls)
                b.Width = b is Elements.Separator ? max - Style.MenuPadding * 2 : max;

            if (this.Left + this.ClientRectangle.Width > _par.Width)
                this.Left -= this.ClientRectangle.Width;
            if (this.Top + this.ClientRectangle.Height > _par.Height)
                this.Top -= this.ClientRectangle.Height;
        }
    }
}