using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Microsoft.Research.Science.Data
{
    public class CredentialsManager
    {
        private const string DefaultLogin = "anonymous";
        private const string DefaultPassword = "anonymous";

        public static void GetCredentials(out string login, out string password)
        {
            RegistryKey rkey = null;
            RegistryKey rsubkey = null;

            try
            {
                rkey = Registry.CurrentUser;
                rsubkey = rkey.OpenSubKey("SOFTWARE\\Microsoft");

				if (rsubkey == null ||
					!rsubkey.GetSubKeyNames().Contains("FetchClimate"))
                {
                    login = CredentialsManager.DefaultLogin;
                    password = CredentialsManager.DefaultPassword;
                    return;
                }

                rsubkey.Close();
                rsubkey = rkey.OpenSubKey("SOFTWARE\\Microsoft\\FetchClimate");

                string privilegedLogin = rsubkey.GetValue("login") as string;
                string privilegedPassword = rsubkey.GetValue("password") as string;

                if (string.IsNullOrEmpty(privilegedLogin) || string.IsNullOrEmpty(privilegedPassword))
                {
                    login = CredentialsManager.DefaultLogin;
                    password = CredentialsManager.DefaultPassword;
                    return;
                }

                login = privilegedLogin;
                password = privilegedPassword;

                return;
            }
            finally
            {
                if (rkey != null)
                    rkey.Close();
                if (rsubkey != null)
                    rsubkey.Close();
            }
        }
    }
}
