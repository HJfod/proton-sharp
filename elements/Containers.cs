using System.Windows.Forms;

namespace proton {
    namespace Elements {
        public class Container : FlowLayoutPanel {
            public Container() {
                this.AutoSize = true;
                this.Padding = Style.Padding;
            }
        }
    }
}