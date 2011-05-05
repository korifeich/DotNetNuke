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
using System.Text;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Links : SkinObjectBase
    {
        private string _alignment;
        private bool _forceLinks = true;
        private string _level;

        public string Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value.ToLower();
            }
        }

        public string CssClass { get; set; }

        public string Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value.ToLower();
            }
        }

        public string Separator { get; set; }

        public bool ShowDisabled { get; set; }

        public bool ForceLinks
        {
            get
            {
                return _forceLinks;
            }
            set
            {
                _forceLinks = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string strCssClass;
            if (!String.IsNullOrEmpty(CssClass))
            {
                strCssClass = CssClass;
            }
            else
            {
                strCssClass = "SkinObject";
            }
            string strSeparator = string.Empty;
            if (!String.IsNullOrEmpty(Separator))
            {
                if (Separator.IndexOf("src=") != -1)
                {
                    Separator = Regex.Replace(Separator, "src=[']?", "$&" + PortalSettings.ActiveTab.SkinPath);
                }
                Separator = string.Format("<span class=\"{0}\">{1}</span>", strCssClass, Separator);
            }
            else
            {
                Separator = " ";
            }
            string strLinks = "";
            strLinks = BuildLinks(Level, strSeparator, strCssClass);
            if (String.IsNullOrEmpty(strLinks) && ForceLinks)
            {
                strLinks = BuildLinks("", strSeparator, strCssClass);
            }
            lblLinks.Text = strLinks;
        }

        private string BuildLinks(string strLevel, string strSeparator, string strCssClass)
        {
            var sbLinks = new StringBuilder();
            List<TabInfo> portalTabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId);
            List<TabInfo> hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger);
            foreach (TabInfo objTab in portalTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }
            foreach (TabInfo objTab in hostTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }
            return sbLinks.ToString();
        }

        private string ProcessTab(TabInfo objTab, string strLevel, string strCssClass)
        {
            if (Navigation.CanShowTab(objTab, AdminMode, ShowDisabled))
            {
                switch (strLevel)
                {
                    case "same":
                    case "":
                        if (objTab.ParentId == PortalSettings.ActiveTab.ParentId)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                    case "child":
                        if (objTab.ParentId == PortalSettings.ActiveTab.TabID)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                    case "parent":
                        if (objTab.TabID == PortalSettings.ActiveTab.ParentId)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                    case "root":
                        if (objTab.Level == 0)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                }
            }
            return "";
        }

        private string ProcessLink(string sLink, int iLinksLength)
        {
            if (String.IsNullOrEmpty(sLink))
            {
                return "";
            }
            if (Alignment == "vertical")
            {
                sLink = string.Concat("<div>", Separator, sLink, "</div>");
            }
            else if (!String.IsNullOrEmpty(Separator) && iLinksLength > 0)
            {
                sLink = string.Concat(Separator, sLink);
            }
            return sLink;
        }

        private string AddLink(string strTabName, string strURL, string strCssClass)
        {
            return string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", strCssClass, strURL, strTabName);
        }
    }
}