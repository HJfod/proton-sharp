using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;
using BorderlessResizer;

namespace proton {
    namespace Elements {
        public class TabShadow : Panel {
            public TabShadow() {
                this.DoubleBuffered = true;
                this.Visible = false;
                this.ForeColor = Style.Colors.Text;
                this.BackColor = Style.Colors.Main;

                GlobalMouseHandler.SuperMouseClick += (p, d, r) => this.Visible = false;
            }

            public void Set(Point _p, Size _s, string _t) {
                this.Location = _p;
                this.Size = _s;
                this.Visible = true;
                this.Text = _t;
                this.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                e.Graphics.FillRectangle(new LinearGradientBrush(
                        this.ClientRectangle,
                        Style.Colors.Main,
                        Style.Colors.Secondary,
                        this.Text == "" ? 90F : 0F
                    ), this.ClientRectangle);

                TextRenderer.DrawText(
                    e.Graphics,
                    this.Text,
                    new Font(Style.Fonts.UI, Style.MenuTextSize),
                    this.ClientRectangle,
                    this.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        public class Tab : Button {
            private bool _hovering = false;
            private Color HoverColor;
            public string FileName = "";
            public string FilePath = "";
            public string FileContent = "";
            public Encoding Encoding = Encoding.UTF8;
            public bool Selected = false;
            public Point? Moving = null;
            public bool IsMoving = false;
            public int ID = 0;
            private string Extention = "";
            private double MoveThreshold = 10;

            public Tab(string _text = "", string _cont = "", int _id = 0, string _path = "") {
                this.Text = _text;
                this.FilePath = _path;
                this.FileContent = _cont;
                this.ID = _id;
                this.Name = "__tab";
                this.AutoSize = true;
                this.Height = Style.TabHeight;
                this.FileName = _text;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.TextAlign = ContentAlignment.BottomLeft;
                this.MouseEnter += (s, e) => this._hovering = true;
                this.MouseLeave += (s, e) => this._hovering = false;
                this.BackColor = Style.Colors.TabBG;
                this.HoverColor = Style.Colors.Light;

                double GetDistance(Point p1, Point p2) {
                    return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }

                this.MouseDown += (s, e) => this.Moving = e.Location;
                this.MouseMove += (s, e) => {
                    if (this.Moving is Point)
                        if (GetDistance((Point)this.Moving, e.Location) > this.MoveThreshold)
                            this.IsMoving = true;
                };
                this.MouseUp += (s, e) => {
                    this.Moving = null;
                    this.IsMoving = false;
                };
            }

            public void Reload() {
                Console.WriteLine(this.FilePath);
                if (this.FilePath != "")
                    this.FileContent = File.ReadAllText(this.FilePath, this.Encoding);
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                if (this.Extention == "") {
                    this.Extention = this.Text.Substring(this.Text.LastIndexOf(".") + 1);
                    this.Text = this.Text.Substring(0, this.Text.LastIndexOf("."));
                }

                Rectangle r = new Rectangle(Style.MenuPadding, 0, this.Width - Style.MenuPadding * 2, this.Height);

                Font M = new Font(Style.Fonts.UI, Style.MenuTextSize);

                Color t = this.Extention == Ext.Project ? Style.Colors.TextSpecial : Style.Colors.Text;

                e.Graphics.FillRectangle(new SolidBrush(this.Selected ? Style.Colors.BG : this._hovering ? this.HoverColor : this.BackColor), this.ClientRectangle);
                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, r, t, TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
    }
}