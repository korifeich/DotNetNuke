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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Portals
{
	/// <summary>
	/// PortalAliasController provides method to manage portal alias.
	/// </summary>
	/// <remarks>
	/// For DotNetNuke to know what site a request should load, it uses a system of portal aliases. 
	/// When a request is recieved by DotNetNuke from IIS, it extracts the domain name portion and does a comparison against 
	/// the list of portal aliases and then redirects to the relevant portal to load the approriate page. 
	/// </remarks>
    public class PortalAliasController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPortalAliasLookupCallBack gets a Dictionary of Host Settings from
        /// the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call</param>
        /// <history>
        /// 	[cnurse]	07/15/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static object GetPortalAliasLookupCallBack(CacheItemArgs cacheItemArgs)
        {
            return new PortalAliasController().GetPortalAliases();
        }

		/// <summary>
		/// Gets the portal alias by portal.
		/// </summary>
		/// <param name="portalId">The portal id.</param>
		/// <param name="portalAlias">The portal alias.</param>
		/// <returns>Portal alias.</returns>
        public static string GetPortalAliasByPortal(int portalId, string portalAlias)
        {
            string retValue = "";
            PortalAliasCollection portalAliasCollection = GetPortalAliasLookup();
            string httpAlias;
            bool foundAlias = false;

            PortalAliasInfo objPortalAliasInfo = portalAliasCollection[portalAlias.ToLower()];
            if (objPortalAliasInfo != null)
            {
                if (objPortalAliasInfo.PortalID == portalId)
                {
                    retValue = objPortalAliasInfo.HTTPAlias;
                    foundAlias = true;
                }
            }


            if (!foundAlias)
            {
                //searching from longest to shortest alias ensures that the most specific portal is matched first
                //In some cases this method has been called with "portalaliases" that were not exactly the real portal alias
                //the startswith behaviour is preserved here to support those non-specific uses
                IEnumerable<String> aliases = portalAliasCollection.Keys.Cast<String>().OrderByDescending(k => k.Length);
                foreach (var currentAlias in aliases)
                {
                    // check if the alias key starts with the portal alias value passed in - we use
                    // StartsWith because child portals are redirected to the parent portal domain name
                    // eg. child = 'www.domain.com/child' and parent is 'www.domain.com'
                    // this allows the parent domain name to resolve to the child alias ( the tabid still identifies the child portalid )
                    objPortalAliasInfo = portalAliasCollection[currentAlias];
                    httpAlias = objPortalAliasInfo.HTTPAlias.ToLower();
                    if (httpAlias.StartsWith(portalAlias.ToLower()) && objPortalAliasInfo.PortalID == portalId)
                    {
                        retValue = objPortalAliasInfo.HTTPAlias;
                        break;
                    }
                    if (httpAlias.StartsWith("www."))
                    {
                        httpAlias = httpAlias.Replace("www.", "");
                    }
                    else
                    {
                        httpAlias = string.Concat("www.", httpAlias);
                    }
                    if (httpAlias.StartsWith(portalAlias.ToLower()) && objPortalAliasInfo.PortalID == portalId)
                    {
                        retValue = objPortalAliasInfo.HTTPAlias;
                        break;
                    }
                }
            }
            return retValue;
        }

		/// <summary>
		/// Gets the portal alias by tab.
		/// </summary>
		/// <param name="TabID">The tab ID.</param>
		/// <param name="PortalAlias">The portal alias.</param>
		/// <returns>Portal alias.</returns>
        public static string GetPortalAliasByTab(int TabID, string PortalAlias)
        {
            string retValue = Null.NullString;
            int intPortalId = -2;
            var objTabs = new TabController();
            TabInfo objTab = objTabs.GetTab(TabID, Null.NullInteger, false);
            if (objTab != null)
            {
                if (!objTab.IsDeleted)
                {
                    intPortalId = objTab.PortalID;
                }
            }
            switch (intPortalId)
            {
                case -2:
                    break;
                case -1:
                    retValue = PortalAlias;
                    break;
                default:
                    retValue = GetPortalAliasByPortal(intPortalId, PortalAlias);
                    break;
            }
            return retValue;
        }

		/// <summary>
		/// Gets the portal alias info.
		/// </summary>
		/// <param name="PortalAlias">The portal alias.</param>
		/// <returns>Portal alias info</returns>
        public static PortalAliasInfo GetPortalAliasInfo(string PortalAlias)
        {
            string strPortalAlias;
            PortalAliasInfo objPortalAliasInfo = GetPortalAliasLookup(PortalAlias.ToLower());
            if (objPortalAliasInfo == null)
            {
                if (PortalAlias.ToLower().StartsWith("www."))
                {
                    strPortalAlias = PortalAlias.Replace("www.", "");
                }
                else
                {
                    strPortalAlias = string.Concat("www.", PortalAlias);
                }
                objPortalAliasInfo = GetPortalAliasLookup(strPortalAlias.ToLower());
            }
            if (objPortalAliasInfo == null)
            {
                if (PortalAlias.IndexOf(".") != -1)
                {
                    strPortalAlias = PortalAlias.Substring(PortalAlias.IndexOf(".") + 1);
                }
                else
                {
                    strPortalAlias = PortalAlias;
                }
                if (objPortalAliasInfo == null)
                {
                    objPortalAliasInfo = GetPortalAliasLookup("*." + strPortalAlias.ToLower());
                }
                if (objPortalAliasInfo == null)
                {
                    objPortalAliasInfo = GetPortalAliasLookup(strPortalAlias.ToLower());
                }
                if (objPortalAliasInfo == null)
                {
                    objPortalAliasInfo = GetPortalAliasLookup("www." + strPortalAlias.ToLower());
                }
            }
            if (objPortalAliasInfo == null)
            {
                PortalAliasCollection objPortalAliasCollection = GetPortalAliasLookup();
                if (!objPortalAliasCollection.HasKeys || (objPortalAliasCollection.Count == 1 && objPortalAliasCollection.Contains("_default")))
                {
                    DataProvider.Instance().UpdatePortalAlias(PortalAlias.ToLower().Trim('/'), UserController.GetCurrentUserInfo().UserID);
                    var objEventLog = new EventLogController();
                    objEventLog.AddLog("PortalAlias",
                                       PortalAlias,
                                       PortalController.GetCurrentPortalSettings(),
                                       UserController.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.PORTALALIAS_UPDATED);
                    DataCache.RemoveCache("GetPortalByAlias");
                    objPortalAliasInfo = GetPortalAliasLookup(PortalAlias.ToLower());
                }
            }
            return objPortalAliasInfo;
        }

		/// <summary>
		/// Gets the portal alias lookup.
		/// </summary>
		/// <returns>Portal Alias Collection</returns>
        public static PortalAliasCollection GetPortalAliasLookup()
        {
            return CBO.GetCachedObject<PortalAliasCollection>(new CacheItemArgs(DataCache.PortalAliasCacheKey, DataCache.PortalAliasCacheTimeOut, DataCache.PortalAliasCachePriority),
                                                              GetPortalAliasLookupCallBack,
                                                              true);
        }

		/// <summary>
		/// Gets the portal alias lookup.
		/// </summary>
		/// <param name="aliasInfo">The alias info.</param>
		/// <returns>Porta lAlias Info</returns>
        public static PortalAliasInfo GetPortalAliasLookup(string aliasInfo)
        {
            return GetPortalAliasLookup()[aliasInfo];
        }

		/// <summary>
		/// Validates the alias.
		/// </summary>
		/// <param name="portalAlias">The portal alias.</param>
		/// <param name="ischild">if set to <c>true</c> [ischild].</param>
		/// <returns><c>true</c> if the alias is a valid url format; otherwise return <c>false</c>.</returns>
        public static bool ValidateAlias(string portalAlias, bool ischild)
        {
            bool isValid = true;

            string validChars = "abcdefghijklmnopqrstuvwxyz0123456789-_";
            if (!ischild)
            {
                validChars += "./:";
            }

            foreach (char c in portalAlias)
            {
                if (!validChars.Contains(c.ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }


		/// <summary>
		/// Adds the portal alias.
		/// </summary>
		/// <param name="objPortalAliasInfo">The obj portal alias info.</param>
		/// <returns>Portal alias id.</returns>
        public int AddPortalAlias(PortalAliasInfo objPortalAliasInfo)
        {
            int Id = DataProvider.Instance().AddPortalAlias(objPortalAliasInfo.PortalID, objPortalAliasInfo.HTTPAlias.ToLower().Trim('/'), UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objPortalAliasInfo, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PORTALALIAS_CREATED);
            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
            return Id;
        }

		/// <summary>
		/// Deletes the portal alias.
		/// </summary>
		/// <param name="PortalAliasID">The portal alias ID.</param>
        public void DeletePortalAlias(int PortalAliasID)
        {
            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
            DataProvider.Instance().DeletePortalAlias(PortalAliasID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog("PortalAliasID",
                               PortalAliasID.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALALIAS_DELETED);
        }

		/// <summary>
		/// Gets the portal alias.
		/// </summary>
		/// <param name="PortalAlias">The portal alias.</param>
		/// <param name="PortalID">The portal ID.</param>
		/// <returns>Portal Alias Info.</returns>
        public PortalAliasInfo GetPortalAlias(string PortalAlias, int PortalID)
        {
            return (PortalAliasInfo) CBO.FillObject(DataProvider.Instance().GetPortalAlias(PortalAlias, PortalID), typeof (PortalAliasInfo));
        }

		/// <summary>
		/// Gets the portal alias array by portal ID.
		/// </summary>
		/// <param name="PortalID">The portal ID.</param>
		/// <returns>Portal alias list.</returns>
        public ArrayList GetPortalAliasArrayByPortalID(int PortalID)
        {
            IDataReader dr = DataProvider.Instance().GetPortalAliasByPortalID(PortalID);
            try
            {
                var arr = new ArrayList();
                while (dr.Read())
                {
                    var objPortalAliasInfo = new PortalAliasInfo();
                    objPortalAliasInfo.PortalAliasID = Convert.ToInt32(dr["PortalAliasID"]);
                    objPortalAliasInfo.PortalID = Convert.ToInt32(dr["PortalID"]);
                    objPortalAliasInfo.HTTPAlias = Convert.ToString(dr["HTTPAlias"]).ToLower();
                    arr.Add(objPortalAliasInfo);
                }
                return arr;
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
        }

		/// <summary>
		/// Gets the portal alias by portal alias ID.
		/// </summary>
		/// <param name="PortalAliasID">The portal alias ID.</param>
		/// <returns>Portal alias info.</returns>
        public PortalAliasInfo GetPortalAliasByPortalAliasID(int PortalAliasID)
        {
            return (PortalAliasInfo) CBO.FillObject((DataProvider.Instance().GetPortalAliasByPortalAliasID(PortalAliasID)), typeof (PortalAliasInfo));
        }

		/// <summary>
		/// Gets the portal alias by portal ID.
		/// </summary>
		/// <param name="PortalID">The portal ID.</param>
		/// <returns>Portal alias collection.</returns>
        public PortalAliasCollection GetPortalAliasByPortalID(int PortalID)
        {
            IDataReader dr = DataProvider.Instance().GetPortalAliasByPortalID(PortalID);
            try
            {
                var objPortalAliasCollection = new PortalAliasCollection();
                while (dr.Read())
                {
                    var objPortalAliasInfo = new PortalAliasInfo();
                    objPortalAliasInfo.PortalAliasID = Convert.ToInt32(dr["PortalAliasID"]);
                    objPortalAliasInfo.PortalID = Convert.ToInt32(dr["PortalID"]);
                    objPortalAliasInfo.HTTPAlias = Convert.ToString(dr["HTTPAlias"]);
                    objPortalAliasCollection.Add(Convert.ToString(dr["HTTPAlias"]).ToLower(), objPortalAliasInfo);
                }
                return objPortalAliasCollection;
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
        }

		/// <summary>
		/// Gets the portal aliases.
		/// </summary>
		/// <returns>Portal alias collection.</returns>
        public PortalAliasCollection GetPortalAliases()
        {
            return GetPortalAliasByPortalID(-1);
        }

		/// <summary>
		/// Gets the portal by portal alias ID.
		/// </summary>
		/// <param name="PortalAliasId">The portal alias id.</param>
		/// <returns>Portal info.</returns>
        public PortalInfo GetPortalByPortalAliasID(int PortalAliasId)
        {
            return (PortalInfo) CBO.FillObject(DataProvider.Instance().GetPortalByPortalAliasID(PortalAliasId), typeof (PortalInfo));
        }

		/// <summary>
		/// Updates the portal alias info.
		/// </summary>
		/// <param name="objPortalAliasInfo">The obj portal alias info.</param>
        public void UpdatePortalAliasInfo(PortalAliasInfo objPortalAliasInfo)
        {
            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
            DataProvider.Instance().UpdatePortalAliasInfo(objPortalAliasInfo.PortalAliasID,
                                                          objPortalAliasInfo.PortalID,
                                                          objPortalAliasInfo.HTTPAlias.ToLower().Trim('/'),
                                                          UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(objPortalAliasInfo, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PORTALALIAS_UPDATED);
        }
    }
}