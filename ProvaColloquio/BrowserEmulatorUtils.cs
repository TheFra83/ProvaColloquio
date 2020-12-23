using Microsoft.Win32;
using System;
using System.Windows;

namespace ProvaColloquio
{
    public class BrowserEmulatorUtils
    {
        const string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer";
        const string subKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
        const string exMess = "Versione di internet explorer non supportata.";

        public static void EnsureBrowserEmulationEnabled(string exename, bool uninstall = false)
        {

            using (var rk = Registry.CurrentUser.OpenSubKey(subKey, true))
            {
                if (!uninstall)
                {
                    rk.SetValue(exename, (uint)GetBrowserVersion(), RegistryValueKind.DWord);
                }
                else
                    rk.DeleteValue(exename);
            }
        }

        private static int GetBrowserVersion()
        {
            string strKeyPath = keyPath;
            string[] ls = new string[] { "svcVersion", "svcUpdateVersion", "Version", "W2kVersion" };

            int maxVer = 0;
            int retVer = 0;
            for (int i = 0; i < ls.Length; ++i)
            {
                object objVal = Registry.GetValue(strKeyPath, ls[i], "0");
                string strVal = Convert.ToString(objVal);
                if (strVal != null)
                {
                    int iPos = strVal.IndexOf('.');
                    if (iPos > 0)
                        strVal = strVal.Substring(0, iPos);

                    int res = 0;
                    if (int.TryParse(strVal, out res))
                        maxVer = Math.Max(maxVer, res);
                }
            }

            switch (maxVer)
            {
                case 7:
                    throw new UnsupportedVersionException(exMess);
                case 8:
                    retVer = 8888;
                    break;
                case 9:
                    retVer = 9999;
                    break;
                case 10:
                    retVer = 10001;
                    break;
                default:
                    retVer = 11001;
                    break;
            }

            return retVer;
        }
  

        [Serializable]
        public class UnsupportedVersionException : Exception
        {
            public UnsupportedVersionException()
            {

            }

            public UnsupportedVersionException(string message)
                : base(message)
            {

            }

        }
    }
}
