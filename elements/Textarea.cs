using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Textarea : RichTextBox {
            public Textarea() {
                this.Padding = Style.Padding;
                this.BorderStyle = BorderStyle.None;
                this.AcceptsTab = true;
                this.DoubleBuffered = true;
            }
        }
    }
}