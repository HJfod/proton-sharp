using System;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;

namespace proton {
    namespace SettingsWind {
        public class General : TableLayoutPanel {
            public General(Control _par) {
                this.Name = "General";
                this.AutoSize = true;
                this.Size = _par.Size.Shrink(Style.PaddingSize);
                this.Dock = DockStyle.Fill;

                this.Controls.Add(new Elements.CheckBox(new Tuple<Func<bool>, Func<bool, bool>> (
                    new Func<bool>(() => { return Settings.S.CloseMenuOnDeFocus; }),
                    new Func<bool, bool>(_val => Settings.S.CloseMenuOnDeFocus = _val)
                ), "Close context menu(s) on Window defocus"));

                Elements.Select enc = new Elements.Select(this);
                foreach (KeyValuePair<string, Encoding> e in Main.Encodings)
                    enc.AddItem(e.Key);
                enc.Selected += ix => {
                    Settings.S.DefaultEncoding = Main.Encodings.ElementAt(ix).Value;
                };

                this.Controls.Add(enc);
            }
        }
        public class Appearance : TableLayoutPanel {
            public Appearance(Control _par) {
                this.Name = "Apperance";
                this.AutoSize = true;
                this.Size = _par.Size.Shrink(Style.PaddingSize);
                this.Dock = DockStyle.Fill;

                Elements.Select themesel = new Elements.Select(this);
                foreach (dynamic theme in Main.Themes)
                    themesel.AddItem(theme.Name);
                themesel.Selected += ix => {
                    Main.Themes[ix].Click(this, new EventArgs());
                    Main.SettingsWindow.Reload(1);
                };

                this.Controls.Add(new Elements.Text("Theme"));
                this.Controls.Add(themesel);
            }
        }
        public class Cloud : TableLayoutPanel {
            public Cloud(Control _par) {
                this.Name = "Sync";
                this.AutoSize = true;
                this.Size = _par.Size.Shrink(Style.PaddingSize);
                this.Dock = DockStyle.Fill;

                this.Controls.Add(new Elements.But("sick af"));
            }
        }
        public class About : TableLayoutPanel {
            public About(Control _par) {
                this.Name = "About";
                this.AutoSize = true;
                this.Size = _par.Size.Shrink(Style.PaddingSize);
                this.Dock = DockStyle.Fill;

                this.Controls.Add(new Elements.Text($"{Settings.AppName}", true, 20));
                this.Controls.Add(new Elements.Text($"Version {Settings.AppVerString} ({Settings.AppVerNum})", false, -1, true));
                this.Controls.Add(new Elements.NewLine());
                this.Controls.Add(new Elements.Text("Developed by HJfod"));
            }
        }
    }
}