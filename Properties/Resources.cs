using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Loader.Properties
{
    [DebuggerNonUserCode]
    [CompilerGenerated]
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class Resources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Resources.resourceMan, null))
                    Resources.resourceMan = new ResourceManager("Loader.Properties.Resources", typeof(Resources).Assembly);
                return Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Resources.resourceCulture;
            }
            set
            {
                Resources.resourceCulture = value;
            }
        }

        internal static Bitmap bad
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("bad", Resources.resourceCulture);
            }
        }

        internal static Bitmap chat_kappa
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("chat_kappa", Resources.resourceCulture);
            }
        }

        internal static string Error
        {
            get
            {
                return Resources.ResourceManager.GetString("Error", Resources.resourceCulture);
            }
        }

        internal static string Error_AccountName
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_AccountName", Resources.resourceCulture);
            }
        }

        internal static string Error_AccPwIncorrect
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_AccPwIncorrect", Resources.resourceCulture);
            }
        }

        internal static string Error_CantAllocSpace
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_CantAllocSpace", Resources.resourceCulture);
            }
        }

        internal static string Error_CreateRemoteThread
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_CreateRemoteThread", Resources.resourceCulture);
            }
        }

        internal static string Error_OpenProcess
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_OpenProcess", Resources.resourceCulture);
            }
        }

        internal static string Error_Password
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_Password", Resources.resourceCulture);
            }
        }

        internal static string Error_WriteMemory
        {
            get
            {
                return Resources.ResourceManager.GetString("Error_WriteMemory", Resources.resourceCulture);
            }
        }

        internal static string ErrorException
        {
            get
            {
                return Resources.ResourceManager.GetString("ErrorException", Resources.resourceCulture);
            }
        }

        internal static string ErrorNoInstallation
        {
            get
            {
                return Resources.ResourceManager.GetString("ErrorNoInstallation", Resources.resourceCulture);
            }
        }

        internal static string ErrorNoScriptsDir
        {
            get
            {
                return Resources.ResourceManager.GetString("ErrorNoScriptsDir", Resources.resourceCulture);
            }
        }

        internal static string ErrorOnConfigSave
        {
            get
            {
                return Resources.ResourceManager.GetString("ErrorOnConfigSave", Resources.resourceCulture);
            }
        }

        internal static Bitmap exclamation
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("exclamation", Resources.resourceCulture);
            }
        }

        internal static Bitmap good
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("good", Resources.resourceCulture);
            }
        }

        internal static Icon loader
        {
            get
            {
                return (Icon)Resources.ResourceManager.GetObject("loader", Resources.resourceCulture);
            }
        }

        internal static string Main_Main_Shown_Error
        {
            get
            {
                return Resources.ResourceManager.GetString("Main_Main_Shown_Error", Resources.resourceCulture);
            }
        }

        internal static Bitmap okay
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("okay", Resources.resourceCulture);
            }
        }

        internal static Bitmap question
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("question", Resources.resourceCulture);
            }
        }

        internal static string Stars
        {
            get
            {
                return Resources.ResourceManager.GetString("Stars", Resources.resourceCulture);
            }
        }

        internal static string StatusInjecting
        {
            get
            {
                return Resources.ResourceManager.GetString("StatusInjecting", Resources.resourceCulture);
            }
        }

        internal static string StatusNotStartedYet
        {
            get
            {
                return Resources.ResourceManager.GetString("StatusNotStartedYet", Resources.resourceCulture);
            }
        }

        internal static string Warning
        {
            get
            {
                return Resources.ResourceManager.GetString("Warning", Resources.resourceCulture);
            }
        }

        internal static string Warning_CantCreateFile
        {
            get
            {
                return Resources.ResourceManager.GetString("Warning_CantCreateFile", Resources.resourceCulture);
            }
        }

        internal static string WarningClose
        {
            get
            {
                return Resources.ResourceManager.GetString("WarningClose", Resources.resourceCulture);
            }
        }

        internal Resources()
        {
        }
    }
}
