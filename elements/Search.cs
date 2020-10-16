using System;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class SearchBox : Panel {
            private Point? _moving;
            private TextBox t;
            public bool _regex = true;

            public string GetInput() {
                return this.t.Text;
            }

            public SearchBox(Control _par, EventHandler _typein, Point _loc) {
                int _s = Style._S(30);

                this.Size = new Size(Style.SearchBoxWidth, _s * 4);
                this.BackColor = Style.Colors.BGDark;
                this.DoubleBuffered = true;
                this.Location = _loc;
                
                this.MouseDown += (s, e) => this._moving = e.Location;
                this.MouseUp += (s, e) => this._moving = null;
                this.MouseMove += (s, e) => {
                    if (this._moving is Point) {
                        int newX = this.Location.X + e.X - ((Point)this._moving).X;
                        int newY = this.Location.Y  + e.Y - ((Point)this._moving).Y;
                        
                        if (newX > _par.ClientRectangle.Width - this.Width) newX = _par.ClientRectangle.Width - this.Width;
                        else if (newX < 0) newX = 0;
                        if (newY > _par.ClientRectangle.Height - this.Height) newY = _par.ClientRectangle.Height - this.Height;
                        else if (newY < Style.TitlebarSize) newY = Style.TitlebarSize;

                        this.Location = new Point(newX, newY);
                    }
                };

                this.t = new TextBox();
                this.t.Size = new Size(this.Width - Style.PaddingSize * 2, _s);
                this.t.Location = new Point(Style.PaddingSize, Style.PaddingSize);
                this.t.BackColor = Style.Colors.Light;
                this.t.MouseEnter += (s, e) => t.BackColor = Style.Colors.Lighter;
                this.t.MouseLeave += (s, e) => t.BackColor = Style.Colors.Light;
                this.t.BorderStyle = BorderStyle.None;
                this.t.Padding = Style.Padding;
                this.t.ForeColor = Style.Colors.Text;
                this.t.TextChanged += _typein;

                this.AddContextMenu(_par, new dynamic[] {
                    new {
                        Type = "Checkbox",
                        Text = "Search with Regex",
                        GetVar = new Func<bool>(() => this._regex),
                        SetVar = new Func<bool, bool>(_val => this._regex = _val)
                    },
                    new { Type = "Separator" },
                    new {
                        Name = "Reset location",
                        Click = new EventHandler((s, e) => this.Location = _loc)
                    },
                    new {
                        Name = "Close",
                        Click = new EventHandler((s, e) => this.Visible = false)
                    }
                });

                TextBox r = new TextBox();
                r.Size = new Size(this.Width - Style.PaddingSize * 2, _s);
                r.Location = new Point(Style.PaddingSize, Style.PaddingSize + _s);
                r.BackColor = Style.Colors.Light;
                r.MouseEnter += (s, e) => r.BackColor = Style.Colors.Lighter;
                r.MouseLeave += (s, e) => r.BackColor = Style.Colors.Light;
                r.BorderStyle = BorderStyle.None;
                r.Padding = Style.Padding;
                r.ForeColor = Style.Colors.Text;

                this.Controls.Add(this.t);
                this.Controls.Add(r);

                TableLayoutPanel p = new TableLayoutPanel();
                p.AutoSize = true;
                p.Size = new Size(this.Width - _s * 2, _s);
                p.Location = new Point(Style.PaddingSize, Style.PaddingSize + _s * 2);
                p.ColumnCount = 5;
                p.RowCount = 1;

                p.Controls.Add(new Elements.But(
                    "🠘", 
                    Style.Colors.Light,
                    null,
                    Style.Colors.Lighter,
                    new Size(_s / 2, _s / 2),
                    null,
                    new EventHandler((s, e) => {}))
                );
                p.Controls.Add(new Elements.But(
                    "🠚", 
                    Style.Colors.Light,
                    null,
                    Style.Colors.Lighter,
                    new Size(_s / 2, _s / 2),
                    null,
                    new EventHandler((s, e) => {}))
                );
                p.Controls.Add(new Elements.But(
                    "Replace", 
                    Style.Colors.Light,
                    null,
                    Style.Colors.Lighter,
                    new Size(_s / 2, _s / 2),
                    null,
                    new EventHandler((s, e) => {}))
                );
                p.Controls.Add(new Elements.But(
                    "Replace All", 
                    Style.Colors.Light,
                    null,
                    Style.Colors.Lighter,
                    new Size(_s / 2, _s / 2),
                    null,
                    new EventHandler((s, e) => {}))
                );
                p.Controls.Add(new Elements.But(
                    "✕", 
                    Style.Colors.Light,
                    null,
                    Style.Colors.Lighter,
                    new Size(_s / 2, _s / 2),
                    null,
                    new EventHandler((s, e) => this.Visible = false))
                );

                this.Controls.Add(p);

                this.Paint += (s, e) => {
                    e.Graphics.DrawRectangle(new Pen(new SolidBrush(Style.Colors.MenuBorder)), 0, 0, this.Width - 1, this.Height - 1);
                };
            }
        }
    }
}