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
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Vendors;
using DotNetNuke.UI;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    public partial class EditVendors : PortalModuleBase
    {
        public int VendorID = -1;

        private void ReturnUrl(string filter)
        {
            if (string.IsNullOrEmpty(filter.Trim()))
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            else
            {
                Response.Redirect(Globals.NavigateURL(TabId, Null.NullString, "filter=" + filter), true);
            }
        }

        private void AddModuleMessage(string message, ModuleMessage.ModuleMessageType type)
        {
            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString(message, LocalResourceFile), type);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            try
            {
                var objTabs = new TabController();
                TabInfo objTab;
                bool blnBanner = false;
                bool blnSignup = false;
                if ((Request.QueryString["VendorID"] != null))
                {
                    VendorID = Int32.Parse(Request.QueryString["VendorID"]);
                }
                if (Request.QueryString["ctl"] != null && VendorID == -1)
                {
                    blnSignup = true;
                }
                if (Request.QueryString["banner"] != null)
                {
                    blnBanner = true;
                }
                if (Page.IsPostBack == false)
                {
                    ctlLogo.FileFilter = Globals.glbImageFileTypes;
                    addresssVendor.ModuleId = ModuleId;
                    addresssVendor.StartTabIndex = 4;
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
                    var objClassifications = new ClassificationController();
                    ArrayList arr = objClassifications.GetVendorClassifications(VendorID);
                    int i;
                    for (i = 0; i <= arr.Count - 1; i++)
                    {
                        var lstItem = new ListItem();
                        var objClassification = (ClassificationInfo) arr[i];
                        lstItem.Text = objClassification.ClassificationName;
                        lstItem.Value = objClassification.ClassificationId.ToString();
                        lstItem.Selected = objClassification.IsAssociated;
                        lstClassifications.Items.Add(lstItem);
                    }
                    var objVendors = new VendorController();
                    if (VendorID != -1)
                    {
                        VendorInfo objVendor;
                        if (PortalSettings.ActiveTab.ParentId == PortalSettings.SuperTabId && UserInfo.IsSuperUser)
                        {
                            objVendor = objVendors.GetVendor(VendorID, Null.NullInteger);
                        }
                        else
                        {
                            objVendor = objVendors.GetVendor(VendorID, PortalId);
                        }
                        if (objVendor != null)
                        {
                            txtVendorName.Text = objVendor.VendorName;
                            txtFirstName.Text = objVendor.FirstName;
                            txtLastName.Text = objVendor.LastName;
                            ctlLogo.Url = objVendor.LogoFile;
                            addresssVendor.Unit = objVendor.Unit;
                            addresssVendor.Street = objVendor.Street;
                            addresssVendor.City = objVendor.City;
                            addresssVendor.Region = objVendor.Region;
                            addresssVendor.Country = objVendor.Country;
                            addresssVendor.Postal = objVendor.PostalCode;
                            addresssVendor.Telephone = objVendor.Telephone;
                            addresssVendor.Fax = objVendor.Fax;
                            addresssVendor.Cell = objVendor.Cell;
                            txtEmail.Text = objVendor.Email;
                            txtWebsite.Text = objVendor.Website;
                            chkAuthorized.Checked = objVendor.Authorized;
                            txtKeyWords.Text = objVendor.KeyWords;
                            ctlAudit.CreatedByUser = objVendor.CreatedByUser;
                            ctlAudit.CreatedDate = objVendor.CreatedDate.ToString();
                        }

                        var banners = ControlUtilities.LoadControl<Banners>(this, TemplateSourceDirectory.Remove(0, Globals.ApplicationPath.Length) + "/Banners.ascx");
                        banners.ID = "/Banners.ascx";
                        banners.VendorID = this.VendorID;
                        banners.ModuleConfiguration = ModuleConfiguration;
                        divBanners.Controls.Add(banners);

                        var affiliates = ControlUtilities.LoadControl<Affiliates>(this, TemplateSourceDirectory.Remove(0, Globals.ApplicationPath.Length) + "/Affiliates.ascx");
                        affiliates.ID = "/Affiliates.ascx";
                        affiliates.VendorID = this.VendorID;
                        affiliates.ModuleConfiguration = ModuleConfiguration;
                        divAffiliates.Controls.Add(affiliates);
                    }
                    else
                    {
                        chkAuthorized.Checked = true;
                        ctlAudit.Visible = false;
                        cmdDelete.Visible = false;
                        pnlBanners.Visible = false;
                        pnlAffiliates.Visible = false;
                    }
                    if (blnSignup || blnBanner)
                    {
                        rowVendor1.Visible = false;
                        rowVendor2.Visible = false;
                        pnlVendor.Visible = false;
                        cmdDelete.Visible = false;
                        ctlAudit.Visible = false;
                        if (blnBanner)
                        {
                            cmdUpdate.Visible = false;
                        }
                        else
                        {
                            cmdUpdate.Text = "Signup";
                        }
                    }
                    else
                    {
                        if (PortalSettings.ActiveTab.ParentId == PortalSettings.SuperTabId)
                        {
                            objTab = objTabs.GetTabByName("Vendors", Null.NullInteger);
                        }
                        else
                        {
                            objTab = objTabs.GetTabByName("Vendors", PortalId);
                        }
                        if (objTab != null)
                        {
                            ViewState["filter"] = Request.QueryString["filter"];
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                ReturnUrl(Convert.ToString(ViewState["filter"]));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (VendorID != -1)
                {
                    var objVendors = new VendorController();
                    objVendors.DeleteVendor(VendorID);
                }
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                int intPortalID;
                if (Page.IsValid)
                {
                    if (PortalSettings.ActiveTab.ParentId == PortalSettings.SuperTabId)
                    {
                        intPortalID = -1;
                    }
                    else
                    {
                        intPortalID = PortalId;
                    }
                    var objVendors = new VendorController();
                    var objVendor = new VendorInfo();
                    objVendor.PortalId = intPortalID;
                    objVendor.VendorId = VendorID;
                    objVendor.VendorName = txtVendorName.Text;
                    objVendor.Unit = addresssVendor.Unit;
                    objVendor.Street = addresssVendor.Street;
                    objVendor.City = addresssVendor.City;
                    objVendor.Region = addresssVendor.Region;
                    objVendor.Country = addresssVendor.Country;
                    objVendor.PostalCode = addresssVendor.Postal;
                    objVendor.Telephone = addresssVendor.Telephone;
                    objVendor.Fax = addresssVendor.Fax;
                    objVendor.Cell = addresssVendor.Cell;
                    objVendor.Email = txtEmail.Text;
                    objVendor.Website = txtWebsite.Text;
                    objVendor.FirstName = txtFirstName.Text;
                    objVendor.LastName = txtLastName.Text;
                    objVendor.UserName = UserInfo.UserID.ToString();
                    objVendor.LogoFile = ctlLogo.Url;
                    objVendor.KeyWords = txtKeyWords.Text;
                    objVendor.Authorized = chkAuthorized.Checked;
                    if (VendorID == -1)
                    {
                        try
                        {
                            VendorID = objVendors.AddVendor(objVendor);
                        }
                        catch
                        {
                            AddModuleMessage("ErrorAddVendor", ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                    }
                    else
                    {
                        var objVendorCheck = new VendorInfo();
                        objVendorCheck = objVendors.GetVendor(VendorID, intPortalID);
                        if (objVendorCheck != null)
                        {
                            objVendors.UpdateVendor(objVendor);
                        }
                        else
                        {
                            Response.Redirect(Globals.NavigateURL());
                        }
                    }
                    var objClassifications = new ClassificationController();
                    objClassifications.DeleteVendorClassifications(VendorID);
                    foreach (ListItem lstItem in lstClassifications.Items)
                    {
                        if (lstItem.Selected)
                        {
                            objClassifications.AddVendorClassification(VendorID, Int32.Parse(lstItem.Value));
                        }
                    }
                    if (cmdUpdate.Text == "Signup")
                    {
                        var Custom = new ArrayList();
                        Custom.Add(DateTime.Now.ToString());
                        Custom.Add(txtVendorName.Text);
                        Custom.Add(txtFirstName.Text);
                        Custom.Add(txtLastName.Text);
                        Custom.Add(addresssVendor.Unit);
                        Custom.Add(addresssVendor.Street);
                        Custom.Add(addresssVendor.City);
                        Custom.Add(addresssVendor.Region);
                        Custom.Add(addresssVendor.Country);
                        Custom.Add(addresssVendor.Postal);
                        Custom.Add(addresssVendor.Telephone);
                        Custom.Add(addresssVendor.Fax);
                        Custom.Add(addresssVendor.Cell);
                        Custom.Add(txtEmail.Text);
                        Custom.Add(txtWebsite.Text);
                        //send email to Admin
                        Mail.SendEmail(PortalSettings.Email,
                                       PortalSettings.Email,
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_ADMINISTRATOR_SUBJECT"),
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_ADMINISTRATOR_BODY", Localization.GlobalResourceFile, Custom));


                        //send email to vendor
                        Custom.Clear();
                        Custom.Add(txtFirstName.Text);
                        Custom.Add(txtLastName.Text);
                        Custom.Add(txtVendorName.Text);

                        Mail.SendEmail(PortalSettings.Email,
                                       txtEmail.Text,
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_SUBJECT"),
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_BODY", Localization.GlobalResourceFile, Custom));


                        ReturnUrl(txtVendorName.Text.Substring(0, 1));
                    }
                    else
                    {
                        ReturnUrl(Convert.ToString(ViewState["filter"]));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}