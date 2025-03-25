﻿#region License

// Distributed under the MIT License
// ============================================================
// Copyright (c) Upendo Ventures, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or 
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

// required for DNN Prompt
//using DotNetNuke.Prompt; // deprecated Prompt is replaced with this
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities; // added for the commented-out clearing of cache below
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Upendo.Modules.UpendoPrompt.Entities;
using System.Web.Security;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using System.Linq;
using System.Collections;
using Upendo.Modules.UpendoPrompt.Custom;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("apply-roles", Constants.PromptCategory, "ApplyRoles")]
    public class ApplyRoles : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DemoUsers));
        private readonly PortalController _portalController = new PortalController();

        private const string PortalIdFlag = "portalId";
        private const string RoleNameFlag = "roleName";
        private const string GenericRoleDescriptionValue = "Role automatically generated by the user via the Upendo Prompt command";
        private const string EditPermissionKey = "EDIT";
        private const string ViewPermissionKey = "VIEW";
        private const string RoleNameDefault = "Site Admin";

        #region Properties
        private int? PortalId { get; set; }
        private string RoleName { get; set; }
        #endregion

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.PortalId = this.GetFlagValue(PortalIdFlag, $"{PortalIdFlag}", Null.NullInteger, false);
            this.RoleName = this.GetFlagValue(RoleNameFlag, $"{RoleNameFlag}", string.Empty, false);
        }

        public override ConsoleResultModel Run()
        {
            string roleNameValue = this.RoleName;
            int? portalIdValue = this.PortalId.Value;

            if (string.IsNullOrEmpty(roleNameValue))
            {
                roleNameValue = RoleNameDefault;
            }

            if (portalIdValue != null)
            {
                if (portalIdValue < -1)
                {
                    return new ConsoleErrorResultModel(this.LocalizeString(Constants.LocalizationKeys.AddRolePortalIdNegative));
                }
                if (portalIdValue == -1)
                {
                    portalIdValue = 0;
                }
            }

            try
            {
                var portal = _portalController.GetPortal(portalIdValue.Value);
                if (portal != null)
                {
                    ApplySiteAdminRole(roleNameValue, portalIdValue.Value);

                    var output = (string.Format(this.LocalizeString(Constants.LocalizationKeys.AddRoleSuccess), roleNameValue, portalIdValue));

                    return new CustomConsoleResultModel
                    {
                        Output = output,
                        IsError = false
                    };
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format(this.LocalizeString(Constants.LocalizationKeys.AddRolePortalNotFound), portalIdValue.Value));
                }
            }
            catch (Exception e)
            {
                LogError(e);
                return new ConsoleErrorResultModel(string.Concat(Constants.OutputPrefix, this.LocalizeString(Constants.LocalizationKeys.ErrorOccurred)));
            }
        }

        #endregion

        #region Helpers


        private PromptMessage ApplySiteAdminRole(string roleName, int portalId)
        {
            try
            {
                var roleController = new RoleController();
                var tabController = new TabController();



                var siteAdminRole = roleController.GetRoleByName(portalId, roleName);

                if (siteAdminRole == null)
                {
                    var roleInfo = new RoleInfo
                    {
                        PortalID = portalId,
                        RoleName = roleName,
                        Description = GenericRoleDescriptionValue,
                        RoleGroupID = -1,
                        IsPublic = false,
                        Status = RoleStatus.Approved
                    };
                    int roleId = roleController.AddRole(roleInfo);
                    siteAdminRole = roleController.GetRoleById(portalId, roleId);
                }

                if (siteAdminRole != null)
                {
                    var tabs = tabController.GetTabsByPortal(portalId);

                    foreach (TabInfo tab in tabs.Values)
                    {
                        var editPermissionId = GetEditPermissionId();
                        var viewPermissionId = GetViewPermissionId();

                        var tabPermissionList = tab.TabPermissions.Cast<TabPermissionInfo>().ToList();

                        bool hasEditPermission = tabPermissionList.Any(p => p.PermissionID == editPermissionId && p.RoleID == siteAdminRole.RoleID);
                        bool hasViewPermission = tabPermissionList.Any(p => p.PermissionID == viewPermissionId && p.RoleID == siteAdminRole.RoleID);

                        if (!hasEditPermission)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                TabID = tab.TabID,
                                RoleID = siteAdminRole.RoleID,
                                PermissionID = editPermissionId,
                                AllowAccess = true
                            });
                        }

                        if (!hasViewPermission)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                TabID = tab.TabID,
                                RoleID = siteAdminRole.RoleID,
                                PermissionID = viewPermissionId,
                                AllowAccess = true
                            });
                        }

                        try
                        {
                            tabController.UpdateTab(tab);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                            throw;
                        }

                    }
                }
                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.AddRoleSuccess), roleName, portalId));
            }

            catch (Exception ex)
            {
                LogError(ex);
                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.AddRoleFailure), roleName, portalId));
                throw;
            }
        }

        /// <summary>
        /// Get Edit Permission Id
        /// </summary>
        /// <returns></returns>
        private int GetEditPermissionId()
        {
            var permissions = PermissionController.GetPermissionsByTab();
            var permissionList = permissions.Cast<PermissionInfo>().ToList();
            return permissionList
                .Single(p => p.PermissionKey == EditPermissionKey).PermissionID;
        }

        /// <summary>
        /// Get View Permission Id
        /// </summary>
        /// <returns></returns>
        private int GetViewPermissionId()
        {
            var permissions = PermissionController.GetPermissionsByTab();
            var permissionList = permissions.Cast<PermissionInfo>().ToList();
            return permissionList
                .Single(p => p.PermissionKey == ViewPermissionKey).PermissionID;
        }

        protected override void LogError(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
        }
        #endregion
    }
}