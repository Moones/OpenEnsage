// Decompiled with JetBrains decompiler
// Type: Loader.Config
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Loader
{
    [DataContract(Name = "ConfigClass")]
    public class Config
    {
        [DataMember(Name = "ZoomValue")]
        private int _zoomValue = 1134;
        [DataMember(Name = "AutoAccept")]
        private bool _autoAccept = true;
        [DataMember(Name = "LogFile")]
        private bool _logfile = true;
        [DataMember(Name = "AutoConnectToChat")]
        private bool _autoConnectToChat = true;
        [DataMember(Name = "ChatLanguage")]
        private string _chatLanguage = "English";
        [DataMember(Name = "DebugConsole")]
        private bool _debugConsole;
        [DataMember(Name = "WarnedNew")]
        private bool _warned;
        [DataMember(Name = "SystemTray")]
        private bool _systemTray;

        [Description("Opens a separate console for debugging scripts.")]
        [Category("Ingame")]
        [DefaultValue(false)]
        public bool DebugConsole
        {
            get
            {
                return _debugConsole;
            }
            set
            {
                _debugConsole = value;
            }
        }

        [DefaultValue(1134)]
        [Category("Ingame")]
        [Description("Select your default zoom value.")]
        public int ZoomValue
        {
            get
            {
                return _zoomValue;
            }
            set
            {
                _zoomValue = Math.Max(Math.Min(value, 1700), 1134);
            }
        }

        [Description("Minimize to system tray.")]
        [Category("Loader")]
        [DefaultValue(false)]
        public bool SystemTray
        {
            get
            {
                return _systemTray;
            }
            set
            {
                _systemTray = value;
            }
        }

        [Category("Loader")]
        [DefaultValue(true)]
        [Description("Log all debug output to a file.")]
        public bool LogFile
        {
            get
            {
                return _logfile;
            }
            set
            {
                _logfile = value;
            }
        }

        [Description("Auto accept game found message.")]
        [DefaultValue(true)]
        [Category("Loader")]
        public bool AutoAccept
        {
            get
            {
                return _autoAccept;
            }
            set
            {
                _autoAccept = value;
            }
        }

        [Browsable(false)]
        public bool AutoConnectToChat
        {
            get
            {
                return _autoConnectToChat;
            }
            set
            {
                _autoConnectToChat = value;
            }
        }

        [Browsable(false)]
        [DefaultValue("English")]
        public string ChatLanguage
        {
            get
            {
                if (_chatLanguage != null)
                    return _chatLanguage;
                switch (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName)
                {
                    case "de":
                        return "German";
                    case "en":
                        return "English";
                    default:
                        return "Russian";
                }
            }
            set
            {
                _chatLanguage = value;
            }
        }

        public static void Load(out Config c)
        {
            using (XmlTextReader xmlTextReader = new XmlTextReader("config.xml"))
            {
                DataContractSerializer contractSerializer = new DataContractSerializer(typeof(Config));
                c = contractSerializer.ReadObject(xmlTextReader) as Config;
            }
        }

        public static void Save(Config c)
        {
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter("config.xml", Encoding.UTF8)
            {
                Formatting = Formatting.Indented
            })
            {
                xmlTextWriter.WriteStartDocument(false);
                new DataContractSerializer(typeof(Config)).WriteObject(xmlTextWriter, c);
                xmlTextWriter.Flush();
            }
        }

        public void WarningShown()
        {
            _warned = true;
            Config.Save(this);
        }

        public bool IsWarningShown()
        {
            return _warned;
        }
    }
}
