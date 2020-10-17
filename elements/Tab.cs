using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace proton {
    namespace Elements {
        public class Tab : Button {
            private bool _hovering = false;
            private Color HoverColor;
            public string FileName = "";
            public string FileContent = "";
            public Encoding Encoding = Encoding.UTF8;
            public bool Selected = false;
            public int ID = 0;
            private string Extention = "";

            public Tab(string _text = "", string _cont = "", int _id = 0) {
                this.Text = _text;
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