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
    [ConsoleCommand("delete-packages", Constants.PromptCategory, "PromptDeletePackages")]
    public class DeletePackages : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeletePackages));
        
        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // do nothing
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var recordCount = 0;
                var ctlPackages = new PackagesController();
                var folderPath = ctlPackages.GetPackageBackupPath();
                string[] filePaths = Directory.GetFiles(folderPath, Constants.FOLDER_EXTENSIONPACKAGES, SearchOption.TopDirectoryOnly);

                recordCount = filePaths.Length;

                var processCount = 0; 

                var messages = new List<PromptMessage>();
                foreach (string filePath in filePaths.OrderBy(f => f).ToList())
                {
                    try
                    {
                        File.Delete(filePath);
                        processCount++;
                    }
                    catch (Exception e)
                    {
                        messages.Add(new PromptMessage
                        {
                            Message = string.Format(Constants.LocalizationKeys.PackageDeletionErrorFormat, filePath)
                        });
                        LogError(e);
                    }
                }

                var output = string.Empty;
                output = string.Format(LocalizeString(Constants.LocalizationKeys.PackageDeletionSuccess), processCount, recordCount);

                if (messages.Count > 0)
                {
                    // on or more errors have occurred 
                    messages.Add(new PromptMessage
                    {
                        Message = Constants.LocalizationKeys.ErrorOccurred
                    });

                    return new ConsoleResultModel
                    {
                        Records = recordCount,
                        Data = messages,
                        Output = output, 
                        IsError = true
                    };
                }
                else
                {
                    // Success!  
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