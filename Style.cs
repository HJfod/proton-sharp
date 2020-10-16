using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Dynamic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace proton {
    public static class Style {
        public static float Scale = ( (new Elements.Container()).CreateGraphics().DpiX / 96 );

        public static int _S(int prop, float sc = 0F) {
            if (sc == 0F) sc = Scale;
            return (int)((float)prop * sc);
        }

        public static int DraggerWidth = _S(6);
        public static int TitlebarSize = _S(30);
        public static int FooterSize = _S(20);
        public static int TabHeight = _S(40);
        public static int TabSideSize = _S(6);
        public static int PaddingSize = _S(15);
        public static int EditorTextSize = 12;
        public static int MenuOptionSize = _S(25);
        public static int UITextSize = _S(10);
        public static int MenuTextSize = _S(8);
        public static int MenuPadding = _S(8);
        public static int MenuDefWidth = _S(20);
        public static int SearchBoxWidth = _S(280);
        public static int CheckBoxStroke = _S(2);
        public static string EditorFont = "Segoe UI Light";
        public static Padding Padding = new Padding(PaddingSize);
        public static dynamic Fonts = new ExpandoObject();

        public static dynamic Colors = new ExpandoObject();

        public static void LoadStyle(dynamic list, Main _rel = null) {
            if (list is ExpandoObject)
                foreach (string k in ((IDictionary<String, Object>)list).Keys)
                    if (((IDictionary<String, Object>)list)[k].ToString().Contains(">"))
                        ((IDictionary<String, Object>)Colors)[k] = ((IDictionary<String, Object>)list)[k].ToString();
                    else
                        ((IDictionary<String, Object>)Colors)[k] = (Color)ColorTranslator.FromHtml(((IDictionary<String, Object>)list)[k].ToString());
            else
                foreach (PropertyInfo i in list.GetType().GetProperties())
                    ((IDictionary<String, Object>)Colors)[i.Name] = (Color)ColorTranslator.FromHtml(i.GetValue(list));

            if (_rel != null)
                _rel.SaveReload();

            PrivateFontCollection fc = new PrivateFontCollection();
            foreach (string file in Directory.GetFiles(@"resources/fonts"))
                if (file.EndsWith(".ttf")) {
                    fc.AddFontFile(Path.GetFullPath(file));
                    string name = Path.GetFileNameWithoutExtension(file);
                    ((IDictionary<String, Object>)Fonts)[name.Substring(0, name.IndexOf("_"))] = new FontFamily(name.Substring(name.IndexOf("_") + 1), fc);
                }
        }

        public static void Init() {
            LoadStyle(DefaultStyle.list);
        }

        public static Color Color(string hex) {
            return ColorTranslator.FromHtml(hex);
        }

        public static void DrawShadow(Graphics G, Color c, Color c2, Rectangle R, int d) {
            Color[] colors = getColorVector(c, c2, d).ToArray();
            for (int i = 0; i < d; i++)
            {
                G.TranslateTransform(1f, 0.75f);                // <== shadow vector!
                using (Pen pen = new Pen(colors[i], 1.75f  ) )  // <== pen width (*)
                    G.DrawPath(pen, getRectPath(R));
            }
            G.ResetTransform();
        }

        private static List<Color> getColorVector(Color fc, Color bc, int depth) {
            List<Color> cv = new List<Color>();
            float dRed = 1f * (bc.R - fc.R) / depth;
            float dGreen = 1f * (bc.G - fc.G) / depth;
            float dBlue = 1f * (bc.B - fc.B) / depth;
            for (int d = 1; d <= depth; d++)
                cv.Add(System.Drawing.Color.FromArgb(255,   (int) (fc.R + dRed * d),
                (int) (fc.G + dGreen * d), (int) (fc.B + dBlue * d) ));
            return cv;
        }

        private static GraphicsPath getRectPath(Rectangle R) {
            byte[] fm = new byte[3];
            for (int b = 0; b < 3; b++) fm[b] = 1;
            List<Point> points = new List<Point>();
            points.Add(new Point(R.Left, R.Bottom));
            points.Add(new Point(R.Right, R.Bottom));
            points.Add(new Point(R.Right, R.Top));
            return new GraphicsPath(points.ToArray(), fm);
        }
    }
}