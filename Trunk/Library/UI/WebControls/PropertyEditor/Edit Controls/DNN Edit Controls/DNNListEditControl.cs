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
using System.Web.UI;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNListEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNListEditControl control provides a standard UI component for selecting
    /// from Lists
    /// </summary>
    /// <history>
    ///     [cnurse]	05/04/2006	created
    /// </history>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNListEditControl runat=server></{0}:DNNListEditControl>")]
    public class DNNListEditControl : EditControl, IPostBackEventHandler
    {
        private ListEntryInfoCollection _List;
        private string _ListName = Null.NullString;
        private string _ParentKey = Null.NullString;
        private ListBoundField _TextField = ListBoundField.Text;
        private ListBoundField _ValueField = ListBoundField.Value;

        protected bool AutoPostBack { get; set; }

        protected int IntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
                    intValue = Convert.ToInt32(Value);
                }
                catch (Exception exc)
                {
                    DnnLog.Error(exc);

                }
                return intValue;
            }
        }

        protected ListEntryInfoCollection List
        {
            get
            {
                if (_List == null)
                {
                    var objListController = new ListController();
                    _List = objListController.GetListEntryInfoCollection(ListName, ParentKey, PortalId);
                }
                return _List;
            }
        }

        protected virtual string ListName
        {
            get
            {
                if (_ListName == Null.NullString)
                {
                    _ListName = Name;
                }
                return _ListName;
            }
            set
            {
                _ListName = value;
            }
        }

        protected int OldIntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
                    intValue = Convert.ToInt32(OldValue);
                }
                catch (Exception exc)
                {
                    DnnLog.Error(exc);

                }
                return intValue;
            }
        }

        protected virtual string ParentKey
        {
            get
            {
                return _ParentKey;
            }
            set
            {
                _ParentKey = value;
            }
        }

        protected int PortalId
        {
            get
            {
                return PortalSettings.Current.PortalId;
            }
        }

        protected virtual ListBoundField TextField
        {
            get
            {
                return _TextField;
            }
            set
            {
                _TextField = value;
            }
        }

        protected virtual ListBoundField ValueField
        {
            get
            {
                return _ValueField;
            }
            set
            {
                _ValueField = value;
            }
        }

        protected string OldStringValue
        {
            get
            {
                return Convert.ToString(OldValue);
            }
        }

        protected override string StringValue
        {
            get
            {
                return Convert.ToString(Value);
            }
            set
            {
                if (ValueField == ListBoundField.Id)
                {
                    Value = Int32.Parse(value);
                }
                else
                {
                    Value = value;
                }
            }
        }

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            if (AutoPostBack)
            {
                OnItemChanged(GetEventArgs());
            }
        }

        #endregion

        public event PropertyChangedEventHandler ItemChanged;

        private PropertyEditorEventArgs GetEventArgs()
        {
            var args = new PropertyEditorEventArgs(Name);
            if (ValueField == ListBoundField.Id)
            {
                args.Value = IntegerValue;
                args.OldValue = OldIntegerValue;
            }
            else
            {
                args.Value = StringValue;
                args.OldValue = OldStringValue;
            }
            args.StringValue = StringValue;
            return args;
        }

        protected override void OnAttributesChanged()
        {
            if ((CustomAttributes != null))
            {
                foreach (Attribute attribute in CustomAttributes)
                {
                    if (attribute is ListAttribute)
                    {
                        var listAtt = (ListAttribute) attribute;
                        ListName = listAtt.ListName;
                        ParentKey = listAtt.ParentKey;
                        TextField = listAtt.TextField;
                        ValueField = listAtt.ValueField;
                        break;
                    }
                }
            }
        }

        protected override void OnDataChanged(EventArgs e)
        {
            base.OnValueChanged(GetEventArgs());
        }

        protected virtual void OnItemChanged(PropertyEditorEventArgs e)
        {
            if (ItemChanged != null)
            {
                ItemChanged(this, e);
            }
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            var objListController = new ListController();
            ListEntryInfo entry = null;
            string entryText = Null.NullString;
            switch (ValueField)
            {
                case ListBoundField.Id:
                    entry = objListController.GetListEntryInfo(Convert.ToInt32(Value));
                    break;
                case ListBoundField.Text:
                    entryText = StringValue;
                    break;
                case ListBoundField.Value:
                    entry = objListController.GetListEntryInfo(ListName, StringValue);
                    break;
            }
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (entry != null)
            {
                switch (TextField)
                {
                    case ListBoundField.Id:
                        writer.Write(entry.EntryID.ToString());
                        break;
                    case ListBoundField.Text:
                        writer.Write(entry.Text);
                        break;
                    case ListBoundField.Value:
                        writer.Write(entry.Value);
                        break;
                }
            }
            else
            {
                writer.Write(entryText);
            }
            writer.RenderEndTag();
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            if (AutoPostBack)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, Page.ClientScript.GetPostBackEventReference(this, ID));
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            if (ValueField == ListBoundField.Text)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }
            if (StringValue == Null.NullString)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">");
            writer.RenderEndTag();
            for (int I = 0; I <= List.Count - 1; I++)
            {
                ListEntryInfo item = List[I];
                string itemValue = Null.NullString;
                switch (ValueField)
                {
                    case ListBoundField.Id:
                        itemValue = item.EntryID.ToString();
                        break;
                    case ListBoundField.Text:
                        itemValue = item.Text;
                        break;
                    case ListBoundField.Value:
                        itemValue = item.Value;
                        break;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Value, itemValue);
                if (StringValue == itemValue)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                switch (TextField)
                {
                    case ListBoundField.Id:
                        writer.Write(item.EntryID.ToString());
                        break;
                    case ListBoundField.Text:
                        writer.Write(item.Text);
                        break;
                    case ListBoundField.Value:
                        writer.Write(item.Value);
                        break;
                }
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }
    }
}