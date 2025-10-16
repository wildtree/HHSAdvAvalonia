// Properties for HHSAdvSDL

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using ReactiveUI;

namespace HHSAdvAvalonia
{
    public enum ThemeType { Light, Dark, System }
    public class ZProperties
    {
        public class Attributes //: ReactiveObject
        {
            private ThemeType themeMode = ThemeType.Light;
            private int fontSize = 12;
            public string FontPath { get; set; } = @"C:\Windows\Fonts\YuGothR.ttc";
            public bool OpeningRoll { get; set; } = true;
            public bool PlaySound { get; set; } = true;
            public ThemeType ThemeMode {
                get => themeMode;
                set => themeMode = value;
            }
            public int FontSize {
                get => fontSize;
                set => fontSize = value;
            }
        }

        private Attributes attributes = new Attributes();

        public Attributes Attrs
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public ZProperties()
        {
        }

        public bool Load(string fileName)
        {
            if (!File.Exists(fileName)) return false;

            string jsonString = File.ReadAllText(fileName);
            if (string.IsNullOrEmpty(jsonString))
            {
                return false;
            }
            var deserializedAttributes = JsonSerializer.Deserialize<Attributes>(jsonString);
            if (deserializedAttributes == null)
            {
                return false;
            }
            attributes = deserializedAttributes;

            return true;
        }
        public bool Save(string fileName)
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(attributes));
            return true;
        }

    }
}
