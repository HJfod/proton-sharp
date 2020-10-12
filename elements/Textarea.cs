using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace proton {
    namespace Elements {
        public class Textarea : RichTextBox {
            public Textarea() {
                this.Padding = Style.Padding;
                this.BorderStyle = BorderStyle.None;
                this.AcceptsTab = true;
                this.DoubleBuffered = true;

                this.KeyDown += (s, e) => {
                    switch (e.KeyCode) {
                        case Keys.Enter:
                            try {
                                string curr = this.Lines[this.GetLineFromCharIndex(this.SelectionStart)];
                                this.SelectedText = "\n"
                                + new String('\t', curr.Count(f => f == '\t'))
                                + new String(' ',  curr.TakeWhile(c => c == ' ').Count());
                                e.Handled = true;
                            } catch (Exception) {};
                            break;
                        case Keys.Tab:
                            try {
                                bool shift = Control.ModifierKeys == Keys.Shift;
                                bool sel = this.SelectedText.Trim() == this.Lines[this.GetLineFromCharIndex(this.SelectionStart)].Trim();
                                if (shift || sel) {
                                    int[] ss = { this.SelectionStart + (shift ? -1 : 1), this.SelectionLength };
                                    int curr = this.GetLineFromCharIndex(this.SelectionStart);

                                    if (shift) {
                                        string[] newLines = this.Lines;
                                        if (this.Lines[curr].StartsWith('\t'))
                                            newLines[curr] = this.Lines[curr].Substring(1);
                                        this.Lines = newLines;
                                    } else {
                                        this.SelectedText = $"\t{this.SelectedText}";
                                    }

                                    this.SelectionStart = ss[0];
                                    this.SelectionLength = ss[1];
                                    e.Handled = true;
                                }
                            } catch (Exception) {};
                            break;
                        /* default: #### NEED TO FIGURE OUT SOME WAY TO GET THE ANNOYING BEEP SOUND TO NOT PLAY EVERY TIME YOU PRESS ARROW KEYS, THIS DOES NOT WORK ####
                            this.Text = this.Text.Insert(this.SelectionStart, e.KeyValue);
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            break; //*/
                    }
                };
            }
        }
    }
}