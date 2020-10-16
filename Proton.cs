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
using System.Text.RegularExpressions;
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
        public static void Clear(this Control.ControlCollection controls, bool dispose) {
            for (int ix = controls.Count - 1; ix >= 0; --ix)
                if (dispose) controls[ix].Dispose(); else controls.RemoveAt(ix);
        }
    }

    public static class Ext {
        public static string Project = "ptd";           // (proton document)
        public static string Userdata = "ptu";          // (proton user)
        public static string Data = "pta";              // (proton app)
        public static string Theme = "ptt";             // (proton theme)
        public static string DefaultFile = $"Unnamed.{Project}";
        public static string Filter = $"Proton documents (*.ptd)|*.ptd|Accepted types (*.ptd;*.txt)|*.txt;*.ptd|All files (*.*)|*.*";
        public static string UserdataFilename = $"user/user.{Userdata}";
        public static Encoding Enc = Encoding.UTF8;
        public static string UFold = "user";
    }

    public static class Dat {
        public static string[] _User;
        public static void SaveToUserData(string _key, string _val) {
            if (!File.Exists(Ext.UserdataFilename)) File.WriteAllText(Ext.UserdataFilename, "", Ext.Enc);
            string newData = "";
            foreach (string f in File.ReadAllLines(Ext.UserdataFilename, Ext.Enc))
                newData += f.Trim().StartsWith(_key) ? "" : $"{f}\n";
            newData += $"{_key}: {_val}\n";
            File.WriteAllText(Ext.UserdataFilename, newData, Ext.Enc);
        }

        public static void LoadUserData() {
            if (File.Exists(Ext.UserdataFilename))
                _User = File.ReadAllLines(Ext.UserdataFilename, Ext.Enc);
        }

        public static string GetUserDataKey(string _key) {
            if (_User != null)
                foreach (string f in _User)
                    if (f.Trim().StartsWith(_key)) return f.Substring(f.IndexOf(":") + 1).Trim();
            return "";
        }

        public static string[] LoadSymbols() {
            return File.ReadAllText($"{Ext.UFold}/symbols.{Ext.Data}", Ext.Enc).Split(" ");
        }
    }

    public class SEventArgs : EventArgs {
        public SEventArgs(string message) {
            Message = message;
        }

        public string Message { get; set; }
    }

    public delegate void SHandler (object Sender, SEventArgs e);
    
    public partial class Main : Form {
        public int FileSystemSize = 200;
        public (int, int) FileSystemSizeLimits = (100, 300);
        public bool FileSystemVisible = false;
        public int TitleBarYOffset = 6;

        public dynamic[] TopMenu;
        public List<dynamic> ShortCuts = new List<dynamic>{};
        public Elements.Textarea Editor;
        public static dynamic[] Themes;

        public static SettingsWindow SettingsWindow;

        public Elements.SearchBox SearchBox;

        private void LoadThemes() {
            List<dynamic> _t = new List<dynamic> ();
            foreach (string f in Directory.GetFiles("resources/themes"))
                if (f.EndsWith(Ext.Theme))
                    _t.Add(new {
                        Name = File.ReadAllLines(f)[0].Split("=")[1],
                        NoClose = true,
                        Click = new EventHandler((s, e) => {
                            IDictionary<string, Object> _style = new ExpandoObject() as IDictionary<string, Object>;
                            foreach (string t in File.ReadAllLines(Path.GetFullPath(f)))
                                if (!t.Contains("="))
                                    _style.Add(t.Substring(0, t.IndexOf(" ")), $"#{Regex.Replace(t, @"\s+", " ").Substring(t.IndexOf(" ") + 1)}");
                            Style.LoadStyle(_style, this);
                            Dat.SaveToUserData("theme", File.ReadAllLines(f)[0].Split("=")[1]);
                        })
                    });
            Themes = _t.ToArray<dynamic>();
        }

        public void LoadTheme(string _name) {
            foreach (dynamic t in Themes) {
                if (t.Name == _name)
                    t.Click(this, new EventArgs());
            }
        }

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
            FullReload();
        }

        public void SaveReload() {
            List<dynamic> _saved = new List<dynamic> ();
            foreach (Elements.Tab tab in this.Controls.Find("__tab", true))
                _saved.Add(new { Name = tab.FileName, Content = tab.FileContent, Selected = tab.Selected });
            Reload(_saved);
        }

        public void FullReload() {
            this.LoadThemes();
            Dat.LoadUserData();
            this.LoadTheme(Dat.GetUserDataKey("theme"));
            Settings.S.CloseMenuOnDeFocus = Dat.GetUserDataKey("close-menus") == "True";

            Reload();
        }

        public void Reload(List<dynamic> _savedtabs = null) {
            this.Controls.Clear(true);

            string[] Symbols = Dat.LoadSymbols();

            this.Size = Settings.DefaultSize;
            this.Text = $"{Settings.AppName} {Settings.AppVerString} {Settings.AppVerNum}";
            this.DoubleBuffered = true;
            this.ForeColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = new Icon("resources/icon.ico");

            Point? MovingWindow = null;

            Elements.Titlebar Titlebar = new Elements.Titlebar();
            if (Style.Colors.TitlebarBG is Color)
                Titlebar.BackColor = Style.Colors.TitlebarBG;
            else
                Elements.Titlebar.bc = Style.Colors.TitlebarBG;
            Titlebar.Dock = DockStyle.Top;
            Titlebar.Top = 0;
            Titlebar.Height = Style.TitlebarSize;
            Titlebar.DoubleClick += (s, e) => MaxNomWindow(this);
            Titlebar.MouseDown += (s, e) => MovingWindow = new Point(e.X, e.Y);
            Titlebar.MouseUp += (s, e) => MovingWindow = null;
            Titlebar.MouseMove += (s, e) => {
                if (MovingWindow is Point) {
                    Point p2 = this.PointToScreen(new Point(e.X, e.Y));
                    this.Location = new Point(p2.X - ((Point)MovingWindow).X, p2.Y - ((Point)MovingWindow).Y - this.TitleBarYOffset);
                }
            };
            Titlebar.AddControlButton("─", this, (s, e) => this.WindowState = FormWindowState.Minimized);
            Titlebar.AddControlButton("☐", this, (s, e) => MaxNomWindow(this));
            Titlebar.AddControlButton("✕", this, (s, e) => this.Close(), Color.Red);

            ContextMenuStrip CM = new ContextMenuStrip();
            CM.Items.Add(new ToolStripMenuItem("Minimize", null, (s, e) => this.WindowState = FormWindowState.Minimized ));
            CM.Items.Add(new ToolStripMenuItem("Maximize", null, (s, e) => MaxNomWindow(this) ));

            CM.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem qi = new ToolStripMenuItem("Quit", null, (s, e) => this.Close() );
            qi.ShortcutKeys = (Keys.Alt | Keys.F4);
            CM.Items.Add(qi);

            Titlebar.ContextMenuStrip = CM;

            Elements.Footer Bottom = new Elements.Footer();
            Bottom.Dock = DockStyle.Bottom;

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
            Editor.Font = new Font(Style.EditorFont, Style.EditorTextSize);
            Editor.ForeColor = Style.Colors.Text;
            Editor.TextChanged += (s, e) => {
                Bottom.UpdateWordCount(
                    Regex.IsMatch(Editor.Text, @"^\s*$") ? 0 : Regex.Replace(Editor.Text, @"\s+", " ").Trim().Split(" ").Length,
                    Editor.Text.Length,
                    Regex.Replace(Editor.Text, @"\s+", "").Length
                );
            };

            dynamic[] EditMenu = new dynamic[] {
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
                            Accelerator = (Keys.Control | Keys.B),
                            Click = new EventHandler((s, e) => ApplyStyle(FontStyle.Bold))
                        },
                        new {
                            Name = "Italics#Ctrl + I",
                            Click = new EventHandler((s, e) => ApplyStyle(FontStyle.Italic))
                        },
                        new {
                            Name = "Underline#Ctrl + U",
                            Click = new EventHandler((s, e) => ApplyStyle(FontStyle.Underline))
                        },
                        new { Type = "Separator" },
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
                            Menu = new dynamic[] {
                                new {
                                    Type = "Table",
                                    Width = 8,
                                    Contents = Symbols,
                                    Click = new SHandler((s, e) => Editor.SelectedText += e.Message)
                                }
                            }
                        }
                    }
                },
                new { Type = "Separator" },
                new {
                    Name = "Search#Ctrl + F",
                    Click = new EventHandler((s, e) => SearchBox.Visible = true)
                },
                new { Type = "Separator" },
                new {
                    Name = "Settings",
                    Menu = new dynamic[] {
                        new {
                            Name = "Theme",
                            Menu = Themes
                        },/*
                        new {
                            Type = "Checkbox",
                            Text = "Close menu when unfocused",
                            GetVar = new Func<bool>(() => Settings.S.CloseMenuOnDeFocus),
                            SetVar = new Func<bool, bool>(_val => Settings.S.CloseMenuOnDeFocus = _val)
                        },  //*/
                        new { Type = "Separator" },
                        new {
                            Name = "Settings#F1",
                            Click = new EventHandler((s, e) => OpenSettings()),
                            Accelerator = (Keys.F1),
                            NoClose = true
                        }
                    }
                }
            };

            Editor.AddContextMenu(this, EditMenu);

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

            void ApplyStyle(FontStyle f) {
                Editor.SelectionFont = ((Editor.SelectionFont.Style & f) == 0) ?
                    new Font(Style.EditorFont, Style.EditorTextSize, f) :
                    new Font(Style.EditorFont, Style.EditorTextSize);
            };

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
                    Menu = EditMenu
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

            void CheckShortCuts (dynamic[] Menu) {
                foreach (dynamic Sub in Menu)
                    try {
                        CheckShortCuts(Sub.Menu);
                    } catch (Exception) {
                        try {
                            ShortCuts.Add(new { A = Sub.Accelerator, C = Sub.Click });
                        } catch (Exception) {}
                    }
            }

            foreach (dynamic Menu in TopMenu) {
                Button M = Titlebar.AddMenuButton(Menu.Name, this);

                CheckShortCuts(Menu.Menu);

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

            SearchBox = new Elements.SearchBox(this, (s, e) => {
                string i = SearchBox.GetInput().ToLower().Trim();
                if (Editor.Text.ToLower().Contains(i))
                    Editor.Select(Editor.Text.ToLower().IndexOf(i), i.Length);
            }, new Point(this.ClientRectangle.Width - Style.SearchBoxWidth - Style.PaddingSize * 2, Style.PaddingSize * 2 + Style.TitlebarSize + Style.TabHeight));
            SearchBox.Visible = false;

            this.Controls.Add(SearchBox);

            this.MenuCloseControlAdd(this, this);

            this.Deactivate += (s, e) => { if (Settings.S.CloseMenuOnDeFocus) CloseAllMenus(this); };

            this.FormClosed += (s, e) => {
                Dat.SaveToUserData("close-menus", Settings.S.CloseMenuOnDeFocus.ToString());
            };

            SearchBox.BringToFront();

            if (_savedtabs != null) {
                int sel = -1;
                int i = 0;
                foreach (dynamic tab in _savedtabs) {
                    Elements.Tab t = AddTab(tab.Name, tab.Content);
                    i++;
                    if (tab.Selected) sel = t.ID;
                }
                SelectTab(sel);
            } else {
                AddTab(Ext.DefaultFile);

                Bottom.UpdateWordCount(0, 0, 0);

                this.CenterToScreen();
            }
        }

        public void OpenSettings() {
            CloseAllMenus(this);
            if (SettingsWindow is SettingsWindow) return;
            SettingsWindow = new SettingsWindow();
            SettingsWindow.Show();
            SettingsWindow.BringToFront();
            SettingsWindow.FormClosed += (s, e) => SettingsWindow = null;
        }

        public void SelectTab(int _id) {
            Elements.Tab n = null;
            Elements.Tab o = null;
            foreach (Elements.Tab t in this.Controls.Find("__tab", true)) {
                if (_id == -1) n = t;
                if (t.Selected) o = t;
                if (t.ID == _id) n = t;
                t.Selected = false;
                t.Invalidate();
            }
            if (n == null) {
                if (this.Controls.Find("__tab", true).Length == 0)
                    AddTab(Ext.DefaultFile);
                SelectTab(-1);
                return;
            }
            if (o != null) o.FileContent = Editor.Text;
            Editor.Text = n.FileContent;
            n.Selected = true;
            n.Invalidate();
        }

        private bool CheckTabIDAvailability(int _id) {
            if (_id > 1000) return false;
            bool f = false;
            foreach (Elements.Tab t in this.Controls.Find("__tab", true))
                if (t.ID == _id) f = true;
            return f;
        }

        public Elements.Tab AddTab(string _name = "", string _content = "") {
            int id = 0;
            while (this.CheckTabIDAvailability(id)) id++;

            Elements.Tab Tab = new Elements.Tab(_name, _content, id);

            Tab.Click += (s, e) => SelectTab(id);

            Tab.AddContextMenu(this, new dynamic[] {
                new {
                    Name = "Close#Ctrl + W",
                    Click = new EventHandler((s, e) => {
                        Tab.Dispose();
                        this.SelectTab(id > 0 ? id - 1 : 0);
                    })
                }
            });

            this.Controls.Find("__tabs", true)[0].Controls.Add(Tab);

            this.SelectTab(id);

            return Tab;
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
            foreach (dynamic k in ShortCuts)
                if (keyData == k.A) {
                    EventHandler temp = k.C;
                    if (temp != null)
                        temp(this, new EventArgs());
                    return true;
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