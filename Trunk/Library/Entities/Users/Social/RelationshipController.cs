﻿#region Copyright

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
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Social.Data;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Users
{
	/// <summary>
	/// Business Layer to manage Relationships. Also contains CRUD methods.
	/// </summary>
    public class RelationshipController : IRelationshipController
	{
        private readonly IDataService _dataService;

        #region Constructors

        public RelationshipController() : this(DataService.Instance)
        {
        }

        public RelationshipController(IDataService dataService)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);

            _dataService = dataService;
        }

        #endregion

		#region "Public Methods"

        #region RelationshipType CRUD

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save RelationshipType
        /// </summary>
        /// <param name="relationshipType">RelationshipType object</param>        
        /// <remarks>
        /// If RelationshipTypeID is -1 (Null.NullIntger), then a new RelationshipType is created, 
        /// else existing RelationshipType is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void SaveRelationshipType(RelationshipType relationshipType)
		{
            Requires.NotNull("The relationshipType can't be null", relationshipType);

            int relationshipTypeID = _dataService.SaveRelationshipType(relationshipType, UserController.GetCurrentUserInfo().UserID);

            //log event
            var logContent = string.Format("'{0}' RelationshipType '{1}'", relationshipType.RelationshipTypeID == Null.NullInteger ? "Added" : "Updated", relationshipType.Name);
			AddLog(logContent);

            relationshipType.RelationshipTypeID = relationshipTypeID;

            //clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
		}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete RelationshipType By RelationshipTypeID
        /// </summary>
        /// <param name="relationshipTypeID">RelationshipTypeID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteRelationshipType(int relationshipTypeID)
        {
            var relationshipType = GetRelationshipType(relationshipTypeID);

            if (relationshipType != null)
            {
                _dataService.DeleteRelationshipType(relationshipTypeID);

                //log event
                var logContent = string.Format("'{0}', ID {1} RelationshipType 'Deleted'", relationshipType.Name, relationshipType.RelationshipTypeID);
                AddLog(logContent);

                //clear cache
                DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get list of All RelationshipTypes defined in system
        /// </summary>        
        /// -----------------------------------------------------------------------------
        public IList<RelationshipType> GetAllRelationshipTypes()
        {
            var cacheArg = new CacheItemArgs(DataCache.RelationshipTypesCacheKey, DataCache.RelationshipTypesCacheTimeOut, DataCache.RelationshipTypesCachePriority, "");
            return CBO.GetCachedObject<IList<RelationshipType>>(cacheArg, GetAllRelationshipTypesCallBack);
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get RelationshipType By RelationshipTypeID
        /// </summary>        
        /// <param name="relationshipTypeID">RelationshipTypeID</param>        
        /// -----------------------------------------------------------------------------
        public RelationshipType GetRelationshipType(int relationshipTypeID)
        {
            return GetAllRelationshipTypes().Where(r => r.RelationshipTypeID == relationshipTypeID).FirstOrDefault();
        }

        #endregion

        #region Relationship CRUD

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save Relationship
        /// </summary>
        /// <param name="relationship">Relationship object</param>        
        /// <remarks>
        /// If RelationshipID is -1 (Null.NullIntger), then a new Relationship is created, 
        /// else existing Relationship is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void SaveRelationship(Relationship relationship)
        {
            Requires.NotNull("The relationship can't be null", relationship);

            int relationshipID = _dataService.SaveRelationship(relationship, UserController.GetCurrentUserInfo().UserID);

            //log event
            var logContent = string.Format("'{0}' Relationship '{1}'", relationship.RelationshipID == Null.NullInteger ? "Added" : "Updated", relationship.Name);
            AddLog(logContent);

            relationship.RelationshipID = relationshipID;

            //clear cache
            ClearRelationshipCache(relationship);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By RelationshipID
        /// </summary>
        /// <param name="relationshipID">RelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteRelationship(int relationshipID)
        {
            var relationship = GetRelationship(relationshipID);

            if (relationship != null)
            {
                _dataService.DeleteRelationship(relationshipID);

                //log event
                var logContent = string.Format("'{0}', ID {1} Relationship 'Deleted'", relationship.Name, relationship.RelationshipID);
                AddLog(logContent);

                //clear cache
                ClearRelationshipCache(relationship);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By UserID
        /// </summary>
        /// <param name="userID">UserID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteRelationshipByUserID(int userID)
        {
            _dataService.DeleteRelationshipByUserID(userID);

            //log event
            var logContent = string.Format("All Relationships for UserID '{0}' 'Deleted'", userID);
            AddLog(logContent);
            
            //cache clear
            DataCache.RemoveCache(string.Format(DataCache.RelationshipByUserIDCacheKey, userID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By PortalID
        /// </summary>
        /// <param name="portalID">PortalID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteRelationshipByPortalID(int portalID)
        {
            _dataService.DeleteRelationshipByPortalID(portalID);

            //log event
            var logContent = string.Format("All Relationships for PortalID '{0}' 'Deleted'", portalID);
            AddLog(logContent);

            //cache clear            
            DataCache.RemoveCache(string.Format(DataCache.RelationshipByPortalIDCacheKey, portalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationship By RelationshipID
        /// </summary>        
        /// <param name="relationshipID">RelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public Relationship GetRelationship(int relationshipID)
        {
            return CBO.FillCollection<Relationship>(_dataService.GetRelationship(relationshipID)).FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By UserID
        /// </summary>        
        /// <param name="userID">UserID</param>        
        /// -----------------------------------------------------------------------------
        public IList<Relationship> GetRelationshipsByUserID(int userID)
        {
            var cacheArg = new CacheItemArgs(string.Format(DataCache.RelationshipByUserIDCacheKey, userID), DataCache.RelationshipByUserIDCacheTimeOut, DataCache.RelationshipByUserIDCachePriority, userID);
            return CBO.GetCachedObject<IList<Relationship>>(cacheArg, GetRelationshipsByUserIDCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By PortalID
        /// </summary>        
        /// <param name="portalID">PortalID</param>        
        /// -----------------------------------------------------------------------------
        public IList<Relationship> GetRelationshipsByPortalID(int portalID)
        {
            var cacheArg = new CacheItemArgs(string.Format(DataCache.RelationshipByPortalIDCacheKey, portalID), DataCache.RelationshipByPortalIDCacheTimeOut, DataCache.RelationshipByPortalIDCachePriority, portalID);
            return CBO.GetCachedObject<IList<Relationship>>(cacheArg, GetRelationshipsByPortalIDCallBack);            
        }

        #endregion

        #region UserRelationship CRUD

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save UserRelationship
        /// </summary>
        /// <param name="userRelationship">UserRelationship object</param>        
        /// <remarks>
        /// If UserRelationshipID is -1 (Null.NullIntger), then a new UserRelationship is created, 
        /// else existing UserRelationship is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void SaveUserRelationship(UserRelationship userRelationship)
        {
            Requires.NotNull("The user relationship can't be null", userRelationship);

            int userRelationshipID = _dataService.SaveUserRelationship(userRelationship, UserController.GetCurrentUserInfo().UserID);

            //log event            
            var logContent = string.Format("UserRelationshipID for ID '{0}', UserID '{1}', RelatedUserID '{2}' '{3}'", userRelationship.UserRelationshipID, userRelationship.UserID, userRelationship.RelatedUserID, userRelationship.RelationshipID == Null.NullInteger ? "Added" : "Updated");
            AddLog(logContent);

            userRelationship.UserRelationshipID = userRelationshipID;

            //clear cache
            DataCache.RemoveCache(string.Format(DataCache.UserRelationshipByUserIDCacheKey, userRelationship.UserID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By UserRelationshipID
        /// </summary>
        /// <param name="userRelationshipID">UserRelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteUserRelationship(int userRelationshipID)
        {
            var userRelationship = GetUserRelationship(userRelationshipID);
            DeleteUserRelationship(userRelationship);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete USerRelationship By UserID
        /// </summary>
        /// <param name="userID">UserID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteUserRelationshipByUserID(int userID)
        {
            var userRelationships = GetUserRelationshipsByUserID(userID);

            foreach (var userRelationship in userRelationships)
            {
                DeleteUserRelationship(userRelationship);
            } 
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By RelatedUserlID
        /// </summary>
        /// <param name="relatedUserID">RelatedUserID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteRelationshipByRelatedID(int relatedUserID)
        {
            var userRelationships = GetUserRelationshipsByRelatedUserID(relatedUserID);

            foreach (var userRelationship in userRelationships)
            {
                DeleteUserRelationship(userRelationship);
            }                        
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship By RelationshipID
        /// </summary>
        /// <param name="relationshipID">RelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public void DeleteUserRelationshipByRelationshipID(int relationshipID)
        {
            var userRelationships = GetUserRelationshipsByRelationshipID(relationshipID);

            foreach(var userRelationship in userRelationships)
            {
                DeleteUserRelationship(userRelationship);                
            }            
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By UserRelationshipID
        /// </summary>        
        /// <param name="userRelationshipID">UserRelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public UserRelationship GetUserRelationship(int userRelationshipID)
        {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationship(userRelationshipID)).FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By Multiple IDs
        /// </summary>        
        /// <param name="userID">UserID</param>        
        /// <param name="relatedUserID">RelatedUserID</param>   
        /// <param name="relationshipID">RelationshipID</param>             
        /// -----------------------------------------------------------------------------
        public UserRelationship GetUserRelationship(int userID, int relatedUserID, int relationshipID)
        {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationship(userID, relatedUserID, relationshipID)).FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By UserID
        /// </summary>        
        /// <param name="userID">UserID</param>        
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetUserRelationshipsByUserID(int userID)
        {
            var cacheArg = new CacheItemArgs(string.Format(DataCache.UserRelationshipByUserIDCacheKey, userID), DataCache.UserRelationshipByUserIDCacheTimeOut, DataCache.UserRelationshipByUserIDCachePriority, userID);
            return CBO.GetCachedObject<IList<UserRelationship>>(cacheArg, GetUserRelationshipsByUserIDCallBack);            
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By RelatedUserID
        /// </summary>        
        /// <param name="relatedUserID">RelatedUserID</param>        
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetUserRelationshipsByRelatedUserID(int relatedUserID)
        {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationshipsByRelatedUserID(relatedUserID)).ToList();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By RelationshipID
        /// </summary>        
        /// <param name="relationshipID">RelationshipID</param>        
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetUserRelationshipsByRelationshipID(int relationshipID)
	    {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationshipsByRelationshipID(relationshipID)).ToList();
	    }

        #endregion

        #region Relationship Business APIs
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initiate a UserRelationship Request
        /// </summary>
        /// <param name="initiatingUser">UserInfo of the user initiating the request</param>        
        /// <param name="targetUser">UserInfo of the user being solicited for initiating the request</param>        
        /// <param name="relationship">Relationship to associate this request to (Portal-Level Relationship or User-Level Relationship)</param>        
        /// <remarks>
        /// If all conditions are met UserRelationship object belonging to Initiating User is returned.
        /// </remarks>
        /// <returns>
        /// Relationship object belonging to the initiating user
        /// </returns>
        /// <exception cref="UserRelationshipBlockedException">Target user has Blocked any relationship request from Initiating user</exception>
        /// <exception cref="InvalidRelationshipTypeException">Relationship type does not exist</exception>
        /// -----------------------------------------------------------------------------
        public UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser, Relationship relationship)
        {
            Requires.NotNull("The initiatingUser can't be null", initiatingUser);
            Requires.NotNull("The targetUser can't be null", targetUser);
            Requires.NotNull("The relationship can't be null", relationship);

            Requires.PropertyNotNegative("initiatingUser", "UserID", initiatingUser.UserID);
            Requires.PropertyNotNegative("targetUser", "UserID", targetUser.UserID);

            Requires.PropertyNotNegative("initiatingUser", "PortalID", initiatingUser.PortalID);
            Requires.PropertyNotNegative("targetUser", "PortalID", targetUser.PortalID);

            Requires.PropertyNotNegative("relationship", "RelationshipID", relationship.RelationshipID);

            //cannot be same user
            if (initiatingUser.UserID == targetUser.UserID)
            {
                throw new UserRelationshipForSameUsersException(Localization.GetExceptionMessage("UserRelationshipForSameUsersError", "Initiating and Target Users cannot have same UserID '{0}'.", initiatingUser.UserID));
            }

            //users must be from same portal
            if (initiatingUser.PortalID != targetUser.PortalID)
            {
                throw new UserRelationshipForDifferentPortalException(Localization.GetExceptionMessage("UserRelationshipForDifferentPortalError", "Portal ID '{0}' of Initiating User is different from Portal ID '{1}' of Target  User.", initiatingUser.PortalID, targetUser.PortalID));
            }

            //check for existing UserRelationship record
            var existingRelationship = GetUserRelationship(initiatingUser.UserID, targetUser.UserID, relationship.RelationshipID);
            if(existingRelationship != null) 
            {
                if (existingRelationship.Status == RelationshipStatus.Blocked)
                {
                    throw new UserRelationshipBlockedException(Localization.GetExceptionMessage("UserRelationshipBlockedError", "Target User '{0}' has Blcoked Relationship '{1}' from Target User '{2}'.", initiatingUser.UserID, relationship.RelationshipID, targetUser.UserID));
                }
                                    
                throw new UserRelationshipExistsException(Localization.GetExceptionMessage("UserRelationshipExistsError", "Relationship already exists for Initiating User '{0}' Target User '{1}' RelationshipID '{2}'.", initiatingUser.UserID, targetUser.UserID, relationship.RelationshipID));
            }

            //no existing UserRelationship record found 

            
            //use Relationship DefaultResponse as status
            var status = relationship.DefaultResponse;

            //check if there is a custom relationship status setting for the user

            if(status == RelationshipStatus.None)
                status = RelationshipStatus.Initiated;

            var userRelationship = new UserRelationship { UserRelationshipID = Null.NullInteger, UserID = initiatingUser.UserID, RelatedUserID = targetUser.UserID, RelationshipID = relationship.RelationshipID, Status = status };

            SaveUserRelationship(userRelationship);

            return userRelationship;            
        }

        public void AcceptUserRelationship(int userRelationshipID)
        {
            ManageUserRelationshipStatus(userRelationshipID, RelationshipStatus.Accepted);
        }

        public void RejectUserRelationship(int userRelationshipID)
        {
            ManageUserRelationshipStatus(userRelationshipID, RelationshipStatus.Rejected);
        }

        public void ReportUserRelationship(int userRelationshipID)
        {
            ManageUserRelationshipStatus(userRelationshipID, RelationshipStatus.Reported);
        }

        public void IgnoreUserRelationship(int userRelationshipID)
        {
            ManageUserRelationshipStatus(userRelationshipID, RelationshipStatus.Ignored);
        }

        public void BlockUserRelationship(int userRelationshipID)
        {
            ManageUserRelationshipStatus(userRelationshipID, RelationshipStatus.Blocked);
        }

        public void RemoveUserRelationship(int userRelationshipID)
        {
            var userRelationship = VerifyUserRelationshipExist(userRelationshipID);
            if (userRelationship != null)
            {
                DeleteUserRelationship(userRelationship);
            }
        }       
        

        #endregion

        #region Easy Wrapper APIs

        public UserRelationship AddFriend(UserInfo targetUser)
        {
            return AddFriend(UserController.GetCurrentUserInfo(), targetUser);
        }

        public UserRelationship AddFriend(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("The initiatingUser can't be null", initiatingUser);

            return InitiateUserRelationship(initiatingUser, targetUser, GetFriendsRelatioshipByPortal(initiatingUser.PortalID));
        }

        public Relationship AddUserList(string listName, string listDescription)
        {
            return AddUserList(UserController.GetCurrentUserInfo(), listName, listDescription, RelationshipStatus.None);
        }

        public Relationship AddUserList(UserInfo owningUser, string listName, string listDescription, RelationshipStatus defaultStatus)
        {
            var relationship = new Relationship { RelationshipID = Null.NullInteger, Name = listName, Description = listDescription, PortalID = owningUser.PortalID, UserID = owningUser.UserID, DefaultResponse = defaultStatus, RelationshipTypeID = (int)DefaultRelationshipTypes.CustomList};

            SaveRelationship(relationship);

            return relationship;
        }

        public Relationship AddPortalRelationship(int portalID, string listName, string listDescription, RelationshipStatus defaultStatus, int relationshipTypeID)
        {
            var relationship = new Relationship { RelationshipID = Null.NullInteger, Name = listName, Description = listDescription, PortalID = portalID, UserID = Null.NullInteger, DefaultResponse = defaultStatus, RelationshipTypeID = relationshipTypeID};

            SaveRelationship(relationship);

            return relationship;            
        }

        #endregion

        #endregion


        #region "Internal Methods"

        internal void CreateDefaultRelationshipsForPortal(int portalID)
        {
            //create default Friend Relationship
            if (GetFriendsRelatioshipByPortal(portalID) == null)
            {
                var friendRelationship = new Relationship
                {
                    RelationshipID = Null.NullInteger,
                    Name = DefaultRelationshipTypes.Friends.ToString(),
                    Description = DefaultRelationshipTypes.Friends.ToString(),
                    PortalID = portalID,
                    UserID = Null.NullInteger,
                    DefaultResponse = RelationshipStatus.None,
                    RelationshipTypeID = (int)DefaultRelationshipTypes.Friends
                };
                SaveRelationship(friendRelationship);
            }

            //create default Follower Relationship
            if (GetFollowersRelatioshipByPortal(portalID) == null)
            {

                var followerRelationship = new Relationship
                {
                    RelationshipID = Null.NullInteger,
                    Name = DefaultRelationshipTypes.Followers.ToString(),
                    Description = DefaultRelationshipTypes.Followers.ToString(),
                    PortalID = portalID,
                    UserID = Null.NullInteger,
                    DefaultResponse = RelationshipStatus.None,
                    RelationshipTypeID = (int)DefaultRelationshipTypes.Followers
                };
                SaveRelationship(followerRelationship);
            }
        }

        #endregion


        #region "Private Methods"

        private void ManageUserRelationshipStatus(int userRelationshipID, RelationshipStatus newStatus)
        {
            var userRelationship = VerifyUserRelationshipExist(userRelationshipID);
            if (userRelationship == null) return;

            //TODO - apply business rules - throw exception if newStatus requested is not allowed
            bool save = false;
            switch (newStatus)
            {
                case RelationshipStatus.None:
                    save = true;
                    break;
                case RelationshipStatus.Initiated:
                    save = true;
                    break;
                case RelationshipStatus.Accepted:
                    save = true;
                    break;
                case RelationshipStatus.Rejected:
                    save = true;
                    break;
                case RelationshipStatus.Ignored:
                    save = true;
                    break;
                case RelationshipStatus.Reported:
                    save = true;
                    break;
                case RelationshipStatus.Blocked:
                    save = true;
                    break;
            }

            if(save)
            {
                userRelationship.Status = newStatus;
                SaveUserRelationship(userRelationship);
            }
        }

        private UserRelationship VerifyUserRelationshipExist(int userRelationshipID)
        {
            var userRelationship = GetUserRelationship(userRelationshipID);
            if (userRelationship == null)
            {
                throw new UserRelationshipDoesNotExistException(Localization.GetExceptionMessage("UserRelationshipDoesNotExistError", "UserRelationshipID '{0}' does not exist.", userRelationshipID));
            }

            return userRelationship;
        }

        private IList<RelationshipType> GetAllRelationshipTypesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<RelationshipType>(_dataService.GetAllRelationshipTypes()).ToList();
        }

        private IList<Relationship> GetRelationshipsByUserIDCallBack(CacheItemArgs cacheItemArgs)
        {
            int userID = (int)cacheItemArgs.ParamList[0];
            return CBO.FillCollection<Relationship>(_dataService.GetRelationshipsByUserID(userID)).ToList();
        }

        private IList<Relationship> GetUserRelationshipsByUserIDCallBack(CacheItemArgs cacheItemArgs)
        {
            int userID = (int)cacheItemArgs.ParamList[0];
            return CBO.FillCollection<Relationship>(_dataService.GetUserRelationshipsByUserID(userID)).ToList();
        }

        private IList<Relationship> GetRelationshipsByPortalIDCallBack(CacheItemArgs cacheItemArgs)
        {
            int portalID = (int)cacheItemArgs.ParamList[0];
            return CBO.FillCollection<Relationship>(_dataService.GetRelationshipsByPortalID(portalID)).ToList();
        }

		private void AddLog(string logContent)
		{
			var objEventLog = new EventLogController();
			objEventLog.AddLog("Message", logContent, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.ADMIN_ALERT);
		}

        private void DeleteUserRelationship(UserRelationship userRelationship)
        {
            _dataService.DeleteUserRelationship(userRelationship.UserRelationshipID);

            //log event
            var logContent = string.Format("UserRelationshipID for ID '{0}', UserID '{1}', RelatedUserID '{2}' 'Deleted'", userRelationship.UserRelationshipID, userRelationship.UserID, userRelationship.RelatedUserID);
            AddLog(logContent);

            //cache clear
            DataCache.RemoveCache(string.Format(DataCache.UserRelationshipByUserIDCacheKey, userRelationship.UserID));
        }


        private void ClearRelationshipCache(Relationship relationship)
        {
            DataCache.RemoveCache(relationship.UserID != Null.NullInteger
                                      ? string.Format(DataCache.RelationshipByUserIDCacheKey, relationship.UserID)
                                      : string.Format(DataCache.RelationshipByPortalIDCacheKey, relationship.PortalID));
            
        }

        private Relationship GetFriendsRelatioshipByPortal(int portalID)
        {
            return GetRelationshipsByPortalID(portalID).Where(re => re.RelationshipTypeID == (int) DefaultRelationshipTypes.Friends).FirstOrDefault();
        }

        private Relationship GetFollowersRelatioshipByPortal(int portalID)
        {
            return GetRelationshipsByPortalID(portalID).Where(re => re.RelationshipTypeID == (int)DefaultRelationshipTypes.Followers).FirstOrDefault();
        }

		#endregion       
    }
}