using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace proton {
    public class MenuWindow : TableLayoutPanel {
        public MenuWindow(Control _par, dynamic[] _menu, bool _isTop = false, Point? _loc = null) {
            this.Name = "__MenuWindow";
            this.BackColor = Color.White;
            
            this.Location = _loc == null ? new Point(
                Cursor.Position.X - _par.Left,
                Cursor.Position.Y - _par.Top
            ) : (Point)_loc;

            this.Size = new Size(300, 200);

            Label y = new Label();
            y.Text = "ss";

            this.Controls.Add(y);
            this.BringToFront();
        }
    }
}