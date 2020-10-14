using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;

namespace proton {
    public static class Methods {
        public static void AddContextMenu(this Control _c, Control _par, dynamic[] _menu) {
            _c.MouseUp += (s, e) => {
                if (e.Button == MouseButtons.Right) {
                    var m = new MenuWindow(_par, _menu);
                    _par.Controls.Add(m);
                    m.BringToFront();
                }
            };
        }
    }

    public static class Ext {
        public static string Project = "ptd";           // (proton document)
        public static string Userdata = "ptu";          // (proton user)
        public static string Data = "pta";              // (proton app)
        public static string DefaultFile = $"Unnamed.{Project}";
        public static string Filter = $"Proton documents (*.ptd)|*.ptd|Accepted types (*.ptd;*.txt)|*.txt;*.ptd|All files (*.*)|*.*";
    }

    public partial class Main : Form {
        public int FileSystemSize = 200;
        public (int, int) FileSystemSizeLimits = (100, 300);
        public bool FileSystemVisible = false;
        public int TitleBarYOffset = 6;

        public dynamic[] TopMenu;
        public List<dynamic> ShortCuts = new List<dynamic>{};
        public Elements.Textarea Editor;

        private void MaxNomWindow(Form OG) {
            OG.WindowState = OG.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        public void NewFile(object s, EventArgs e) {
            AddTab(Ext.DefaultFile);
        }

        public void SaveFile(object s, EventArgs e, bool _SaveAs = false) {

        }

        public void OpenFile(object s, EventArgs e) {
            using (OpenFileDialog fd = new OpenFileDialog()) {
                fd.InitialDirectory = "c:\\";
                fd.Filter = Ext.Filter;
                fd.FilterIndex = 0;
                fd.Multiselect = true;
                fd.RestoreDirectory = true;
                fd.DefaultExt = Ext.Project;

                if (fd.ShowDialog() == DialogResult.OK)
                    foreach (string file in fd.FileNames)
                        this.AddTab(Path.GetFileName(file), File.ReadAllText(file));
            }
        }

        public Main() {
            this.Size = Settings.DefaultSize;
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
            Titlebar.BackColor = Style.Colors.TitlebarBG;
            Titlebar.DoubleClick += (s, e) => MaxNomWindow(this);
            Titlebar.MouseDown += (s, e) => MovingWindow = new Point(e.X, e.Y);
            Titlebar.MouseUp += (s, e) => MovingWindow = null;
            Titlebar.MouseMove += (s, e) => {
                if (MovingWindow is Point) {
                    Point p2 = this.PointToScreen(new Point(e.X, e.Y));
                    this.Location = new Point(p2.X - ((Point)MovingWindow).X, p2.Y - ((Point)MovingWindow).Y - this.TitleBarYOffset);
                }
            };
            Titlebar.AddControlButton("─", (s, e) => this.WindowState = FormWindowState.Minimized);
            Titlebar.AddControlButton("☐", (s, e) => MaxNomWindow(this));
            Titlebar.AddControlButton("✕", (s, e) => this.Close());

            ContextMenuStrip CM = new ContextMenuStrip();
            CM.Items.Add(new ToolStripMenuItem("Minimize", null, (s, e) => this.WindowState = FormWindowState.Minimized ));
            CM.Items.Add(new ToolStripMenuItem("Maximize", null, (s, e) => MaxNomWindow(this) ));

            CM.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem qi = new ToolStripMenuItem("Quit", null, (s, e) => this.Close() );
            qi.ShortcutKeys = (Keys.Alt | Keys.F4);
            CM.Items.Add(qi);

            Titlebar.ContextMenuStrip = CM;

            Panel Bottom = new Panel();
            Bottom.Dock = DockStyle.Bottom;
            Bottom.Height = Style.FooterSize;
            Bottom.BackColor = Style.Colors.FooterBG;

            Panel Base = new Panel();
            Base.Dock = DockStyle.Fill;
            Base.BackColor = Style.Colors.BGDark;
            Base.BorderStyle = BorderStyle.None;

            Panel FileSystem = new Panel();
            FileSystem.Width = FileSystemSize;
            FileSystem.Dock = DockStyle.Left | DockStyle.Bottom | DockStyle.Top;
            FileSystem.BackColor = Style.Colors.BGDark;

            Panel FileSystemControls = new Panel();
            FileSystemControls.BackColor = Style.Colors.FooterBG;
            FileSystemControls.Height = Style.TabHeight;
            FileSystemControls.Dock = DockStyle.Top;

            TreeView FileSystemView = new TreeView();
            FileSystemView.Width = FileSystemSize;
            FileSystemView.Dock = DockStyle.Fill;
            FileSystemView.BackColor = Style.Colors.BGDark;
            FileSystemView.BorderStyle = BorderStyle.None;

            FileSystem.Controls.Add(FileSystemView);
            FileSystem.Controls.Add(FileSystemControls);
            
            bool ResizingFileSystem = false;

            Panel Dragger = new Panel();
            Dragger.BackColor = Style.Colors.BGDark;
            Dragger.Dock = DockStyle.Bottom | DockStyle.Top | DockStyle.Left;
            Dragger.Left = FileSystemSize;
            Dragger.Width = Style.DraggerWidth;
            Dragger.MouseEnter += (object s, EventArgs e) => Dragger.Cursor = FileSystem.Visible ? Cursors.SizeWE : Cursors.PanEast;
            Dragger.MouseDown += (object s, MouseEventArgs e) => ResizingFileSystem = true;
            Dragger.MouseUp += (object s, MouseEventArgs e) => ResizingFileSystem = false;
            Dragger.MouseMove += (object s, MouseEventArgs e) => {
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
            Dragger.DoubleClick += (object s, EventArgs e) => {
                FileSystem.Visible = FileSystem.Visible ? false : true;
                Dragger.Visible = FileSystem.Visible;
            };

            Panel EditorContainer = new Panel();
            EditorContainer.Left = FileSystemSize + Style.DraggerWidth;
            EditorContainer.Dock = DockStyle.Fill;

            FlowLayoutPanel TabContainer = new FlowLayoutPanel();
            TabContainer.Dock = DockStyle.Top;
            TabContainer.Height = Style.TabHeight;
            TabContainer.BackColor = Style.Colors.TabBG;
            TabContainer.Name = "__tabs";

            Panel EditorPadding = new Panel();
            EditorPadding.Dock = DockStyle.Fill;
            EditorPadding.BackColor = Style.Colors.BG;
            EditorPadding.Padding = Style.Padding;

            Editor = new Elements.Textarea();
            Editor.Left = FileSystemSize + Style.DraggerWidth;
            Editor.Dock = DockStyle.Fill;
            Editor.BackColor = Style.Colors.BG;
            Editor.Font = new Font("Segoe UI Light", Style.EditorTextSize);
            Editor.ForeColor = Style.Colors.Text;

            Editor.AddContextMenu(this, new dynamic[] {
                new {
                    Name = "Copy#Ctrl + C",
                    Click = new EventHandler((s, e) => Editor.Copy())
                },
                new {
                    Name = "Paste#Ctrl + V",
                    Click = new EventHandler((s, e) => Editor.Paste())
                },
                new {
                    Name = "Cut#Ctrl + X",
                    Click = new EventHandler((s, e) => Editor.Cut())
                },
                new { Type = "Separator" },
                new {
                    Name = "Style",
                    Menu = new dynamic[] {
                        new {
                            Name = "Bold#Ctrl + B",
                            Click = new EventHandler((s, e) => {})
                        },
                        new {
                            Name = "Italics#Ctrl + I",
                            Click = new EventHandler((s, e) => {})
                        },
                        new {
                            Name = "Underline#Ctrl + U",
                            Click = new EventHandler((s, e) => {})
                        },
                        new {
                            Name = "Color",
                            Menu = new dynamic[] {}
                        }
                    }
                },
                new {
                    Name = "Insert",
                    Menu = new dynamic[] {
                        new {
                            Name = "Symbol",
                            Menu = new dynamic[] {}
                        },
                        new {
                            Name = "Image",
                            Click = new EventHandler((s, e) => {})
                        }
                    }
                },
                new { Type = "Separator" },
                new {
                    Name = "Search",
                    Click = new EventHandler((s, e) => {})
                },
                new { Type = "Separator" },
                new {
                    Name = "Settings",
                    Menu = new dynamic[] {
                        new {
                            Name = "yeah",
                            Click = new EventHandler((s, e) => {})
                        },
                        new {
                            Name = "Another sub",
                            Menu = new dynamic[] {
                                new {
                                    Name = "third",
                                    Click = new EventHandler((s, e) => {})
                                }
                            }
                        }
                    }
                }
            });

            FileSystemView.AddContextMenu(this, new dynamic[] {
                new {
                    Name = "Hide filesystem#Ctrl + M",
                    Click = new EventHandler((s, e) => {
                        FileSystem.Visible = FileSystem.Visible ? false : true;
                        Dragger.Visible = FileSystem.Visible;
                    })
                }
            });

            TabContainer.AddContextMenu(this, new dynamic[] {
                new {
                    Name = "New Document#Ctrl + N",
                    Click = new EventHandler(NewFile)
                }
            });

            EditorPadding.Controls.Add(Editor);

            EditorContainer.Controls.Add(EditorPadding);
            EditorContainer.Controls.Add(TabContainer);

            this.TopMenu = new dynamic[] {
                new {
                    Name = "File",
                    Menu = new dynamic[] {
                        new {
                            Name = "New File#Ctrl + N",
                            Click = new EventHandler(this.NewFile),
                            Accelerator = (Keys.Control | Keys.N)
                        },
                        new {
                            Name = "Open File#Ctrl + O",
                            Click = new EventHandler(this.OpenFile),
                            Accelerator = (Keys.Control | Keys.O)
                        },
                        new {
                            Name = "Save File#Ctrl + S",
                            Click = new EventHandler((s, e) => this.SaveFile(s, e))
                        },
                        new {
                            Name = "Save as#Ctrl + Shift + S",
                            Click = new EventHandler((s, e) => this.SaveFile(s, e, true))
                        },
                        new { Type = "Separator" },
                        new {
                            Name = "New Window",
                            Click = new EventHandler((s, e) => {

                            })
                        },
                        new { Type = "Separator" },
                        new {
                            Name = "Quit#Alt + F4",
                            Accelerator = (Keys.Alt | Keys.F4),
                            Click = new EventHandler((s, e) => { this.Close(); })
                        }
                    }
                },
                new {
                    Name = "Edit",
                    Menu = new dynamic[] {
                        new {
                            Name = "Settings",
                            Click = new EventHandler((s, e) => {})
                        }
                    }
                },
                new {
                    Name = "View",
                    Menu = new dynamic[] {
                        new {
                            Name = "Toggle File System#Ctrl + M",
                            Accelerator = (Keys.Control | Keys.M),
                            Click = new EventHandler((s, e) => {
                                FileSystem.Visible = FileSystem.Visible ? false : true;
                                Dragger.Visible = FileSystem.Visible;
                            })
                        }
                    }
                }
            };

            Array.Reverse(this.TopMenu);

            foreach (dynamic Menu in TopMenu) {
                Button M = Titlebar.AddMenuButton(Menu.Name);

                foreach (dynamic Sub in Menu.Menu) {
                    try {
                        var T = Sub.Type;
                    } catch (RuntimeBinderException) {
                        try {
                            ShortCuts.Add(new { A = Sub.Accelerator, C = Sub.Click });
                        } catch (Exception) {}
                    }
                }

                // M.Click += (s, e) => M_CM.Show(new Point(this.Left + M.Left, this.Top + Style.TitlebarSize));
                M.Click += (s, e) => {
                    var m = new MenuWindow(this, Menu.Menu, true, new Point(M.Left, Style.TitlebarSize));
                    m.BringToFront();
                };
            }

            Base.Controls.Add(EditorContainer);
            Base.Controls.Add(Dragger);
            Base.Controls.Add(FileSystem);
            Base.Controls.Add(Titlebar);
            Base.Controls.Add(Bottom);
/*
            Base.Paint += (s, e) => {
                Style.DrawShadow(e.Graphics, Color.Black, this.BackColor, new Rectangle(100, 100, 200, 200), 20);
            };
*/
            this.Controls.Add(Base);

            this.MenuCloseControlAdd(this, this);

            AddTab(Ext.DefaultFile);

            this.CenterToScreen();
        }

        public void SelectTab(int _id) {
            foreach (Elements.Tab t in this.Controls.Find("__tab", true)) {
                if (t.Selected)
                    t.FileContent = Editor.Text;
                t.Selected = t.ID == _id ? true : false;
                t.Invalidate();
            }
            Editor.Text = ((Elements.Tab)this.Controls.Find("__tab", true)[_id]).FileContent;
        }

        private bool CheckTabIDAvailability(int _id) {
            bool f = false;
            foreach (Elements.Tab t in this.Controls.Find("__tab", true))
                if (t.ID == _id) f = true;
            return f;
        }

        public void AddTab(string _name = "", string _content = "") {
            int id = this.Controls.Find("__tab", true).Length;

            while (CheckTabIDAvailability(id)) id++;

            Elements.Tab Tab = new Elements.Tab(_name, _content, id);

            Tab.Click += (s, e) => SelectTab(id);

            Tab.AddContextMenu(this, new dynamic[] {
                new {
                    Name = "Close#Ctrl + W",
                    Click = new EventHandler((s, e) => {
                        Tab.Dispose();
                        SelectTab(this.Controls.Find("__tab", true).Length - 1);
                    })
                }
            });

            this.Controls.Find("__tabs", true)[0].Controls.Add(Tab);

            SelectTab(id);
        }

        private void MenuCloseControlAdd(Control c, Form Base) {
            foreach (Control cc in c.Controls)
                this.MenuCloseControlAdd(cc, Base);
            
            c.MouseDown += (s, e) => CloseAllMenus(Base);
        }

        public static void CloseAllMenus(Control Base, int _lvl = 0) {
            foreach (MenuWindow mw in Base.Controls.Find("__MenuWindow", true))
                if (mw.Level >= _lvl)
                    mw.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            foreach (dynamic k in ShortCuts) {
                if (keyData == k.A) {
                    EventHandler temp = k.C;
                    if (temp != null) {
                        temp(this, new EventArgs());
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x40000;
                return cp;
            }
        }
    }
}