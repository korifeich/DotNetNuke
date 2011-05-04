#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common.Utilities;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The EditHtml PortalModuleBase is used to manage Html
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditHtml : PortalModuleBase
    {
        private readonly HtmlTextController _htmlTextController = new HtmlTextController();
        private readonly HtmlTextLogController _htmlTextLogController = new HtmlTextLogController();
        private readonly WorkflowStateController _workflowStateController = new WorkflowStateController();

        private ModuleController _moduleController = new ModuleController();

        #region Nested type: WorkflowType

        private enum WorkflowType
        {
            DirectPublish = 1,
            ContentStaging = 2
        }

        #endregion

        #region "Private Properties"

        private int WorkflowID
        {
            get
            {
                int _workflowID = -1;

                if (ViewState["WorkflowID"] == null)
                {
                    _workflowID = _htmlTextController.GetWorkflow(ModuleId, TabId, PortalId).Value;
                    ViewState.Add("WorkflowID", _workflowID);
                }
                else
                {
                    _workflowID = int.Parse(ViewState["WorkflowID"].ToString());
                }

                return _workflowID;
            }
        }

        private int LockedByUserID
        {
            get
            {
                int _userID = -1;
                if ((Settings["Content_LockedBy"]) != null)
                {
                    _userID = int.Parse(Settings["Content_LockedBy"].ToString());
                }

                return _userID;
            }
            set
            {
                Settings["Content_LockedBy"] = value;
            }
        }

        private string TempContent
        {
            get
            {
                string _content = "";
                if ((ViewState["TempContent"] != null))
                {
                    _content = ViewState["TempContent"].ToString();
                }
                return _content;
            }
            set
            {
                ViewState["TempContent"] = value;
            }
        }

        private WorkflowType CurrentWorkflowType
        {
            get
            {
                WorkflowType _currentWorkflowType = default(WorkflowType);
                if (ViewState["_currentWorkflowType"] != null)
                {
                    _currentWorkflowType = (WorkflowType) Enum.Parse(typeof (WorkflowType), ViewState["_currentWorkflowType"].ToString());
                }

                return _currentWorkflowType;
            }
            set
            {
                ViewState["_currentWorkflowType"] = value;
            }
        }

        #endregion

        #region "Private Methods"

        /// <summary>
        ///   Displays the history of an html content item in a grid in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayHistory(HtmlTextInfo htmlContent)
        {
            tblHistory.Visible = !(CurrentWorkflowType == WorkflowType.DirectPublish);
            dshHistory.Visible = !(CurrentWorkflowType == WorkflowType.DirectPublish);

            if (((CurrentWorkflowType == WorkflowType.DirectPublish)))
            {
                return;
            }
            else
            {
                ArrayList htmlLogging = _htmlTextLogController.GetHtmlTextLog(htmlContent.ItemID);
                grdLog.DataSource = htmlLogging;
                grdLog.DataBind();

                tblHistory.Visible = !(htmlLogging.Count == 0);
                dshHistory.Visible = !(htmlLogging.Count == 0);
            }
        }

        /// <summary>
        ///   Displays the versions of the html content in the versions section
        /// </summary>
        private void DisplayVersions()
        {
            grdVersions.DataSource = _htmlTextController.GetAllHtmlText(ModuleId);
            grdVersions.DataBind();
        }

        /// <summary>
        ///   Displays the content of the master language if localized content is enabled.
        /// </summary>
        private void DisplayMasterLanguageContent()
        {
            //Get master language
            ModuleInfo objModule = new ModuleController().GetModule(ModuleId, TabId);
            if (objModule.DefaultLanguageModule != null)
            {
                HtmlTextInfo masterContent = _htmlTextController.GetTopHtmlText(objModule.DefaultLanguageModule.ModuleID, false, WorkflowID);
                if (masterContent != null)
                {
                    placeMasterContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(objModule.DefaultLanguageModule.ModuleID, FormatContent(masterContent.Content), Settings)));
                }
            }

            dshMaster.Visible = objModule.DefaultLanguageModule != null;
            tblMaster.Visible = objModule.DefaultLanguageModule != null;
        }

        /// <summary>
        ///   Displays the html content in the preview section.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayContent(HtmlTextInfo htmlContent)
        {
            lblCurrentWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblCurrentWorkflowState.Text = GetLocalizedString(htmlContent.StateName);
            lblCurrentVersion.Text = htmlContent.Version.ToString();
            txtContent.Text = FormatContent(htmlContent.Content);

            DisplayMasterLanguageContent();
        }

        /// <summary>
        ///   Displays the content preview in the preview section
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(HtmlTextInfo htmlContent)
        {
            lblPreviewVersion.Text = htmlContent.Version.ToString();
            lblPreviewWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblPreviewWorkflowState.Text = GetLocalizedString(htmlContent.StateName);
            litPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent.Content, Settings);
            DisplayHistory(htmlContent);
        }

        /// <summary>
        ///   Displays the preview in the preview section
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        private void DisplayPreview(string htmlContent)
        {
            litPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent, Settings);
            rowPreviewVersion.Visible = false;
            rowPreviewWorlflow.Visible = false;

            rowPreviewWorkflowState.Visible = true;
            lblPreviewWorkflowState.Text = GetLocalizedString("EditPreviewState");
            dshPreview.IsExpanded = true;
        }

        /// <summary>
        ///   Displays the content but hide the editor if editing is locked from the current user
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <param name = "lastPublishedContent">Last content of the published.</param>
        private void DisplayLockedContent(HtmlTextInfo htmlContent, HtmlTextInfo lastPublishedContent)
        {
            txtContent.Visible = false;
            cmdSave.Visible = false;
            cmdPreview.Visible = false;
            rowPublish.Visible = false;

            rowSubmittedContent.Visible = true;
            dshPreview.IsExpanded = true;

            lblCurrentWorkflowInUse.Text = GetLocalizedString(htmlContent.WorkflowName);
            lblCurrentWorkflowState.Text = GetLocalizedString(htmlContent.StateName);

            litCurrentContentPreview.Text = HtmlTextController.FormatHtmlText(ModuleId, htmlContent.Content, Settings);
            lblCurrentVersion.Text = htmlContent.Version.ToString();
            DisplayVersions();

            if ((lastPublishedContent != null))
            {
                DisplayPreview(lastPublishedContent);
                DisplayHistory(lastPublishedContent);
            }
            else
            {
                tblHistory.Visible = false;
                dshHistory.Visible = false;
                DisplayPreview(htmlContent.Content);
            }
        }

        /// <summary>
        ///   Displays the initial content when a module is first added to the page.
        /// </summary>
        /// <param name = "firstState">The first state.</param>
        private void DisplayInitialContent(WorkflowStateInfo firstState)
        {
            txtContent.Text = GetLocalizedString("AddContent");
            litPreview.Text = GetLocalizedString("AddContent");
            lblCurrentWorkflowInUse.Text = firstState.WorkflowName;
            lblPreviewWorkflowInUse.Text = firstState.WorkflowName;
            rowPreviewVersion.Visible = false;

            dshVersions.Visible = false;
            tblVersions.Visible = false;

            dshHistory.Visible = false;
            tblHistory.Visible = false;

            rowCurrentWorkflowState.Visible = false;
            rowCurrentVersion.Visible = false;
            rowPreviewWorkflowState.Visible = false;

            lblPreviewWorkflowState.Text = firstState.StateName;
        }

        #endregion

        #region "Private Functions"

        /// <summary>
        ///   Formats the content to make it html safe.
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private string FormatContent(string htmlContent)
        {
            string strContent = HttpUtility.HtmlDecode(htmlContent);
            strContent = HtmlTextController.ManageRelativePaths(strContent, PortalSettings.HomeDirectory, "src", PortalId);
            strContent = HtmlTextController.ManageRelativePaths(strContent, PortalSettings.HomeDirectory, "background", PortalId);
            return HttpUtility.HtmlEncode(strContent);
        }

        /// <summary>
        ///   Gets the localized string from a resource file if it exists.
        /// </summary>
        /// <param name = "str">The STR.</param>
        /// <returns></returns>
        private string GetLocalizedString(string str)
        {
            string localizedString = Localization.GetString(str, LocalResourceFile);
            return (string.IsNullOrEmpty(localizedString) ? str : localizedString);
        }

        /// <summary>
        ///   Gets the latest html content of the module
        /// </summary>
        /// <returns></returns>
        private HtmlTextInfo GetLatestHTMLContent()
        {
            HtmlTextInfo htmlContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);
            if (htmlContent == null)
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
                htmlContent.StateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                htmlContent.WorkflowID = WorkflowID;
                htmlContent.ModuleID = ModuleId;
            }

            return htmlContent;
        }

        /// <summary>
        ///   Returns whether or not the user has review permissions to this module
        /// </summary>
        /// <param name = "htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private bool UserCanReview(HtmlTextInfo htmlContent)
        {
            if ((htmlContent != null))
            {
                return WorkflowStatePermissionController.HasWorkflowStatePermission(WorkflowStatePermissionController.GetWorkflowStatePermissions(htmlContent.StateID), "REVIEW");
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///   Gets the last published version of this module
        /// </summary>
        /// <param name = "publishedStateID">The published state ID.</param>
        /// <returns></returns>
        private HtmlTextInfo GetLastPublishedVersion(int publishedStateID)
        {
            return (from version in _htmlTextController.GetAllHtmlText(ModuleId) where version.StateID == publishedStateID orderby version.Version descending select version).ToList()[0];
        }

        #endregion

        #region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdPreview.Click += cmdPreview_Click;
            cmdSave.Click += cmdSave_Click;
            grdLog.ItemDataBound += grdLog_ItemDataBound;
            grdVersions.ItemCommand += grdVersions_ItemCommand;
            grdVersions.ItemDataBound += grdVersions_ItemDataBound;
            grdVersions.PageIndexChanged += grdVersions_PageIndexChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                int htmlContentItemID = -1;
                HtmlTextInfo htmlContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);

                if ((htmlContent != null))
                {
                    htmlContentItemID = htmlContent.ItemID;
                }

                if (!Page.IsPostBack)
                {
                    ArrayList WorkflowStates = _workflowStateController.GetWorkflowStates(WorkflowID);
                    int MaxVersions = _htmlTextController.GetMaximumVersionHistory(PortalId);
                    bool UserCanEdit = UserInfo.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) || UserInfo.UserID == LockedByUserID;

                    lblMaxVersions.Text = MaxVersions.ToString();
                    grdVersions.PageSize = (MaxVersions < 10 ? MaxVersions : 10);

                    switch (WorkflowStates.Count)
                    {
                        case 1:
                            CurrentWorkflowType = WorkflowType.DirectPublish;
                            break;
                        case 2:
                            CurrentWorkflowType = WorkflowType.ContentStaging;
                            break;
                    }

                    if (htmlContentItemID != -1)
                    {
                        DisplayContent(htmlContent);
                        DisplayPreview(htmlContent);
                    }
                    else
                    {
                        DisplayInitialContent(WorkflowStates[0] as WorkflowStateInfo);
                    }

                    rowPublish.Visible = !(CurrentWorkflowType == WorkflowType.DirectPublish);
                    DisplayVersions();
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
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            bool redirect = true;

            try
            {
                // get content
                HtmlTextInfo htmlContent = GetLatestHTMLContent();

                PortalAliasController pac = new PortalAliasController();
                var aliases = from PortalAliasInfo pa in pac.GetPortalAliasByPortalID(PortalSettings.PortalId).Values select pa.HTTPAlias;
                htmlContent.Content = HtmlUtils.AbsoluteToRelativeUrls(txtContent.Text, aliases);


                int draftStateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                int nextWorkflowStateID = _workflowStateController.GetNextWorkflowStateID(WorkflowID, htmlContent.StateID);
                int publishedStateID = _workflowStateController.GetLastWorkflowStateID(WorkflowID);

                bool UserCanUpdate = UserInfo.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) || UserInfo.UserID == LockedByUserID;

                switch (CurrentWorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));

                        break;
                    case WorkflowType.ContentStaging:
                        if (chkPublish.Checked)
                        {
                            //if it's already published set it to draft
                            if (htmlContent.StateID == publishedStateID)
                            {
                                htmlContent.StateID = draftStateID;
                            }
                            else
                            {
                                htmlContent.StateID = publishedStateID;
                                //here it's in published mode
                            }
                        }
                        else
                        {
                            //if it's already published set it back to draft
                            if ((htmlContent.StateID != draftStateID))
                            {
                                htmlContent.StateID = draftStateID;
                            }
                        }

                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(Page, "Error occurred: ", exc.Message, ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // redirect back to portal
            if (redirect)
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
        }

        private void cmdPreview_Click(object sender, EventArgs e)
        {
            try
            {
                DisplayPreview(txtContent.Text);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void grdLog_ItemDataBound(object sender, GridItemEventArgs e)
        {
            GridItem item = e.Item;


            if (item.ItemType == GridItemType.Item || item.ItemType == GridItemType.AlternatingItem || item.ItemType == GridItemType.SelectedItem)
            {
                //Localize columns
                item.Cells[2].Text = Localization.GetString(item.Cells[2].Text, LocalResourceFile);
                item.Cells[3].Text = Localization.GetString(item.Cells[3].Text, LocalResourceFile);
            }
        }

        private void grdVersions_ItemCommand(object source, GridCommandEventArgs e)
        {
            try
            {
                HtmlTextInfo htmlContent = _htmlTextController.GetHtmlText(ModuleId, int.Parse(e.CommandArgument.ToString()));

                switch (e.CommandName.ToLower())
                {
                    case "remove":
                        _htmlTextController.DeleteHtmlText(ModuleId, htmlContent.ItemID);
                        break;
                    case "rollback":
                        htmlContent.ItemID = -1;
                        htmlContent.ModuleID = ModuleId;
                        htmlContent.WorkflowID = WorkflowID;
                        htmlContent.StateID = _workflowStateController.GetFirstWorkflowStateID(WorkflowID);
                        _htmlTextController.UpdateHtmlText(htmlContent, _htmlTextController.GetMaximumVersionHistory(PortalId));
                        break;
                    case "preview":
                        DisplayPreview(htmlContent);
                        dshHistory.IsExpanded = true;
                        dshPreview.IsExpanded = true;
                        break;
                }

                if ((e.CommandName.ToLower() != "preview"))
                {
                    HtmlTextInfo latestContent = _htmlTextController.GetTopHtmlText(ModuleId, false, WorkflowID);
                    if (latestContent == null)
                    {
                        DisplayInitialContent(_workflowStateController.GetWorkflowStates(WorkflowID)[0] as WorkflowStateInfo);
                    }
                    else
                    {
                        DisplayContent(latestContent);
                        DisplayPreview(latestContent);
                        DisplayVersions();
                    }
                }

                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void grdVersions_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if ((e.Item.ItemType == GridItemType.Item || e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.SelectedItem))
            {
                var item = e.Item as GridDataItem;
                var htmlContent = item.DataItem as HtmlTextInfo;

                string createdBy = "Default";
                if ((htmlContent.CreatedByUserID != -1))
                {
                    UserInfo createdByByUser = UserController.GetUserById(PortalId, htmlContent.CreatedByUserID);
                    if (createdByByUser != null)
                    {
                        createdBy = createdByByUser.DisplayName;
                    }                    
                }

                foreach (TableCell cell in item.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is ImageButton)
                        {
                            var _imageButton = cellControl as ImageButton;
                            _imageButton.CommandArgument = htmlContent.ItemID.ToString();
                            switch (_imageButton.CommandName.ToLower())
                            {
                                case "rollback":
                                    //hide rollback for the first item
                                    if (grdVersions.CurrentPageIndex == 0)
                                    {
                                        if ((item.ItemIndex == 0))
                                        {
                                            _imageButton.Visible = false;
                                            break;
                                        }
                                    }

                                    _imageButton.Visible = true;

                                    break;
                                case "remove":
                                    string msg = GetLocalizedString("DeleteVersion.Confirm");
                                    msg =
                                        msg.Replace("[VERSION]", htmlContent.Version.ToString()).Replace("[STATE]", htmlContent.StateName).Replace("[DATECREATED]", htmlContent.CreatedOnDate.ToString())
                                            .Replace("[USERNAME]", createdBy);
                                    _imageButton.OnClientClick = "return confirm(\"" + msg + "\");";
                                    //hide the delete button
                                    bool showDelete = UserInfo.IsSuperUser || PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);

                                    if (!showDelete)
                                    {
                                        showDelete = htmlContent.IsPublished == false;
                                    }

                                    _imageButton.Visible = showDelete;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void grdVersions_PageIndexChanged(object source, GridPageChangedEventArgs e)
        {
            DisplayVersions();
        }

        #endregion
    }
}