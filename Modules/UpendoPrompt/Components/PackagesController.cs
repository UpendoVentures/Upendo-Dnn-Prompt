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

using System.Globalization;
using System.Xml;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade.Internals;
using Upendo.Modules.UpendoPrompt.Entities.Interfaces;

namespace Upendo.Modules.UpendoPrompt.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;
    using Upendo.Modules.UpendoPrompt.Entities;

    public class PackagesController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackagesController));

        public PackagesController()
        {
            // do nothing
        }

        public string GetPackageBackupPath()
        {
            try
            {
                var folderPath = Path.Combine(DotNetNuke.Common.Globals.ApplicationMapPath, DotNetNuke.Services.Installer.Util.BackupInstallPackageFolder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        public List<AvailablePackagesDto> GetAvailablePackages(string packageType)
        {
            var packages = new List<AvailablePackagesDto>();
            string packagePath;
            if (this.HasAvailablePackage(packageType, out packagePath))
            {
                var validpackages = new Dictionary<string, PackageInfo>();
                var invalidPackages = new List<string>();

                foreach (string file in Directory.GetFiles(packagePath))
                {
                    if (file.ToLower().EndsWith(".zip") || file.ToLower().EndsWith(".resources"))
                    {
                        PackageController.ParsePackage(file, packagePath, validpackages, invalidPackages);
                    }
                }

                if (packageType.ToLowerInvariant() == "corelanguagepack")
                {
                    this.GetAvaialableLanguagePacks(validpackages);
                }

                packages.Add(new AvailablePackagesDto()
                {
                    PackageType = packageType,
                    ValidPackages = validpackages.Values.Select(p => new PackageInfoSlimDto(Null.NullInteger, p)).ToList(),
                    InvalidPackages = invalidPackages,
                });
            }
            return packages;
        }

        public bool HasAvailablePackage(string packageType, out string rootPath)
        {
            var type = packageType;
            switch (packageType.ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    type = PackageTypes.AuthSystem.ToString();
                    rootPath = Globals.ApplicationMapPath + "\\Install\\AuthSystem";
                    break;
                case "javascriptlibrary":
                case "javascript_library":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\JavaScriptLibrary";
                    break;
                case "extensionlanguagepack":
                    type = PackageTypes.Language.ToString();
                    rootPath = Globals.ApplicationMapPath + "\\Install\\Language";
                    break;
                case "corelanguagepack":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\Language";
                    return true; // core languages should always marked as have available packages.
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    rootPath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
                    break;
                default:
                    type = string.Empty;
                    rootPath = string.Empty;
                    break;
            }
            /*
             // original code in DNN Platform
            if (!string.IsNullOrEmpty(type) && Directory.Exists(rootPath) &&
                (Directory.GetFiles(rootPath, "*.zip").Length > 0 ||
                 Directory.GetFiles(rootPath, "*.resources").Length > 0))
            {
                return true;
            }
            */

            if (!string.IsNullOrEmpty(type) && Directory.Exists(rootPath) &&
                (Directory.GetFiles(rootPath, "*.zip").Length > 0))
            {
                return true;
            }

            return false;
        }

        private void GetAvaialableLanguagePacks(IDictionary<string, PackageInfo> validPackages)
        {
            try
            {
                StreamReader myResponseReader = UpdateService.GetLanguageList();
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(myResponseReader);
                XmlNodeList languages = xmlDoc.SelectNodes("available/language");

                if (languages != null)
                {
                    var installedPackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack");
                    var installedLanguages = installedPackages.Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID)).ToList();
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = "";
                        string version = "";
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }
                        if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
                        {
                            var myCIintl = new CultureInfo(cultureCode, true);
                            version = version.Insert(4, ".").Insert(2, ".");
                            var package = new PackageInfo { Owner = "DotNetNuke Update Service", Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName };
                            package.Name = myCIintl.NativeName;
                            package.PackageType = "CoreLanguagePack";
                            package.Description = cultureCode;
                            Version ver = null;
                            Version.TryParse(version, out ver);
                            package.Version = ver;

                            if (
                                installedLanguages.Any(
                                    l =>
                                        LocaleController.Instance.GetLocale(l.LanguageID).Code.ToLowerInvariant().Equals(cultureCode.ToLowerInvariant())
                                        && installedPackages.First(p => p.PackageID == l.PackageID).Version >= ver))
                            {
                                continue;
                            }

                            if (validPackages.Values.Any(p => p.Name == package.Name))
                            {
                                var existPackage = validPackages.Values.First(p => p.Name == package.Name);
                                if (package.Version > existPackage.Version)
                                {
                                    existPackage.Version = package.Version;
                                }
                            }
                            else
                            {
                                validPackages.Add(cultureCode, package);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // suppress for now - need to decide what to do when webservice is unreachable
                // throw;
                // same problem happens in InstallWizard.aspx.cs in BindLanguageList method
            }
        }

        #region Helper Methods
        protected void LogError(Exception ex)
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