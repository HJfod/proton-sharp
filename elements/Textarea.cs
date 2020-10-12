using System;
using System.Linq;
using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Textarea : RichTextBox {
            public Textarea() {
                this.Padding = Style.Padding;
                this.BorderStyle = BorderStyle.None;
                this.AcceptsTab = true;
                this.DoubleBuffered = true;

                this.KeyDown += (s, e) => {
                    if (e.KeyCode == Keys.Enter) {
                        try {
                            string curr = this.Lines[this.GetLineFromCharIndex(this.SelectionStart)];
                            this.SelectedText = "\n"
                            + new String('\t', curr.Count(f => f == '\t'))
                            + new String(' ',  curr.TakeWhile(c => c == ' ').Count());
                            e.Handled = true;
                        } catch (Exception err) {};
                    }
                };
            }
        }
    }
}