﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content;
using System.Xml;
using DotNetNuke.ComponentModel;
using DotNetNuke.Common.Utilities;
using System.Data;

namespace DotNetNuke.Services.Journal {
    public class JournalController {
        private readonly IJournalDataService _DataService;
        #region "Constructors"
        public JournalController() : this(GetDataService()) {
        }
        public JournalController(IJournalDataService dataService) {
            _DataService = dataService;
        }

        #endregion
        #region "Private Shared Methods"

        private static IJournalDataService GetDataService() {
            var ds = ComponentFactory.GetComponent<IJournalDataService>();

            if (ds == null) {
                ds = new JournalDataService();
                ComponentFactory.RegisterComponentInstance<IJournalDataService>(ds);
            }
            return ds;
        }

        #endregion
        #region "Public Methods"
        public List<JournalItem> ListForProfile(int PortalId, int CurrentUserId, int ProfileId, int RowIndex, int MaxRows) {
            return CBO.FillCollection<JournalItem>(_DataService.Journal_ListForProfile(PortalId, CurrentUserId, ProfileId, RowIndex, MaxRows));
        }
        public JournalItem Journal_Get(int PortalId, int CurrentUserId, int JournalId) {
            return (JournalItem)CBO.FillObject(_DataService.Journal_Get(PortalId, CurrentUserId, JournalId), typeof(JournalItem));
        }
        public JournalItem Journal_Save(JournalItem objJournalItem, int TabId) {
            JournalDataService jds = new JournalDataService();
            string xml = null;
            if (!String.IsNullOrEmpty(objJournalItem.Body)) {
                System.Xml.XmlDocument xDoc = new XmlDocument();
                XmlElement xnode = xDoc.CreateElement("items");
                XmlElement xnode2 = xDoc.CreateElement("item");
                XmlAttribute xattrib = xDoc.CreateAttribute("itemkey");
                xattrib.Value = "1234";
                xnode2.Attributes.Append(xattrib);

                xnode2.AppendChild(CreateElement(xDoc, "id", "-1"));
                xnode2.AppendChild(CreateCDataElement(xDoc, "body", objJournalItem.Body));
                xnode2.AppendChild(CreateElement(xDoc, "likes", string.Empty));
                xnode.AppendChild(xnode2);
                xDoc.AppendChild(xnode);

                XmlDeclaration xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
                xDec.Encoding = "UTF-16";
                xDec.Standalone = "yes";
                XmlElement root = xDoc.DocumentElement;
                xDoc.InsertBefore(xDec, root);
                objJournalItem.JournalXML = xDoc;
                xml = objJournalItem.JournalXML.OuterXml;
               
            }
            string journalData = string.Empty;
            journalData = objJournalItem.ItemData.ToJson();
            objJournalItem.JournalId = jds.Journal_Save(objJournalItem.PortalId, objJournalItem.UserId, objJournalItem.ProfileId, objJournalItem.SocialGroupId,
                    objJournalItem.JournalId, objJournalItem.JournalTypeId, objJournalItem.Title, objJournalItem.Summary, objJournalItem.Body, journalData, xml, objJournalItem.ObjectKey, objJournalItem.AccessKey);

            objJournalItem = Journal_Get(objJournalItem.PortalId, objJournalItem.UserId, objJournalItem.JournalId);
            Content cnt = new Content();

            if (objJournalItem.ContentItemId > 0) {
                cnt.UpdateContentItem(objJournalItem, TabId);
            } else {
                ContentItem ci = new ContentItem();
                ci = cnt.CreateContentItem(objJournalItem, TabId);
                jds.Journal_UpdateContentItemId(objJournalItem.JournalId, ci.ContentItemId);
            }
            return objJournalItem;
        }
        public void Journal_Delete(int JournalId) {
            JournalDataService jds = new JournalDataService();
            jds.Journal_Delete(JournalId);

        }
        public void Journal_Like(int JournalId, int UserId, string DisplayName) {
            JournalDataService jds = new JournalDataService();
            jds.Journal_Like(JournalId, UserId, DisplayName);

        }
        public List<object> Journal_LikeList(int PortalId, int JournalId) {
            JournalDataService jds = new JournalDataService();
            List<object> list = new List<object>();
            using (IDataReader dr = jds.Journal_LikeList(PortalId, JournalId)) {
                while (dr.Read()) {
                    list.Add(new { userId = dr["UserId"].ToString(), name = dr["DisplayName"].ToString() });
                }
                dr.Close();
            }
            return list;
        }
        #endregion
        
       
        private XmlElement CreateElement(XmlDocument xDoc, string name, string value) {
            XmlElement xnode = xDoc.CreateElement(name);
            XmlText xtext = xDoc.CreateTextNode(value);
            xnode.AppendChild(xtext);
            return xnode;
        }
        private XmlElement CreateCDataElement(XmlDocument xDoc, string name, string value) {
            XmlElement xnode = xDoc.CreateElement(name);
            XmlCDataSection xdata = xDoc.CreateCDataSection(value);
            xnode.AppendChild(xdata);
            return xnode;

        }
    }
}