using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace proton {
    namespace Elements {
        public class TinyOption : Option {
            public TinyOption(string _txt, Color _c, Color _h, SHandler _e) : base(_txt, _c, _h) {
                this.Size = new Size(20, 20);
                this._Pad = 0;
                this._Centered = true;
                this.Click += (s, e) => _e(this, new SEventArgs(this.Text));
            }
        };
        
        public class Option : Button {
            private bool _hovering = false;
            private string _shortcut = "";
            private Color HoverColor;
            public int _Pad = Style.MenuPadding;
            public bool _Centered = false;

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

                Rectangle r = new Rectangle(_Pad, 0, this.Width - _Pad * 2, this.Height);

                Font M = new Font(Style.Fonts.UI, Style.MenuTextSize);

                e.Graphics.FillRectangle(new SolidBrush(this._hovering ? this.HoverColor : this.BackColor), this.ClientRectangle);
                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, r, Style.Colors.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                if (this._shortcut == "--BT" || this._shortcut == "--BF") {
                    int rs = Style.CheckBoxStroke;
                    Rectangle rr = new Rectangle(
                        this.ClientRectangle.Width - (int)(this.ClientRectangle.Height / 1.25),
                        this.Height / 4,
                        this.Height / 2,
                        this.Height / 2
                    );

                    e.Graphics.DrawRectangle(new Pen(new SolidBrush(Style.Colors.TextDark), rs), rr);
                    if (this._shortcut == "--BT") {
                        Pen p = new Pen(new SolidBrush(this.BackColor), rs);

                        e.Graphics.FillRectangle(new SolidBrush(Style.Colors.TextDark), rr);
                        e.Graphics.DrawLine(
                            p,
                            new Point(rr.X + (int)(this.Height / 2.4), rr.Y + (int)(this.Height / 7.5)),
                            new Point(rr.X + (int)(this.Height / 5),   rr.Y + (int)(this.Height / 3))
                        );
                        e.Graphics.DrawLine(
                            p,
                            new Point(rr.X + (int)(this.Height / 5),   rr.Y + (int)(this.Height / 3)),
                            new Point(rr.X + (int)(this.Height / 10),   rr.Y + (int)(this.Height / 4.2))
                        );
                    }
                } else {
                    StringFormat sf = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    sf.LineAlignment = StringAlignment.Center;
                    if (_Centered) sf.Alignment = StringAlignment.Center;
                    if (this._shortcut != "")
                        e.Graphics.DrawString(this._shortcut, M, new SolidBrush(Style.Colors.TextDark), r, sf);
                }
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
                        case "Table":
                            TableLayoutPanel p = new TableLayoutPanel();
                            p.AutoSize = true;
                            p.ColumnCount = Sub.Width;
                            p.RowCount = (int)Math.Ceiling((Decimal)Sub.Contents.Length / (Decimal)Sub.Width);
                            foreach (string s in Sub.Contents)
                                p.Controls.Add(new Elements.TinyOption(s, this.BackColor, this.HoverColor, Sub.Click));
                            this.Controls.Add(p);
                            break;
                        case "Checkbox":
                            Elements.Option b = new Elements.Option($"{Sub.Text}   #{(Sub.GetVar() ? "--BT" : "--BF")}", this.BackColor, this.HoverColor);
                            b.AutoSize = true;
                            b.Click += (s, e) => {
                                Sub.SetVar(!Sub.GetVar());
                                b.Text = $"{Sub.Text}   #{(Sub.GetVar() ? "--BT" : "--BF")}";
                                
                                try {
                                    bool x = Sub.NoClose;
                                } catch (Exception) {
                                    Main.CloseAllMenus(_par);
                                }
                            };
                            this.Controls.Add(b);

                            if (b.ClientRectangle.Width > maxLength) maxLength = b.ClientRectangle.Width;
                            break;
                        case "List":
                            Elements.Option list = new Elements.Option(Sub.Text, this.BackColor, this.HoverColor);
                            list.Size = new Size(this.Width, Style.MenuOptionSize);
                            list.AutoSize = true;

                            List<dynamic> items = new List<dynamic>();
                            foreach (dynamic item in Sub.List)
                                items.Add(new {
                                    Type = "Checkbox",
                                    Text = item.Text,
                                    GetVar = new Func<bool> (() => { return item.Encoding.Equals( Sub.GetVar() ); }),
                                    SetVar = new Func<bool, bool> (_val => Sub.SetVar(item.Encoding))
                                });
                            
                            list.Text = $"{list.Text}   #\u2bc8";
                            list.Click += (s, e) => {
                                Main.CloseAllMenus(_par, this.Level + 1);

                                MenuWindow m = new MenuWindow(_par, items.ToArray(), this.isTop,
                                    new Point(this.Location.X + this.Width, this.Location.Y + list.Top), this.Level + 1);
                                _par.Controls.Add(m);
                                m.BringToFront();
                            };
                            
                            this.Controls.Add(list);

                            if (list.ClientRectangle.Width > maxLength) maxLength = list.ClientRectangle.Width;
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

                            MenuWindow m = new MenuWindow(_par, subm, this.isTop,
                                new Point(this.Location.X + this.Width, this.Location.Y + b.Top), this.Level + 1);
                            _par.Controls.Add(m);
                            m.BringToFront();
                        };
                    } catch (Exception) {
                        b.Click += Sub.Click;
                        b.Click += (s, e) => {
                            try {
                                bool x = Sub.NoClose;
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
        public bool isTop = false;

        public MenuWindow(Control _par, dynamic[] _menu, bool _isTop = false, Point? _loc = null, int _level = 0) {
            this.Name = "__MenuWindow";
            this.BackColor = _isTop ? Elements.Titlebar.GetTitlebarColor() : Style.Colors.MenuBG;
            this.HoverColor = _isTop ? Style.Colors.TitlebarHover : Style.Colors.MenuHover;
            this.ForeColor = Style.Colors.Text;
            this.Level = _level;
            this.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Style.Colors.MenuBorder)), 0, 0, this.Width - 1, this.Height - 1);
            };
            isTop = _isTop;
            
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
                b.Width = b is Elements.TinyOption ? 10 : b is Elements.Separator ? max - Style.MenuPadding * 2 : max;

            if (this.Left + this.ClientRectangle.Width > _par.Width)
                this.Left -= this.ClientRectangle.Width;
            if (this.Top + this.ClientRectangle.Height > _par.Height)
                this.Top -= this.ClientRectangle.Height;
        }
    }
}