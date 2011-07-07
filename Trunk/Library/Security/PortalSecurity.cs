#region Copyright

// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.Security
{
    public class PortalSecurity
    {
        #region FilterFlag enum

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// The FilterFlag enum determines which filters are applied by the InputFilter
        /// function.  The Flags attribute allows the user to include multiple
        /// enumerated values in a single variable by OR'ing the individual values
        /// together.
        /// </summary>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created  Bug #000120, #000121
        /// </history>
        ///-----------------------------------------------------------------------------
        [Flags]
        public enum FilterFlag
        {
            MultiLine = 1,
            NoMarkup = 2,
            NoScripting = 4,
            NoSQL = 8,
            NoAngleBrackets = 16
        }

        #endregion
		
		#region "Private Methods"

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the FormatDisableScripting function
        /// </remarks>
        /// <history>
        ///     [cathal]        3/06/2007   Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FilterStrings(string strInput)
        {
			//setup up list of search terms as items may be used twice
            string TempInput = strInput;
            var listStrings = new List<string>();
            listStrings.Add("<script[^>]*>.*?</script[^><]*>");
            listStrings.Add("<script");
            listStrings.Add("<input[^>]*>.*?</input[^><]*>");
            listStrings.Add("<object[^>]*>.*?</object[^><]*>");
            listStrings.Add("<embed[^>]*>.*?</embed[^><]*>");
            listStrings.Add("<applet[^>]*>.*?</applet[^><]*>");
            listStrings.Add("<form[^>]*>.*?</form[^><]*>");
            listStrings.Add("<option[^>]*>.*?</option[^><]*>");
            listStrings.Add("<select[^>]*>.*?</select[^><]*>");
            listStrings.Add("<iframe[^>]*>.*?</iframe[^><]*>");
            listStrings.Add("<iframe.*?<");
            listStrings.Add("<iframe.*?");
            listStrings.Add("<ilayer[^>]*>.*?</ilayer[^><]*>");
            listStrings.Add("<form[^>]*>");
            listStrings.Add("</form[^><]*>");
            listStrings.Add("onerror");
            listStrings.Add("onmouseover");
            listStrings.Add("javascript:");
            listStrings.Add("vbscript:");
            listStrings.Add("alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?");

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            string strReplacement = " ";

            //check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (TempInput.Contains("&gt;") && TempInput.Contains("&lt;"))
            {
				//text is encoded, so decode and try again
                TempInput = HttpContext.Current.Server.HtmlDecode(TempInput);
                foreach (string s in listStrings)
                {
                    TempInput = Regex.Replace(TempInput, s, strReplacement, options);
                }
				
                //Re-encode
                TempInput = HttpContext.Current.Server.HtmlEncode(TempInput);
            }
            else
            {
                foreach (string s in listStrings)
                {
                    TempInput = Regex.Replace(TempInput, s, strReplacement, options);
                }
            }
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created Bug #000120
        ///     [cathal]        3/06/2007   Added check for encoded content
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FormatDisableScripting(string strInput)
        {
            string TempInput = strInput;
            TempInput = FilterStrings(TempInput);
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes angle brackets i.e.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Cathal] 	6/1/2006	Created to fufill client request
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FormatAngleBrackets(string strInput)
        {
            string TempInput = strInput.Replace("<", "");
            TempInput = TempInput.Replace(">", "");
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes CrLf characters and inserts br
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created Bug #000120
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FormatMultiLine(string strInput)
        {
            string TempInput = strInput.Replace(Environment.NewLine, "<br>");
            return TempInput.Replace("\r", "<br>");
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function verifies raw SQL statements to prevent SQL injection attacks
        /// and replaces a similar function (PreventSQLInjection) from the Common.Globals.vb module
        /// </summary>
        /// <param name="strSQL">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created Bug #000121
        ///     [Tom Lucas]     3/8/2004    Fixed   Bug #000114 (Aardvark)
        ///                     8/5/2009 added additional strings and performance tweak
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FormatRemoveSQL(string strSQL)
        {
            const string BadStatementExpression = ";|--|create|drop|select|insert|delete|update|union|sp_|xp_|exec|/\\*.*\\*/|declare";
            return Regex.Replace(strSQL, BadStatementExpression, " ", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace("'", "''");
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function determines if the Input string contains any markup.
        /// </summary>
        /// <param name="strInput">This is the string to be checked</param>
        /// <returns>True if string contains Markup tag(s)</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created Bug #000120
        /// </history>
        ///-----------------------------------------------------------------------------
        private bool IncludesMarkup(string strInput)
        {
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            string strPattern = "<[^<>]*>";
            return Regex.IsMatch(strInput, strPattern, options);
        }
		
		#endregion
		
		#region "Public Methods"

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function converts a byte array to a hex string
        /// </summary>
        /// <param name="bytes">An array of bytes</param>
        /// <returns>A string representing the hex converted value</returns>
        /// <remarks>
        /// This is a private function that is used internally by the CreateKey function
        /// </remarks>
        /// <history>
        /// </history>
        ///-----------------------------------------------------------------------------
        private string BytesToHexString(byte[] bytes)
        {
            var hexString = new StringBuilder(64);
            int counter;
            for (counter = 0; counter <= bytes.Length - 1; counter++)
            {
                hexString.Append(String.Format("{0:X2}", bytes[counter]));
            }
            return hexString.ToString();
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function creates a random key
        /// </summary>
        /// <param name="numBytes">This is the number of bytes for the key</param>
        /// <returns>A random string</returns>
        /// <remarks>
        /// This is a public function used for generating SHA1 keys
        /// </remarks>
        /// <history>
        /// </history>
        ///-----------------------------------------------------------------------------
        public string CreateKey(int numBytes)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[numBytes];
            rng.GetBytes(buff);
            return BytesToHexString(buff);
        }

        public static string EncryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5 
            // We use the MD5 hash generator as the result is a 128 bit byte array 
            // which is a valid length for the TripleDES encoder we use below 

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object 
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder 
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[] 
            byte[] DataToEncrypt = UTF8.GetBytes(Message);

            // Step 5. Attempt to encrypt the string 
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information 
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string 
            return Convert.ToBase64String(Results);
        }

        public static string DecryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5 
            // We use the MD5 hash generator as the result is a 128 bit byte array 
            // which is a valid length for the TripleDES encoder we use below 

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object 
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder 
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[] 
            byte[] DataToDecrypt = Convert.FromBase64String(Message);

            // Step 5. Attempt to decrypt the string 
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information 
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format 
            return UTF8.GetString(Results);
        }
        public string Decrypt(string strKey, string strData)
        {
            if (String.IsNullOrEmpty(strData))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
				//convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }
				
                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array and Base64 decode
                var byteData = new byte[strData.Length];
                try
                {
                    byteData = Convert.FromBase64String(strData);
                }
                catch //invalid length
                {
                    strValue = strData;
                }
                if (String.IsNullOrEmpty(strValue))
                {
                    try
                    {
						//decrypt
                        var objDES = new DESCryptoServiceProvider();
                        var objMemoryStream = new MemoryStream();
                        var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write);
                        objCryptoStream.Write(byteData, 0, byteData.Length);
                        objCryptoStream.FlushFinalBlock();

                        //convert to string
                        Encoding objEncoding = Encoding.UTF8;
                        strValue = objEncoding.GetString(objMemoryStream.ToArray());
                    }
                    catch //decryption error
                    {
                        strValue = "";
                    }
                }
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        public string Encrypt(string strKey, string strData)
        {
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
                //convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }
				
                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(strData);

                //encrypt 
                var objDES = new DESCryptoServiceProvider();
                var objMemoryStream = new MemoryStream();
                var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(byteKey, byteVector), CryptoStreamMode.Write);
                objCryptoStream.Write(byteData, 0, byteData.Length);
                objCryptoStream.FlushFinalBlock();

                //convert to string and Base64 encode
                strValue = Convert.ToBase64String(objMemoryStream.ToArray());
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function applies security filtering to the UserInput string.
        /// </summary>
        /// <param name="UserInput">This is the string to be filtered</param>
        /// <param name="FilterType">Flags which designate the filters to be applied</param>
        /// <returns>Filtered UserInput</returns>
        /// <history>
        /// 	[Joe Brinkman] 	8/15/2003	Created Bug #000120, #000121
        /// </history>
        ///-----------------------------------------------------------------------------
        public string InputFilter(string UserInput, FilterFlag FilterType)
        {
            if (UserInput == null)
            {
                return "";
            }
            string TempInput = UserInput;
            if ((FilterType & FilterFlag.NoAngleBrackets) == FilterFlag.NoAngleBrackets)
            {
                bool RemoveAngleBrackets;
                if (Config.GetSetting("RemoveAngleBrackets") == null)
                {
                    RemoveAngleBrackets = false;
                }
                else
                {
                    RemoveAngleBrackets = bool.Parse(Config.GetSetting("RemoveAngleBrackets"));
                }
                if (RemoveAngleBrackets)
                {
                    TempInput = FormatAngleBrackets(TempInput);
                }
            }
            if ((FilterType & FilterFlag.NoSQL) == FilterFlag.NoSQL)
            {
                TempInput = FormatRemoveSQL(TempInput);
            }
            else
            {
                if ((FilterType & FilterFlag.NoMarkup) == FilterFlag.NoMarkup && IncludesMarkup(TempInput))
                {
                    TempInput = HttpUtility.HtmlEncode(TempInput);
                }
                if ((FilterType & FilterFlag.NoScripting) == FilterFlag.NoScripting)
                {
                    TempInput = FormatDisableScripting(TempInput);
                }
                if ((FilterType & FilterFlag.MultiLine) == FilterFlag.MultiLine)
                {
                    TempInput = FormatMultiLine(TempInput);
                }
            }
            return TempInput;
        }

        public void SignOut()
        {
			//Log User Off from Cookie Authentication System
            FormsAuthentication.SignOut();
			//Remove current userinfo from context items
			HttpContext.Current.Items.Remove("UserInfo");

            //remove language cookie
            HttpContext.Current.Response.Cookies["language"].Value = "";

            //remove authentication type cookie
            HttpContext.Current.Response.Cookies["authentication"].Value = "";

            //expire cookies
            HttpContext.Current.Response.Cookies["portalaliasid"].Value = null;
            HttpContext.Current.Response.Cookies["portalaliasid"].Path = "/";
            HttpContext.Current.Response.Cookies["portalaliasid"].Expires = DateTime.Now.AddYears(-30);

            HttpContext.Current.Response.Cookies["portalroles"].Value = null;
            HttpContext.Current.Response.Cookies["portalroles"].Path = "/";
            HttpContext.Current.Response.Cookies["portalroles"].Expires = DateTime.Now.AddYears(-30);
        }
		
		#endregion
		
		#region "Public Shared/Static Methods"

        public static void ClearRoles()
        {
            HttpContext.Current.Response.Cookies["portalroles"].Value = null;
            HttpContext.Current.Response.Cookies["portalroles"].Path = "/";
            HttpContext.Current.Response.Cookies["portalroles"].Expires = DateTime.Now.AddYears(-30);
        }

        public static void ForceSecureConnection()
        {
			//get current url
            string URL = HttpContext.Current.Request.Url.ToString();
			//if unsecure connection
            if (URL.StartsWith("http://"))
            {
				//switch to secure connection
                URL = URL.Replace("http://", "https://");
                //append ssl parameter to querystring to indicate secure connection processing has already occurred
                if (URL.IndexOf("?") == -1)
                {
                    URL = URL + "?ssl=1";
                }
                else
                {
                    URL = URL + "&ssl=1";
                }
                //redirect to secure connection
                HttpContext.Current.Response.Redirect(URL, true);
            }
        }

        public static bool IsInRole(string role)
        {
            UserInfo objUserInfo = UserController.GetCurrentUserInfo();
            HttpContext context = HttpContext.Current;
            if ((!String.IsNullOrEmpty(role) && role != null && ((context.Request.IsAuthenticated == false && role == Globals.glbRoleUnauthUserName))))
            {
                return true;
            }
            else
            {
                return objUserInfo.IsInRole(role);
            }
        }

        public static bool IsInRoles(string roles)
        {
            UserInfo objUserInfo = UserController.GetCurrentUserInfo();

            //super user always has full access
            bool blnIsInRoles = objUserInfo.IsSuperUser;

            if (!blnIsInRoles)
            {
                if (roles != null)
                {
                    HttpContext context = HttpContext.Current;

                    //permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                    foreach (string role in roles.Split(new[] {';'}))
                    {
                        if (!string.IsNullOrEmpty(role))
                        {
							//Deny permission
                            if (role.StartsWith("!"))
                            {
                                //Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                                PortalSettings settings = PortalController.GetCurrentPortalSettings();
                                if (!(settings.PortalId == objUserInfo.PortalID && settings.AdministratorId == objUserInfo.UserID))
                                {
                                    string denyRole = role.Replace("!", "");
                                    if (((context.Request.IsAuthenticated == false && denyRole == Globals.glbRoleUnauthUserName) || denyRole == Globals.glbRoleAllUsersName ||
                                         objUserInfo.IsInRole(denyRole)))
                                    {
                                        blnIsInRoles = false;
                                        break;
                                    }
                                }
                            }
                            else //Grant permission
                            {
                                if (((context.Request.IsAuthenticated == false && role == Globals.glbRoleUnauthUserName) || role == Globals.glbRoleAllUsersName || objUserInfo.IsInRole(role)))
                                {
                                    blnIsInRoles = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return blnIsInRoles;
        }
		
		#endregion
		
		#region "Obsoleted Methods, retained for Binary Compatability"

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo, Username)")]
        public static bool HasEditPermissions(int ModuleId)
        {
            return
                ModulePermissionController.HasModulePermission(
                    new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(ModuleId, -1), typeof (ModulePermissionInfo))), "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasEditPermissions(ModulePermissionCollection objModulePermissions)
        {
            return ModulePermissionController.HasModulePermission(objModulePermissions, "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasEditPermissions(int ModuleId, int Tabid)
        {
            return ModulePermissionController.HasModulePermission(ModulePermissionController.GetModulePermissions(ModuleId, Tabid), "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration, string UserName)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration, UserInfo User)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use TabPermissionController.CanAdminPage")]
        public static bool IsPageAdmin()
        {
            return TabPermissionController.CanAdminPage();
        }

        [Obsolete("This function has been replaced by UserController.UserLogin")]
        public int UserLogin(string Username, string Password, int PortalID, string PortalName, string IP, bool CreatePersistentCookie)
        {
            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            int UserId = -1;
            UserInfo objUser = UserController.UserLogin(PortalID, Username, Password, "", PortalName, IP, ref loginStatus, CreatePersistentCookie);
            if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
            {
                UserId = objUser.UserID;
            }
            return UserId;
        }
		
		#endregion
    }
}
