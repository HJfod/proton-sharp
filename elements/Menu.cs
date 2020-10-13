using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class Option : Button {
            public Option(string _txt) {
                this.Text = _txt;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.TextAlign = ContentAlignment.BottomLeft;
            }
            protected override void OnPaint(PaintEventArgs e) {
                TextRenderer.DrawText(
                    e.Graphics,
                    this.Text,
                    Style.UIFont,
                    new Rectangle(0, 0, this.Width, this.Height),
                    Style.Colors.Text,
                    Style.Colors.MenuBG,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                
            }
        }
    }
    public class MenuWindow : TableLayoutPanel {
        public void ParseMenu(dynamic[] _menu, Control _par) {
            foreach (dynamic Sub in _menu) {
                Elements.Option b = new Elements.Option(Sub.Name);
                b.Size = new Size(this.Width, Style.MenuOptionSize);

                try {
                    dynamic[] subm = Sub.Menu;
                    b.Text = $"{b.Text}\u2bc8";
                    b.Click += (s, e) => {
                        MenuWindow m = new MenuWindow(_par, subm, false,
                            new Point(this.Location.X + this.Width, this.Location.Y));
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
                this.Controls.Add(b);
            }
        }

        public MenuWindow(Control _par, dynamic[] _menu, bool _isTop = false, Point? _loc = null) {
            this.Name = "__MenuWindow";
            this.BackColor = Style.Colors.MenuBG;
            this.ForeColor = Style.Colors.Text;
            
            this.Location = _loc == null ? new Point(
                Cursor.Position.X - _par.Left,
                Cursor.Position.Y - _par.Top
            ) : (Point)_loc;

            this.MinimumSize = new Size(1, 1);
            this.AutoSize = true;
            this.BringToFront();

            this.ParseMenu(_menu, _par);
        }
    }
}