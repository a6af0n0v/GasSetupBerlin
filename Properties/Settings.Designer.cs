﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeasureConsole.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.8.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int Version {
            get {
                return ((int)(this["Version"]));
            }
            set {
                this["Version"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ArduinoPort {
            get {
                return ((string)(this["ArduinoPort"]));
            }
            set {
                this["ArduinoPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int PalmsensePortIndex {
            get {
                return ((int)(this["PalmsensePortIndex"]));
            }
            set {
                this["PalmsensePortIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300")]
        public double JSListWidth {
            get {
                return ((double)(this["JSListWidth"]));
            }
            set {
                this["JSListWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int LastFilesMaxCount {
            get {
                return ((int)(this["LastFilesMaxCount"]));
            }
            set {
                this["LastFilesMaxCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM20")]
        public string HuberPort {
            get {
                return ((string)(this["HuberPort"]));
            }
            set {
                this["HuberPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int HuberPollingInterval {
            get {
                return ((int)(this["HuberPollingInterval"]));
            }
            set {
                this["HuberPollingInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LogFolder {
            get {
                return ((string)(this["LogFolder"]));
            }
            set {
                this["LogFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string CSVFolder {
            get {
                return ((string)(this["CSVFolder"]));
            }
            set {
                this["CSVFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DataFolder {
            get {
                return ((string)(this["DataFolder"]));
            }
            set {
                this["DataFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("N; timestamp; MFC1_Flow; MFC2_Flow; T; rH; P; V0; V1; V2; V3; V4; V5; Huber; SHTr" +
            "H; SHTt")]
        public string CSVColumnNames {
            get {
                return ((string)(this["CSVColumnNames"]));
            }
            set {
                this["CSVColumnNames"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-273")]
        public int huberFaultTValue {
            get {
                return ((int)(this["huberFaultTValue"]));
            }
            set {
                this["huberFaultTValue"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoConnect {
            get {
                return ((bool)(this["AutoConnect"]));
            }
            set {
                this["AutoConnect"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public int MaxPointsOnChart {
            get {
                return ((int)(this["MaxPointsOnChart"]));
            }
            set {
                this["MaxPointsOnChart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int MaxNumberOfLinesInCSV {
            get {
                return ((int)(this["MaxNumberOfLinesInCSV"]));
            }
            set {
                this["MaxNumberOfLinesInCSV"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int MaxNumberOfMsgInStatusBar {
            get {
                return ((int)(this["MaxNumberOfMsgInStatusBar"]));
            }
            set {
                this["MaxNumberOfMsgInStatusBar"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int MaxNumberOfLinesInLog {
            get {
                return ((int)(this["MaxNumberOfLinesInLog"]));
            }
            set {
                this["MaxNumberOfLinesInLog"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsd=\"http://www.w3." +
            "org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <s" +
            "tring>test.jks</string>\r\n  <string>last.js</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection LastFiles {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["LastFiles"]));
            }
            set {
                this["LastFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("setup4.png")]
        public string SceneFileName {
            get {
                return ((string)(this["SceneFileName"]));
            }
            set {
                this["SceneFileName"] = value;
            }
        }
    }
}
