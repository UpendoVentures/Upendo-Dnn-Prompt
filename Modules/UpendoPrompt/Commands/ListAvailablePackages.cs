#region License

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
using System.IO;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Upendo.Modules.UpendoPrompt.Data;
using Upendo.Modules.UpendoPrompt.Entities;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("list-availablepackages", Constants.PromptCategory, "PromptListAvailablePackages")]
    public class ListAvailablePackages : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListAvailablePackages));
        
        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // do nothing
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var messages = new List<PromptMessage>();
                messages = GetPackageMessages();

                var recordCount = messages?.Count ?? 0;

                var output = string.Empty;
                output = LocalizeString(recordCount > 0 ? Constants.LocalizationKeys.RECORDS_SOME : Constants.LocalizationKeys.RECORDS_NONE);

                if (recordCount > 0)
                {
                    return new ConsoleResultModel
                    {
                        Records = recordCount,
                        Data = messages,
                        Output = output
                    };
                }
                else
                {
                    return new ConsoleResultModel
                    {
                        Records = recordCount,
                        Output = output
                    };
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

        private List<PromptMessage> GetPackageMessages()
        {
            try
            {
                var ctlPackages = new PackagesController();
                var messages = new List<PromptMessage>();
                string outPath; // unused intentionally 

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Auth_System, out outPath))
                {
                    var pkgAuth = ctlPackages.GetAvailablePackages(Constants.PackageType.Auth_System);
                    if (pkgAuth != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageAuthentication), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Container, out outPath))
                {
                    var pkgContainer = ctlPackages.GetAvailablePackages(Constants.PackageType.Container);
                    if (pkgContainer != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageContainer), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.JavaScriptLibrary, out outPath))
                {
                    var pkgJsLibrary = ctlPackages.GetAvailablePackages(Constants.PackageType.JavaScriptLibrary);
                    if (pkgJsLibrary != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageJsLibrary), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Language, out outPath))
                {
                    var pkgLanguage = ctlPackages.GetAvailablePackages(Constants.PackageType.Language);
                    if (pkgLanguage != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageLanguage), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Library, out outPath))
                {
                    var pkgLibrary = ctlPackages.GetAvailablePackages(Constants.PackageType.Library);
                    if (pkgLibrary != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageLibrary), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Module, out outPath))
                {
                    var pkgModule = ctlPackages.GetAvailablePackages(Constants.PackageType.Module);
                    if (pkgModule != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageModule), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Provider, out outPath))
                {
                    var pkgProvider = ctlPackages.GetAvailablePackages(Constants.PackageType.Provider);
                    if (pkgProvider != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageProvider), outPath)));
                    }
                }

                if (ctlPackages.HasAvailablePackage(Constants.PackageType.Skin, out outPath))
                {
                    var pkgSkin = ctlPackages.GetAvailablePackages(Constants.PackageType.Skin);
                    if (pkgSkin != null)
                    {
                        messages.Add(new PromptMessage(string.Format(LocalizeString(Constants.LocalizationKeys.PackageFoundMessage), LocalizeString(Constants.LocalizationKeys.PackageSkin), outPath)));
                    }
                }

                return messages;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
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