﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KeyboardMouseWin {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LeftCtrl,LeftAlt,W")]
        public string CaptionKeyCombination {
            get {
                return ((string)(this["CaptionKeyCombination"]));
            }
            set {
                this["CaptionKeyCombination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ClickOnCenterOfBoundingRectangle {
            get {
                return ((bool)(this["ClickOnCenterOfBoundingRectangle"]));
            }
            set {
                this["ClickOnCenterOfBoundingRectangle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Escape")]
        public string ClearKeyCombiantion {
            get {
                return ((string)(this["ClearKeyCombiantion"]));
            }
            set {
                this["ClearKeyCombiantion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LeftAlt,LeftShift,A")]
        public string ReclickLastPointKeyCombination {
            get {
                return ((string)(this["ReclickLastPointKeyCombination"]));
            }
            set {
                this["ReclickLastPointKeyCombination"] = value;
            }
        }
    }
}
