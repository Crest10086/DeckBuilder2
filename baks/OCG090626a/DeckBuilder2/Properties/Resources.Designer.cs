﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeckBuilder2.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DeckBuilder2.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static System.Drawing.Bitmap Disconnect {
            get {
                object obj = ResourceManager.GetObject("Disconnect", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Find {
            get {
                object obj = ResourceManager.GetObject("Find", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;MenuList&gt;
        ///  &lt;Menu MenuId=&quot;0&quot; Text=&quot;限制类型&quot;&gt;
        ///    &lt;MenuItem Id=&quot;0&quot; Text=&quot;全部&quot; ClickedText=&quot;限制类型&quot;&gt;
        ///      &lt;SearchText&gt;&lt;/SearchText&gt;
        ///    &lt;/MenuItem&gt;		
        ///    &lt;Separator Id=&quot;1&quot;&gt;&lt;/Separator&gt;
        ///    &lt;MenuItem Id=&quot;2&quot; Text=&quot;禁止卡&quot;&gt;
        ///      &lt;SearchText&gt;+limit:0&lt;/SearchText&gt;
        ///    &lt;/MenuItem&gt;
        ///    &lt;MenuItem Id=&quot;3&quot; Text=&quot;限制卡&quot;&gt;
        ///      &lt;SearchText&gt;+limit:1&lt;/SearchText&gt;
        ///    &lt;/MenuItem&gt;
        ///    &lt;MenuItem Id=&quot;4&quot; Text=&quot;准限制卡&quot;&gt;
        ///      &lt;SearchText&gt;+limit:2&lt;/SearchText&gt;
        ///    &lt;/MenuItem&gt;
        ///    &lt;MenuI [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MenuList {
            get {
                return ResourceManager.GetString("MenuList", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap search4 {
            get {
                object obj = ResourceManager.GetObject("search4", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
