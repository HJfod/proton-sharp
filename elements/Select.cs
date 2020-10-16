using System;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    namespace Elements {
        public class SelectItem : Button {
            public bool Selected = false;
            private Color HoverColor;
            private Color SelectColor;
            private bool _hovering;

            public SelectItem(string _text, Select _par, int _index) {
                this.BackColor = Style.Colors.Light;
                this.ForeColor = Style.Colors.Text;
                this.HoverColor = Style.Colors.Lighter;
                this.SelectColor = Style.Colors.Main;
                this.Size = new Size(_par.ClientRectangle.Width - Style.PaddingSize * 2, Style.SelectOptionHeight);
                this.Text = _text;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.MouseEnter += (s, e) => this._hovering = true;
                this.MouseLeave += (s, e) => this._hovering = false;
                this.Click += (s, e) => {
                    _par.SelectedIndex = _index;
                    _par.FireSelectedEvent();
                    foreach (SelectItem si in _par.cont.Controls) {
                            si.Selected = false;
                        si.Invalidate();
                    }
                    this.Selected = true;
                };
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                Font M = this.Font is Font ? this.Font : new Font(Style.Fonts.UI, Style.MenuTextSize);

                e.Graphics.FillRectangle(new SolidBrush(this.Selected ? this.SelectColor : this._hovering ? this.HoverColor : this.BackColor), this.ClientRectangle);
                TextRenderer.DrawText(
                    e.Graphics, this.Text, M, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        public class SelectBox : TableLayoutPanel {
            public SelectBox() {
                this.AutoScroll = false;
                this.HorizontalScroll.Enabled = false;
                this.HorizontalScroll.Visible = false;
                this.VerticalScroll.Enabled = false;
                this.VerticalScroll.Visible = false;
                this.DoubleBuffered = true;
            }
        }

        public class Select : Panel {
            public int SelectedIndex = 0;
            public event Action<int> Selected;
            public TableLayoutPanel cont;

            private void SelectEH(int Index) {}

            public Select(Control _par, int _height = -1) {
                _height = _height < 0 ? Style.SelectBoxHeight : _height;

                this.Size = new Size(_par.ClientRectangle.Width, _height);
                this.MaximumSize = new Size(_par.ClientRectangle.Width - Style.PaddingSize * 2, _height);
                this.BackColor = Style.Colors.Light;
                this.ForeColor = Style.Colors.Text;
                this.BorderStyle = BorderStyle.None;
                this.Font = new Font(Style.Fonts.UI, Style.UITextSize);
                this.DoubleBuffered = true;

                this.Selected += new Action<int>(SelectEH);

                this.cont = new SelectBox();
                this.cont.Size = this.Size.Shrink(Style.PaddingSize * 2);
                this.cont.BackColor = this.BackColor;
                this.cont.Location = new Point(Style.PaddingSize, Style.PaddingSize);

                this.Controls.Add(cont);

                VScrollBar v = new VScrollBar();
                v.Dock = DockStyle.Right;
                v.Scroll += (s, e) => this.cont.VerticalScroll.Value = v.Value;

                this.Controls.Add(v);
                v.BringToFront();

                MouseEventHandler scroll = new MouseEventHandler((s, e) => {
                    if (v.Value - e.Delta / Settings.S.ScrollWheelDecelerator <= 1)
                        v.Value = 1;
                    else if (v.Value - e.Delta / Settings.S.ScrollWheelDecelerator >= v.Maximum - 1)
                        v.Value = v.Maximum - 1;
                    else
                        v.Value -= (int)((long)e.Delta / (long)Settings.S.ScrollWheelDecelerator);
                    if (this.cont.VerticalScroll.Value != v.Value)
                        this.cont.VerticalScroll.Value = v.Value;
                });

                this.cont.MouseWheel += scroll;
                this.MouseWheel += scroll;
            }

            public void FireSelectedEvent() {
                this.Selected(this.SelectedIndex);
            }

            public bool AddItem(string _text) {
                this.cont.Controls.Add(new SelectItem(_text, this, this.cont.Controls.Count));

                return true;
            }
        }
    }
}