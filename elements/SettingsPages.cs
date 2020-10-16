using System;
using System.Windows.Forms;

namespace proton {
    namespace SettingsWind {
        public class General : TableLayoutPanel {
            public General() {
                this.Name = "General";
                this.AutoSize = true;

                this.Controls.Add(new Elements.CheckBox(new Tuple<Func<bool>, Func<bool, bool>> (
                    new Func<bool>(() => { return Settings.S.CloseMenuOnDeFocus; }),
                    new Func<bool, bool>(_val => Settings.S.CloseMenuOnDeFocus = _val)
                ), "Close context menu(s) on Window defocus"));
            }
        }
        public class Appearance : TableLayoutPanel {
            public Appearance() {
                this.Name = "Apperance";
                this.AutoSize = true;

                ListBox themesel = new ListBox();
                foreach (dynamic theme in Main.Themes)
                    themesel.Items.Add(theme.Name);
                themesel.SelectedIndexChanged += (s, e) => {
                    Main.Themes[themesel.SelectedIndex].Click(this, new EventArgs());
                    Main.SettingsWindow.Reload(1);
                };
                this.Controls.Add(themesel);
            }
        }
        public class Cloud : TableLayoutPanel {
            public Cloud() {
                this.Name = "Sync";
                this.AutoSize = true;

                this.Controls.Add(new Elements.But("sick af"));
            }
        }
        public class About : TableLayoutPanel {
            public About() {
                this.Name = "About";
                this.AutoSize = true;

                this.Controls.Add(new Elements.Text($"{Settings.AppName}", true, 20));
                this.Controls.Add(new Elements.Text($"Version {Settings.AppVerString} ({Settings.AppVerNum})", false, -1, true));
                this.Controls.Add(new Elements.NewLine());
                this.Controls.Add(new Elements.Text("Developed by HJfod"));
            }
        }
    }
}