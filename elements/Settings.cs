using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace proton {
    public class SettingsWindow : Form {
        public int TitleBarYOffset = 6;
        public int TabsWidth = 100;

        public SettingsWindow() {
            this.Size = new Size(500, 400);
            this.CenterToScreen();

            (new Core.DropShadow()).ApplyShadows(this);

            this.Reload();
        }

        public void Reload(int _selectedtab = 0) {
            this.Controls.Clear(true);

            this.Text = $"{Settings.AppName} {Settings.AppVerString} {Settings.AppVerNum}";
            this.DoubleBuffered = true;
            this.ForeColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = new Icon("resources/icon.ico");

            Point? MovingWindow = null;

            Elements.Titlebar Titlebar = new Elements.Titlebar();
            Titlebar.Dock = DockStyle.Top;
            Titlebar.Top = 0;
            Titlebar.Height = Style.TitlebarSize;
            if (Style.Colors.TitlebarBG is Color)
                Titlebar.BackColor = Style.Colors.TitlebarBG;
            else
                Elements.Titlebar.bc = Style.Colors.TitlebarBG;
            Titlebar.MouseDown += (s, e) => MovingWindow = new Point(e.X, e.Y);
            Titlebar.MouseUp += (s, e) => MovingWindow = null;
            Titlebar.MouseMove += (s, e) => {
                if (MovingWindow is Point) {
                    Point p2 = this.PointToScreen(new Point(e.X, e.Y));
                    this.Location = new Point(p2.X - ((Point)MovingWindow).X, p2.Y - ((Point)MovingWindow).Y - this.TitleBarYOffset);
                }
            };
            Titlebar.AddControlButton("✕", this, (s, e) => this.Close(), Color.Red);
            Titlebar.AddTitle("Settings", this);

            ContextMenuStrip CM = new ContextMenuStrip();

            ToolStripMenuItem qi = new ToolStripMenuItem("Quit", null, (s, e) => this.Close() );
            qi.ShortcutKeys = (Keys.Alt | Keys.F4);
            CM.Items.Add(qi);

            Titlebar.ContextMenuStrip = CM;

            Panel Base = new Panel();
            Base.Dock = DockStyle.Fill;
            Base.BackColor = Style.Colors.BGDark;
            Base.BorderStyle = BorderStyle.None;

            TableLayoutPanel Tabs = new TableLayoutPanel();
            Tabs.Dock = DockStyle.Left | DockStyle.Bottom | DockStyle.Top;
            Tabs.Width = TabsWidth;
            Tabs.Left = 0;
            Tabs.BackColor = Style.Colors.BGDark;

            Panel Con = new Panel();
            Con.Dock = DockStyle.Bottom | DockStyle.Top;
            Con.Left = TabsWidth;
            Con.BackColor = Style.Colors.BG;
            Con.Width = this.Width - TabsWidth;
            Con.Padding = Style.Padding;

            foreach (TableLayoutPanel x in new List<TableLayoutPanel> () {
                new SettingsWind.General(Con),
                new SettingsWind.Appearance(Con),
                new SettingsWind.Cloud(Con),
                new SettingsWind.About(Con)
            }) {
                x.Location = new Point(TabsWidth + Style.PaddingSize, Style.PaddingSize);

                Elements.But t = new Elements.But(x.Name, Tabs.BackColor, null, Style.Colors.Lighter, null, null, new EventHandler((s, e) => {
                    foreach (Elements.But b in Tabs.Controls) {
                        b.Selected = false;
                        b.Invalidate();
                    }
                    foreach (TableLayoutPanel y in Con.Controls)
                        y.Visible = false;
                    x.Visible = true;
                }), true, Con.BackColor);

                Con.Controls.Add(x);
                x.Visible = false;
                t.Margin = new Padding(0);
                t.Width = Tabs.Width;
                Tabs.Controls.Add(t);
            }
            
            ((Elements.But)Tabs.Controls[_selectedtab]).PerformClick();

            Base.Controls.Add(Con);
            Base.Controls.Add(Tabs);
            Base.Controls.Add(Titlebar);

            this.Controls.Add(Base);
        }
    }
}