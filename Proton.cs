using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proton {
    public class WindowResizer : Panel {
        public WindowResizer(string direction = "NW") {

        }
    }
    public partial class Main : Form {
        public int FileSystemSize = 200;
        public (int, int) FileSystemSizeLimits = (100, 300);
        public bool FileSystemVisible = false;

        private void MaxNomWindow(Form OG) {
            OG.WindowState = OG.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        public Main() {
            this.Size = Settings.DefaultSize;
            this.Text = $"{Settings.AppName} {Settings.AppVerString} {Settings.AppVerNum}";
            this.DoubleBuffered = true;
            this.ForeColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;

            Point? MovingWindow = null;

            Elements.Titlebar Titlebar = new Elements.Titlebar();
            Titlebar.Dock = DockStyle.Top;
            Titlebar.Top = 0;
            Titlebar.Height = Style.TitlebarSize;
            Titlebar.BackColor = Style.Colors.TitlebarBG;
            Titlebar.DoubleClick += (object s, EventArgs e) => MaxNomWindow(this);
            Titlebar.MouseDown += (object s, MouseEventArgs e) => MovingWindow = new Point(e.X, e.Y);
            Titlebar.MouseUp += (object s, MouseEventArgs e) => MovingWindow = null;
            Titlebar.MouseMove += (object s, MouseEventArgs e) => {
                if (MovingWindow is Point) {
                    Point p2 = this.PointToScreen(new Point(e.X, e.Y));
                    this.Location = new Point(p2.X - ((Point)MovingWindow).X, p2.Y - ((Point)MovingWindow).Y);
                }
            };
            Titlebar.AddControlButton("─", (object s, EventArgs e) => this.WindowState = FormWindowState.Minimized);
            Titlebar.AddControlButton("☐", (object s, EventArgs e) => MaxNomWindow(this));
            Titlebar.AddControlButton("✕", (object s, EventArgs e) => this.Close());

            ContextMenuStrip CM = new ContextMenuStrip();
            CM.Items.Add(new ToolStripMenuItem("Minimize", null, (object s, EventArgs e) => this.WindowState = FormWindowState.Minimized ));
            CM.Items.Add(new ToolStripMenuItem("Maximize", null, (object s, EventArgs e) => MaxNomWindow(this) ));

            CM.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem qi = new ToolStripMenuItem("Quit", null, (object s, EventArgs e) => this.Close() );
            qi.ShortcutKeys = (Keys.Alt | Keys.F4);
            CM.Items.Add(qi);

            Titlebar.ContextMenuStrip = CM;

            Panel Bottom = new Panel();
            Bottom.Dock = DockStyle.Bottom;
            Bottom.Height = Style.FooterSize;
            Bottom.BackColor = Style.Colors.FooterBG;
            Bottom.Cursor = Cursors.SizeNS;

            Panel Base = new Panel();
            Base.Dock = DockStyle.Fill;
            Base.BackColor = Style.Colors.BGDark;
            Base.BorderStyle = BorderStyle.None;

            TreeView FileSystem = new TreeView();
            FileSystem.Width = FileSystemSize;
            FileSystem.Dock = DockStyle.Left | DockStyle.Bottom | DockStyle.Top;
            FileSystem.BackColor = Style.Colors.BGDark;
            FileSystem.BorderStyle = BorderStyle.None;
            
            bool ResizingFileSystem = false;

            Panel Dragger = new Panel();
            Dragger.BackColor = Style.Colors.BGDark;
            Dragger.Dock = DockStyle.Bottom | DockStyle.Top | DockStyle.Left;
            Dragger.Left = FileSystemSize;
            Dragger.Width = Style.DraggerWidth;
            Dragger.Cursor = Cursors.SizeWE;
            Dragger.MouseDown += (object sender, MouseEventArgs e) => ResizingFileSystem = true;
            Dragger.MouseUp += (object sender, MouseEventArgs e) => ResizingFileSystem = false;
            Dragger.MouseMove += (object sender, MouseEventArgs e) => {
                if (ResizingFileSystem) {
                    int S = FileSystem.Width;
                    S = S + e.X;
                    if (S < FileSystemSizeLimits.Item1)
                        S = FileSystemSizeLimits.Item1;
                    else if (S > FileSystemSizeLimits.Item2)
                        S = FileSystemSizeLimits.Item2;
                    FileSystem.Width = S;
                }
            };
            Dragger.DoubleClick += (object sender, EventArgs e) => {
                FileSystem.Visible = FileSystem.Visible ? false : true;
            };

            Panel SideDragger = new Panel();

            Panel EditorContainer = new Panel();
            EditorContainer.Left = FileSystemSize + Style.DraggerWidth;
            EditorContainer.Dock = DockStyle.Fill;
            EditorContainer.BackColor = Style.Colors.BG;

            Label Q = new Label();
            Q.Text = "Hello bitch ass";
            Q.ForeColor = Style.Colors.Text;

            EditorContainer.Controls.Add(Q);

            Base.Controls.Add(EditorContainer);
            Base.Controls.Add(Dragger);
            Base.Controls.Add(FileSystem);
            Base.Controls.Add(Titlebar);
            Base.Controls.Add(Bottom);

            this.Controls.Add(Base);

            this.CenterToScreen();
        }
    }
}