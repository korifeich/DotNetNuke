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

using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Social.Data;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Users.Social.Internal
{
    internal class RelationshipControllerImpl : IRelationshipController
    {
        private readonly IDataService _dataService;
        private readonly IEventLogController _eventLogController;

        #region Constructors

        public RelationshipControllerImpl() : this(DataService.Instance, new EventLogController())
        {
        }

        public RelationshipControllerImpl(IDataService dataService, IEventLogController eventLogController)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("eventLogController", eventLogController);

            _dataService = dataService;
            _eventLogController = eventLogController;
        }

        #endregion

        #region IRelationshipController Implementation

        #region RelationshipType CRUD

        public void DeleteRelationshipType(RelationshipType relationshipType)
        {
            Requires.NotNull("relationshipType", relationshipType);

            _dataService.DeleteRelationshipType(relationshipType.RelationshipTypeId);

            //log event
            string logContent =
                string.Format(Localization.GetString("RelationshipType_Deleted", Localization.GlobalResourceFile),
                              relationshipType.Name, relationshipType.RelationshipTypeId);
            AddLog(logContent);

            //clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
        }

        public IList<RelationshipType> GetAllRelationshipTypes()
        {
            var cacheArgs = new CacheItemArgs(DataCache.RelationshipTypesCacheKey,
                                              DataCache.RelationshipTypesCacheTimeOut,
                                              DataCache.RelationshipTypesCachePriority);
            return CBO.GetCachedObject<IList<RelationshipType>>(cacheArgs,
                                                                c =>
                                                                CBO.FillCollection<RelationshipType>(
                                                                    _dataService.GetAllRelationshipTypes()));
        }

        public RelationshipType GetRelationshipType(int relationshipTypeId)
        {
            return GetAllRelationshipTypes().FirstOrDefault(r => r.RelationshipTypeId == relationshipTypeId);
        }

        public void SaveRelationshipType(RelationshipType relationshipType)
        {
            Requires.NotNull("relationshipType", relationshipType);

            string localizationKey = (relationshipType.RelationshipTypeId == Null.NullInteger)
                                         ? "RelationshipType_Added"
                                         : "RelationshipType_Updated";

            relationshipType.RelationshipTypeId = _dataService.SaveRelationshipType(relationshipType,
                                                                                    UserController.GetCurrentUserInfo().
                                                                                        UserID);

            //log event
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              relationshipType.Name);
            AddLog(logContent);

            //clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
        }

        #endregion

        #region Relationship CRUD

        public void DeleteRelationship(Relationship relationship)
        {
            Requires.NotNull("relationship", relationship);

            _dataService.DeleteRelationship(relationship.RelationshipId);

            //log event
            string logContent =
                string.Format(Localization.GetString("Relationship_Deleted", Localization.GlobalResourceFile),
                              relationship.Name, relationship.RelationshipId);
            AddLog(logContent);

            //clear cache
            ClearRelationshipCache(relationship);
        }

        public Relationship GetRelationship(int relationshipId)
        {
            return CBO.FillCollection<Relationship>(_dataService.GetRelationship(relationshipId)).FirstOrDefault();
        }

        public IList<Relationship> GetRelationshipsByUserId(int userId)
        {
            return CBO.FillCollection<Relationship>(_dataService.GetRelationshipsByUserId(userId));
        }

        public IList<Relationship> GetRelationshipsByPortalId(int portalId)
        {
            var cacheArgs = new CacheItemArgs(string.Format(DataCache.RelationshipByPortalIDCacheKey, portalId),
                                              DataCache.RelationshipByPortalIDCacheTimeOut,
                                              DataCache.RelationshipByPortalIDCachePriority,
                                              portalId);
            return CBO.GetCachedObject<IList<Relationship>>(cacheArgs,
                                                            c =>
                                                            CBO.FillCollection<Relationship>(
                                                                _dataService.GetRelationshipsByPortalId(
                                                                    (int) c.ParamList[0])));
        }

        public void SaveRelationship(Relationship relationship)
        {
            Requires.NotNull("relationship", relationship);

            string localizationKey = (relationship.RelationshipId == Null.NullInteger)
                                         ? "Relationship_Added"
                                         : "Relationship_Updated";

            relationship.RelationshipId = _dataService.SaveRelationship(relationship,
                                                                        UserController.GetCurrentUserInfo().UserID);

            //log event
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              relationship.Name);
            AddLog(logContent);

            //clear cache
            ClearRelationshipCache(relationship);
        }

        #endregion

        #region UserRelationship CRUD

        public void DeleteUserRelationship(UserRelationship userRelationship)
        {
            Requires.NotNull("userRelationship", userRelationship);

            _dataService.DeleteUserRelationship(userRelationship.UserRelationshipId);

            //log event
            string logContent =
                string.Format(Localization.GetString("UserRelationship_Deleted", Localization.GlobalResourceFile),
                              userRelationship.UserRelationshipId, userRelationship.UserId,
                              userRelationship.RelatedUserId);
            AddLog(logContent);

            //cache clear
            ClearUserCache(userRelationship);
        }

        public UserRelationship GetUserRelationship(int userRelationshipId)
        {
            return CBO.FillObject<UserRelationship>(_dataService.GetUserRelationship(userRelationshipId));
        }

        public UserRelationship GetUserRelationship(UserInfo user, UserInfo relatedUser, Relationship relationship)
        {
            return
                CBO.FillObject<UserRelationship>(_dataService.GetUserRelationship(user.UserID, relatedUser.UserID,
                                                                                  relationship.RelationshipId,
                                                                                  GetRelationshipType(
                                                                                      relationship.RelationshipTypeId).
                                                                                      Direction));
        }

        public IList<UserRelationship> GetUserRelationships(UserInfo user)
        {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationships(user.UserID));
        }

        public void SaveUserRelationship(UserRelationship userRelationship)
        {
            Requires.NotNull("userRelationship", userRelationship);

            string localizationKey = (userRelationship.UserRelationshipId == Null.NullInteger)
                                         ? "UserRelationship_Added"
                                         : "UserRelationship_Updated";

            userRelationship.UserRelationshipId = _dataService.SaveUserRelationship(userRelationship,
                                                                                    UserController.GetCurrentUserInfo().
                                                                                        UserID);

            //log event            
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              userRelationship.UserRelationshipId, userRelationship.UserId,
                                              userRelationship.RelatedUserId);
            AddLog(logContent);

            //cache clear
            ClearUserCache(userRelationship);
        }

        #endregion

        #region UserRelationshipPreference CRUD

        public void DeleteUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            _dataService.DeleteUserRelationshipPreference(userRelationshipPreference.PreferenceId);

            //log event
            string logContent =
                string.Format(
                    Localization.GetString("UserRelationshipPreference_Deleted", Localization.GlobalResourceFile),
                    userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId,
                    userRelationshipPreference.RelationshipId);
            AddLog(logContent);
        }

        public UserRelationshipPreference GetUserRelationshipPreference(int preferenceId)
        {
            return
                CBO.FillObject<UserRelationshipPreference>(_dataService.GetUserRelationshipPreferenceById(preferenceId));
        }

        public UserRelationshipPreference GetUserRelationshipPreference(int userId, int relationshipId)
        {
            return
                CBO.FillObject<UserRelationshipPreference>(_dataService.GetUserRelationshipPreference(userId,
                                                                                                      relationshipId));
        }

        public void SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            string localizationKey = (userRelationshipPreference.PreferenceId == Null.NullInteger)
                                         ? "UserRelationshipPreference_Added"
                                         : "UserRelationshipPreference_Updated";

            userRelationshipPreference.PreferenceId =
                _dataService.SaveUserRelationshipPreference(userRelationshipPreference,
                                                            UserController.GetCurrentUserInfo().UserID);

            //log event            
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId,
                                              userRelationshipPreference.RelationshipId);
            AddLog(logContent);
        }

        #endregion

        #region Relationship Business APIs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initiate an UserRelationship Request
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
        public UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser,
                                                         Relationship relationship)
        {
            Requires.NotNull("initiatingUser", initiatingUser);
            Requires.NotNull("targetUser", targetUser);
            Requires.NotNull("relationship", relationship);

            Requires.PropertyNotNegative("initiatingUser", "UserID", initiatingUser.UserID);
            Requires.PropertyNotNegative("targetUser", "UserID", targetUser.UserID);

            Requires.PropertyNotNegative("initiatingUser", "PortalID", initiatingUser.PortalID);
            Requires.PropertyNotNegative("targetUser", "PortalID", targetUser.PortalID);

            Requires.PropertyNotNegative("relationship", "RelationshipId", relationship.RelationshipId);

            //cannot be same user
            if (initiatingUser.UserID == targetUser.UserID)
            {
                throw new UserRelationshipForSameUsersException(
                    Localization.GetExceptionMessage("UserRelationshipForSameUsersError",
                                                     "Initiating and Target Users cannot have same UserID '{0}'.",
                                                     initiatingUser.UserID));
            }

            //users must be from same portal
            if (initiatingUser.PortalID != targetUser.PortalID)
            {
                throw new UserRelationshipForDifferentPortalException(
                    Localization.GetExceptionMessage("UserRelationshipForDifferentPortalError",
                                                     "Portal ID '{0}' of Initiating User is different from Portal ID '{1}' of Target  User.",
                                                     initiatingUser.PortalID, targetUser.PortalID));
            }

            //check for existing UserRelationship record
            UserRelationship existingRelationship = GetUserRelationship(initiatingUser, targetUser, relationship);
            if (existingRelationship != null)
            {
                throw new UserRelationshipExistsException(Localization.GetExceptionMessage(
                    "UserRelationshipExistsError",
                    "Relationship already exists for Initiating User '{0}' Target User '{1}' RelationshipID '{2}'.",
                    initiatingUser.UserID, targetUser.UserID, relationship.RelationshipId));
            }

            //no existing UserRelationship record found 


            //use Relationship DefaultResponse as status
            RelationshipStatus status = relationship.DefaultResponse;

            //check if there is a custom relationship status setting for the user. 
            //TODO - Is this check only applicable for portal or host list
            //if (relationship.IsPortalList || relationship.IsHostList)
            {
                UserRelationshipPreference preference = GetUserRelationshipPreference(targetUser.UserID,
                                                                                      relationship.RelationshipId);
                if (preference != null)
                {
                    status = preference.DefaultResponse;
                }
            }

            if (status == RelationshipStatus.None)
            {
                status = RelationshipStatus.Initiated;
            }

            var userRelationship = new UserRelationship
                                       {
                                           UserRelationshipId = Null.NullInteger,
                                           UserId = initiatingUser.UserID,
                                           RelatedUserId = targetUser.UserID,
                                           RelationshipId = relationship.RelationshipId,
                                           Status = status
                                       };

            SaveUserRelationship(userRelationship);

            return userRelationship;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Accept an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Accepted.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void AcceptUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Accepted);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reject an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Rejected.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void RejectUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Rejected);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Report an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Reported.
        /// </remarks>
        /// -----------------------------------------------------------------------------        
        public void ReportUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Reported);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Ignore an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Ignored.
        /// </remarks>
        /// ----------------------------------------------------------------------------- 
        public void IgnoreUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Ignored);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Block an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Blocked.
        /// </remarks>
        /// ----------------------------------------------------------------------------- 
        public void BlockUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Blocked);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Remove an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// UserRelationship record is physically removed.
        /// </remarks>
        /// -----------------------------------------------------------------------------  
        public void RemoveUserRelationship(int userRelationshipId)
        {
            UserRelationship userRelationship = VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship != null)
            {
                DeleteUserRelationship(userRelationship);
            }
        }

        #endregion

        #region Easy Wrapper APIs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsFriend - Are the Current User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>True or False</returns>
        /// <remarks>True is returned only if a Friend Relationship exists between the two Users. 
        /// The relation status must be Accepted. Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool IsFriend(UserInfo targetUser)
        {
            return AreFriends(UserController.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AreFriends - Are the Initiating User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>                
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>True or False</returns>
        /// <remarks>True is returned only if a Friend Relationship exists between the two Users. 
        /// The relation status must be Accepted. Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool AreFriends(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);
            Requires.NotNull("targetUser", targetUser);

            Relationship friend = GetFriendsRelatioshipByPortal(initiatingUser.PortalID);

            UserRelationship userRelationship = GetUserRelationship(initiatingUser, targetUser, friend);

            return (userRelationship != null && userRelationship.Status == RelationshipStatus.Accepted);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendRelationship - Get the UserRelationship between Current User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFriendRelationship(UserInfo targetUser)
        {
            return GetFriendRelationship(UserController.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFriendRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);
            Requires.NotNull("targetUser", targetUser);

            return GetUserRelationship(initiatingUser, targetUser,
                                       GetFriendsRelatioshipByPortal(initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFriend - Current User initiates a Friend Request to the Target User
        /// </summary>                
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Friend Relationship is setup for auto-acceptance at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship AddFriend(UserInfo targetUser)
        {
            return AddFriend(UserController.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFriend - Initiating User initiates a Friend Request to the Target User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Friend Relationship is setup for auto-acceptance at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship AddFriend(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);

            return InitiateUserRelationship(initiatingUser, targetUser,
                                            GetFriendsRelatioshipByPortal(initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFollower - Current User initiates a Follower Request to the Target User
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Follower Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship AddFollower(UserInfo targetUser)
        {
            return AddFollower(UserController.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFollower - Initiating User initiates a Friend Request to the Target User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Follower Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship AddFollower(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);

            return InitiateUserRelationship(initiatingUser, targetUser,
                                            GetFollowersRelatioshipByPortal(initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriends - Get List of Friends (UserRelationship with status as Accepted) for an User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <returns>List of UserRelationship objects</returns>
        /// <remarks>This method can bring huge set of data.         
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetFriends(UserInfo initiatingUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);

            IList<UserRelationship> friends;
            Relationship relationship = GetFriendsRelatioshipByPortal(initiatingUser.PortalID);
            initiatingUser.Social.UserRelationships.TryGetValue(relationship.RelationshipId, out friends);

            return friends;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFollowers - Get List of Followers (UserRelationship with status as Accepted) for an User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <returns>List of UserRelationship objects</returns>
        /// <remarks>This method can bring huge set of data. Followers means the User is being Followed by x other Users        
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetFollowers(UserInfo initiatingUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);

            IList<UserRelationship> followers;
            Relationship relationship = GetFollowersRelatioshipByPortal(initiatingUser.PortalID);
            initiatingUser.Social.UserRelationships.TryGetValue(relationship.RelationshipId, out followers);

            return followers;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriends - Get List of Following (UserRelationship with status as Accepted) for an User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <returns>List of UserRelationship objects</returns>
        /// <remarks>This method can bring huge set of data. Following means the User is Following x other Users        
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public IList<UserRelationship> GetFollowing(UserInfo initiatingUser)
        {
            Requires.NotNull("initiatingUser", initiatingUser);

            Relationship relationship = GetFriendsRelatioshipByPortal(initiatingUser.PortalID);
            return new List<UserRelationship>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddUserList - Add a new Relationship for the Current User, e.g. My Best Friends or My Inlaws.
        /// </summary>        
        /// <param name="listName">Name of the Relationship. It is also called as List Name, e.g. My School Friends.</param> 
        /// <param name="listDescription">Description of the Relationship. It is also called as List Description </param>         
        /// <returns>Relationship object</returns>
        /// <remarks>Each Portal contains default Relationships of Friends and Followers. 
        /// AddUserList can be used to create addiotional User-Level Relationships.
        /// A DefaultStatus of None is used in this call, indicating there will not be any auto-acceptance 
        /// when an UserRelationship request is made for this Relationship.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public Relationship AddUserList(string listName, string listDescription)
        {
            return AddUserList(UserController.GetCurrentUserInfo(), listName, listDescription, RelationshipStatus.None);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddUserList - Add a new Relationship for an User, e.g. My Best Friends or My Inlaws.
        /// </summary>        
        /// <param name="owningUser">UserInfo for the User to which the Relationship will belong. </param>        
        /// <param name="listName">Name of the Relationship. It is also called as List Name, e.g. My School Friends.</param> 
        /// <param name="listDescription">Description of the Relationship. It is also called as List Description </param>         
        /// <param name="defaultStatus">DefaultRelationshiStatus of the Relationship when a new UserRelationship is initiated. 
        /// E.g. Accepted, which means automatically accept all UserRelationship request. Default is None, meaning no automatic action.</param>         
        /// <returns>Relationship object</returns>
        /// <remarks>Each Portal contains default Relationships of Friends and Followers. 
        /// AddUserList can be used to create addiotional User-Level Relationships.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public Relationship AddUserList(UserInfo owningUser, string listName, string listDescription,
                                        RelationshipStatus defaultStatus)
        {
            var relationship = new Relationship
                                   {
                                       RelationshipId = Null.NullInteger,
                                       Name = listName,
                                       Description = listDescription,
                                       PortalId = owningUser.PortalID,
                                       UserId = owningUser.UserID,
                                       DefaultResponse = defaultStatus,
                                       RelationshipTypeId = (int) DefaultRelationshipTypes.CustomList
                                   };

            SaveRelationship(relationship);

            return relationship;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPortalRelationship - Add a new Relationship for a Portal, e.g. Co-workers
        /// </summary>        
        /// <param name="portalId">PortalId to which the Relationship will belong. Valid value includes Non-Negative number.</param>        
        /// <param name="listName">Name of the Relationship. It is also called as List Name, e.g. Co-Workers </param> 
        /// <param name="listDescription">Description of the Relationship. It is also called as List Description </param>         
        /// <param name="defaultStatus">DefaultRelationshiStatus of the Relationship when a new UserRelationship is initiated. 
        /// E.g. Accepted, which means automatically accept all UserRelationship request. Default is None, meaning no automatic action.</param> 
        /// <param name="relationshipTypeId">RelationshipTypeID. A new Relationship requires RelationshipType to be present in the system. </param> 
        /// <returns>Relationship object</returns>
        /// <remarks>Each Portal contains default Relationships of Friends and Followers. 
        /// AddPortalRelationship can be used to create addiotional Relationships
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public Relationship AddPortalRelationship(int portalId, string listName, string listDescription,
                                                  RelationshipStatus defaultStatus, int relationshipTypeId)
        {
            var relationship = new Relationship
                                   {
                                       RelationshipId = Null.NullInteger,
                                       Name = listName,
                                       Description = listDescription,
                                       PortalId = portalId,
                                       UserId = Null.NullInteger,
                                       DefaultResponse = defaultStatus,
                                       RelationshipTypeId = relationshipTypeId
                                   };

            SaveRelationship(relationship);

            return relationship;
        }

        /// <summary>
        /// GetUsersAdvancedSearch -- Gets a filtered list of users along with their profile properties.
        /// </summary>
        /// <param name="currentUser"> User to determine correct visibility permissions</param>
        /// <param name="filterUser"> User which could be used in conjunction with other parameters to filter the list in terms of relationship to.</param>
        /// <param name="filterRole">Filter the list of user based on their role.</param>
        /// <param name="relationshipType">Filter the list of user based on their relationship to the current user [currUser parameter].</param>
        /// <param name="propertyNamesValues"> A collection of Key Value pairs of property names and values to filter upon.</param>
        /// <param name="additionalParameters"> A collection of Key Value pairs of more fields to filter upon</param>
        /// <returns></returns>
        public IDataReader GetUsersAdvancedSearch(UserInfo currentUser, UserInfo filterUser, UserRoleInfo filterRole,
                                                  RelationshipType relationshipType,
                                                  IDictionary<string, string> propertyNamesValues,
                                                  Dictionary<string, string> additionalParameters)
        {
            if (additionalParameters == null)
            {
                additionalParameters = new Dictionary<string, string>();
            }

            int portalId = additionalParameters.ContainsKey("PortalId") && additionalParameters["PortalId"] != null
                               ? int.Parse(additionalParameters["PortalId"])
                               : filterUser.PortalID;

            bool isAdmin = additionalParameters.ContainsKey("IsAdmin") && additionalParameters["IsAdmin"] != null
                               ? bool.Parse(additionalParameters["IsAdmin"])
                               : true;
            int filterRoleId = filterRole != null ? filterRole.RoleID : -1;
            int relationshipTypeId = relationshipType != null ? relationshipType.RelationshipTypeId : -1;

            int pageIndex = additionalParameters.ContainsKey("PageIndex") && additionalParameters["PageIndex"] != null
                                ? int.Parse(additionalParameters["PageIndex"])
                                : 1;
            int records = additionalParameters.ContainsKey("Records") && additionalParameters["Records"] != null
                              ? int.Parse(additionalParameters["Records"])
                              : 1;
            string sortByColumn = additionalParameters.ContainsKey("SortBy") && additionalParameters["SortBy"] != null
                                      ? additionalParameters["SortBy"]
                                      : string.Empty;
            bool sortAscending = additionalParameters.ContainsKey("SortAscending") &&
                                 additionalParameters["SortAscending"] != null
                                     ? bool.Parse(additionalParameters["SortAscending"])
                                     : true;
            string propertyNamesFilter = propertyNamesValues.Aggregate("",
                                                                       (current, property) =>
                                                                       current + "," + property.Key);
            string propertyValuesFilter = propertyNamesValues.Aggregate("",
                                                                        (current, property) =>
                                                                        current + "," + property.Value);


            return _dataService.GetUsersAdvancedSearch(portalId, currentUser.UserID, filterUser.UserID, filterRoleId,
                                                       relationshipTypeId, isAdmin, records, pageIndex, sortByColumn,
                                                       sortAscending, propertyNamesFilter, propertyValuesFilter);
        }

        #endregion

        public void CreateDefaultRelationshipsForPortal(int portalId)
        {
            //create default Friend Relationship
            if (GetFriendsRelatioshipByPortal(portalId) == null)
            {
                var friendRelationship = new Relationship
                                             {
                                                 RelationshipId = Null.NullInteger,
                                                 Name = DefaultRelationshipTypes.Friends.ToString(),
                                                 Description = DefaultRelationshipTypes.Friends.ToString(),
                                                 PortalId = portalId,
                                                 UserId = Null.NullInteger,
                                                 DefaultResponse = RelationshipStatus.None,
                                                 //default response is None
                                                 RelationshipTypeId = (int) DefaultRelationshipTypes.Friends
                                             };
                SaveRelationship(friendRelationship);
            }

            //create default Follower Relationship
            if (GetFollowersRelatioshipByPortal(portalId) == null)
            {
                var followerRelationship = new Relationship
                                               {
                                                   RelationshipId = Null.NullInteger,
                                                   Name = DefaultRelationshipTypes.Followers.ToString(),
                                                   Description = DefaultRelationshipTypes.Followers.ToString(),
                                                   PortalId = portalId,
                                                   UserId = Null.NullInteger,
                                                   DefaultResponse = RelationshipStatus.Accepted,
                                                   //default response is Accepted
                                                   RelationshipTypeId = (int) DefaultRelationshipTypes.Followers
                                               };
                SaveRelationship(followerRelationship);
            }
        }

        #endregion

        #region Private Methods

        private void AddLog(string logContent)
        {
            _eventLogController.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT);
        }

        private void ClearRelationshipCache(Relationship relationship)
        {
            if (relationship.UserId == Null.NullInteger)
            {
                DataCache.RemoveCache(string.Format(DataCache.RelationshipByPortalIDCacheKey, relationship.PortalId));
            }
        }

        private void ClearUserCache(UserRelationship userRelationship)
        {
            //Get Portal
            PortalSettings settings = PortalController.GetCurrentPortalSettings();

            if (settings != null)
            {
                //Get User
                UserInfo user = UserController.GetUserById(settings.PortalId, userRelationship.UserId);

                if (user != null)
                {
                    DataCache.ClearUserCache(settings.PortalId, user.Username);
                }
            }
        }

        private Relationship GetFriendsRelatioshipByPortal(int portalId)
        {
            return
                GetRelationshipsByPortalId(portalId).Where(
                    re => re.RelationshipTypeId == (int) DefaultRelationshipTypes.Friends).FirstOrDefault();
        }

        private Relationship GetFollowersRelatioshipByPortal(int portalId)
        {
            return
                GetRelationshipsByPortalId(portalId).Where(
                    re => re.RelationshipTypeId == (int) DefaultRelationshipTypes.Followers).FirstOrDefault();
        }

        private void ManageUserRelationshipStatus(int userRelationshipId, RelationshipStatus newStatus)
        {
            UserRelationship userRelationship = VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship == null)
            {
                return;
            }

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

            if (save)
            {
                userRelationship.Status = newStatus;
                SaveUserRelationship(userRelationship);
            }
        }

        private UserRelationship VerifyUserRelationshipExist(int userRelationshipId)
        {
            UserRelationship userRelationship = GetUserRelationship(userRelationshipId);
            if (userRelationship == null)
            {
                throw new UserRelationshipDoesNotExistException(
                    Localization.GetExceptionMessage("UserRelationshipDoesNotExistError",
                                                     "UserRelationshipID '{0}' does not exist.", userRelationshipId));
            }

            return userRelationship;
        }

        #endregion
    }
}