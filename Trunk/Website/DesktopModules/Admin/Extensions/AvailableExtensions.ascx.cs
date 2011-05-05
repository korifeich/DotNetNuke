#region Copyright

// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;

using ICSharpCode.SharpZipLib.Zip;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class AvailableExtensions : ModuleUserControlBase
    {
        private const string DefaultExtensionImage = "icon_extensions_32px.gif";
        private const string DefaultLanguageImage = "icon_languagePack.gif";
        private const string DefaultAuthenicationImage = "icon_authentication.png";
        private const string DefaultContainerImage = "icon_container.gif";
        private const string DefaultSkinImage = "icon_skin.gif";
        private const string DefaultProviderImage = "icon_provider.gif";

        private IDictionary<string, string> _packageTypes;

        protected IDictionary<string, string> PackageTypesList
        {
            get
            {
                if ((_packageTypes == null))
                {
                    _packageTypes = new Dictionary<string, string>();
                    foreach (PackageType packageType in PackageController.GetPackageTypes())
                    {
                        string installPath;
                        string type;
                        switch (packageType.Type)
                        {
                            case "Auth_System":
                                type = "AuthSystem";
                                installPath = Globals.ApplicationMapPath + "\\Install\\AuthSystem";
                                break;
                            case "Module":
                            case "Skin":
                            case "Container":
                            case "Provider":
                                type = packageType.Type;
                                installPath = Globals.ApplicationMapPath + "\\Install\\" + packageType.Type;
                                break;
                            case "CoreLanguagePack":
                                type = "Language";
                                installPath = Globals.ApplicationMapPath + "\\Install\\Language";
                                break;
                            default:
                                type = String.Empty;
                                installPath = String.Empty;
                                break;
                        }
                        if (!String.IsNullOrEmpty(type) && Directory.Exists(installPath) && 
                            (Directory.GetFiles(installPath, "*.zip").Length > 0 || Directory.GetFiles(installPath, "*.resources").Length > 0))
                        {
                            _packageTypes[type] = installPath;
                        }
                    }
                }
                return _packageTypes;
            }
        }
        
        private void BindGrid(string installPath, DataGrid grid)
        {
            var packages = new List<PackageInfo>();

            foreach (string file in Directory.GetFiles(installPath))
            {
                if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                {
                    Stream inputStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    var unzip = new ZipInputStream(inputStream);

                    try
                    {
                        ZipEntry entry = unzip.GetNextEntry();
                        while (entry != null && !entry.IsDirectory)
                        {
                            var fileName = entry.Name;
                            string extension = System.IO.Path.GetExtension(fileName);
                            if (extension.ToLower() == ".dnn" || extension.ToLower() == ".dnn5")
                            {
                                //Manifest
                                var manifestReader = new StreamReader(unzip);
                                var manifest = manifestReader.ReadToEnd();

                                var package = new PackageInfo();
                                package.Manifest = manifest;
                                if (!string.IsNullOrEmpty(manifest))
                                {
                                    var doc = new XPathDocument(new StringReader(manifest));
                                    XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
                                    string packageType = String.Empty;
                                    if (rootNav.Name == "dotnetnuke")
                                    {
                                        packageType = XmlUtils.GetAttributeValue(rootNav, "type");
                                    }
                                    else if (rootNav.Name.ToLower() == "languagepack")
                                    {
                                        packageType = "LanguagePack";
                                    }
                                    XPathNavigator nav = null;
                                    switch (packageType.ToLower())
                                    {
                                        case "package":
                                            nav = rootNav.SelectSingleNode("packages/package");
                                            break;
                                        case "module":
                                        case "languagepack":
                                        case "skinobject":
                                            nav = Installer.ConvertLegacyNavigator(rootNav, new InstallerInfo()).SelectSingleNode("packages/package");
                                            break;
                                    }

                                    if (nav != null)
                                    {
                                        package.Name = XmlUtils.GetAttributeValue(nav, "name");
                                        package.PackageType = XmlUtils.GetAttributeValue(nav, "type");
                                        package.IsSystemPackage = XmlUtils.GetAttributeValueAsBoolean(nav, "isSystem", false);
                                        package.Version = new Version(XmlUtils.GetAttributeValue(nav, "version"));
                                        package.FriendlyName = XmlUtils.GetNodeValue(nav, "friendlyName");
                                        package.Description = XmlUtils.GetNodeValue(nav, "description");

                                        XPathNavigator foldernameNav = null;
                                        switch (package.PackageType)
                                        {
                                            case "Module":
                                            case "Auth_System":
                                                foldernameNav = nav.SelectSingleNode("components/component/files");
                                                if (foldernameNav != null) package.FolderName = Util.ReadElement(foldernameNav, "basePath").Replace('\\', '/');
                                                break;
                                            case "Container":
                                                foldernameNav = nav.SelectSingleNode("components/component/containerFiles");
                                                if (foldernameNav != null) package.FolderName = Globals.glbContainersPath + Util.ReadElement(foldernameNav, "containerName").Replace('\\', '/');
                                                break;
                                            case "Skin":
                                                foldernameNav = nav.SelectSingleNode("components/component/skinFiles");
                                                if (foldernameNav != null) package.FolderName = Globals.glbSkinsPath + Util.ReadElement(foldernameNav, "skinName").Replace('\\', '/');
                                                break;
                                            default:
                                                break;
                                        }

                                        XPathNavigator iconFileNav = nav.SelectSingleNode("iconFile");
                                        if (package.FolderName != string.Empty && iconFileNav != null)
                                        {

                                            if ((iconFileNav.Value != string.Empty) && (package.PackageType == "Module" || package.PackageType == "Auth_System" || package.PackageType == "Container" || package.PackageType == "Skin"))
                                            {
                                                package.IconFile = package.FolderName + "/" + iconFileNav.Value;
                                                package.IconFile = (!package.IconFile.StartsWith("~/")) ? "~/" + package.IconFile : package.IconFile;
                                            }
                                        }

                                        packages.Add(package);
                                    }
                                }

                                break;
                            }
                            entry = unzip.GetNextEntry();
                        }
                    }
                    finally
                    {
                        if (unzip != null)
                        {
                            unzip.Close();
                            unzip.Dispose();
                        }
                    }                    
                }
            }

            grid.DataSource = packages;
            grid.DataBind();

            Localization.LocalizeDataGrid(ref grid, LocalResourceFile);           
        }

        private void BindPackageTypes()
        {
            extensionTypeRepeater.DataSource = PackageTypesList;
            extensionTypeRepeater.DataBind();
        }

        //private bool InstallAuthSystems()
        //{
        //    return InstallPackageItems("AuthSystem", lstAuthSystems, lblNoAuthSystems, "InstallAuthSystemError");
        //}

        //private bool InstallLanguages()
        //{
        //    return InstallPackageItems("Language", lstLanguages, lblLanguagesError, "InstallLanguageError");
        //}

        //private bool InstallModules()
        //{
        //    return InstallPackageItems("Module", lstModules, lblModulesError, "InstallModuleError");
        //}

        //private bool InstallPackageItems(string packageType, CheckBoxList list, Label errorLabel, string errorKey)
        //{
        //    bool success = false;
        //    string strErrorMessage = Null.NullString;
        //    int scriptTimeOut = Server.ScriptTimeout;
        //    try
        //    {
        //        Server.ScriptTimeout = int.MaxValue;
        //        string InstallPath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
        //        foreach (ListItem packageItem in list.Items)
        //        {
        //            if (packageItem.Selected)
        //            {
        //                success = Upgrade.InstallPackage(InstallPath + "\\" + packageItem.Value, packageType, false);
        //                if (!success)
        //                {
        //                    strErrorMessage += string.Format(Localization.GetString(errorKey, LocalResourceFile), packageItem.Text);
        //                }
        //            }
        //        }
        //        success = string.IsNullOrEmpty(strErrorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        DnnLog.Debug(ex);
        //        strErrorMessage = ex.StackTrace;
        //    }
        //    finally
        //    {
        //        Server.ScriptTimeout = scriptTimeOut;
        //    }
        //    if (!success)
        //    {
        //        errorLabel.Text += strErrorMessage;
        //    }
        //    return success;
        //}

        //private bool InstallSkins()
        //{
        //    bool skinSuccess = InstallPackageItems("Skin", lstSkins, lblSkinsError, "InstallSkinError");
        //    bool containerSuccess = InstallPackageItems("Container", lstContainers, lblSkinsError, "InstallContainerError");
        //    return skinSuccess && containerSuccess;
        //}

        protected string FormatVersion(object version)
        {
            var package = version as PackageInfo;
            string retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Version.ToString(3);
            }
            return retValue;
        }

        protected string GetAboutTooltip(object dataItem)
        {
            string returnValue = string.Empty;
            try
            {
                if ((ModuleContext.PortalSettings.ActiveTab.IsSuperTab))
                {
                    int portalID = Convert.ToInt32(DataBinder.Eval(dataItem, "PortalID"));
                    if ((portalID != Null.NullInteger && portalID != int.MinValue))
                    {
                        var controller = new PortalController();
                        PortalInfo portal = controller.GetPortal(portalID);
                        returnValue = string.Format(Localization.GetString("InstalledOnPortal.Tooltip", LocalResourceFile), portal.PortalName);
                    }
                    else
                    {
                        returnValue = Localization.GetString("InstalledOnHost.Tooltip", LocalResourceFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
            return returnValue;
        }

        protected string GetPackageDescription(object dataItem)
        {
            var package = dataItem as PackageInfo;
            string retValue = Null.NullString;
            if (package != null)
            {
                retValue = package.Description;
            }
            return retValue;
        }

        protected string GetPackageIcon(object dataItem)
        {
            var package = dataItem as PackageInfo;
            switch (package.PackageType)
            {
                case "Module":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
                case "Container":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultContainerImage;
                case "Skin":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultSkinImage;
                case "AuthenticationSystem":
                case "Auth_System":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultAuthenicationImage;
                case "CoreLanguagePack":
                case "ExtensionLanguagePack":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultLanguageImage;
                case "Provider":
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultProviderImage;
                default:
                    return (!String.IsNullOrEmpty(package.IconFile)) ? package.IconFile : Globals.ImagePath + DefaultExtensionImage;
            }
        }

        protected string GetPackageType(object dataItem)
        {
            var kvp = (KeyValuePair<string, string>)dataItem;

            return Localization.GetString(kvp.Key + ".Type", LocalResourceFile);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //cmdInstall.Click += cmdInstall_Click;
            extensionTypeRepeater.ItemDataBound += extensionTypeRepeater_ItemDataBound;
            
            BindPackageTypes();
        }

        //protected void cmdInstall_Click(object sender, EventArgs e)
        //{
        //    bool moduleSuccess;
        //    bool skinSuccess;
        //    bool languagesSuccess;
        //    bool AuthSystemSuccess;
        //    if (lstAuthSystems.SelectedIndex == Null.NullInteger && lstContainers.SelectedIndex == Null.NullInteger && lstSkins.SelectedIndex == Null.NullInteger &&
        //        lstModules.SelectedIndex == Null.NullInteger && lstLanguages.SelectedIndex == Null.NullInteger)
        //    {
        //        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoneSelected", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
        //        return;
        //    }
        //    moduleSuccess = InstallModules();
        //    skinSuccess = InstallSkins();
        //    languagesSuccess = InstallLanguages();
        //    AuthSystemSuccess = InstallAuthSystems();
        //    if (moduleSuccess && skinSuccess && languagesSuccess && AuthSystemSuccess)
        //    {
        //        Response.Redirect(Request.RawUrl, true);
        //    }
        //}

        private void extensionTypeRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var kvp = (KeyValuePair<string, string>)e.Item.DataItem;

                DataGrid extensionsGrid = item.Controls[1] as DataGrid;

                BindGrid(kvp.Value, extensionsGrid);
            }
        }
    }
}