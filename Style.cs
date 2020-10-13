using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Dynamic;

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
        public static int UITextSize = _S(12);
        public static Padding Padding = new Padding(PaddingSize);
        public static string UIFontName = "Segoe UI Light";
        public static Font UIFont = new Font(UIFontName, UITextSize);

        public static dynamic Colors = new ExpandoObject();

        public static void LoadStyle(dynamic list) {
                foreach (PropertyInfo i in list.GetType().GetProperties()) {
                    ((IDictionary<String, Object>)Colors)[i.Name] = (Color)ColorTranslator.FromHtml(i.GetValue(list));
                }
        }

        public static void Init() {
            LoadStyle(DefaultStyle.list);
        }

        public static Color Color(string hex) {
            return ColorTranslator.FromHtml(hex);
        }
    }
}