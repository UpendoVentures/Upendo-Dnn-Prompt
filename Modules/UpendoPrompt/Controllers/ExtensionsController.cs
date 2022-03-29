using System;

namespace Upendo.Modules.UpendoPrompt.Controllers
{
    using System.Collections.Generic;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Upgrade;

    public class ExtensionsController
    {
        internal static string IsPackageInUse(PackageInfo packageInfo, int portalId)
        {
            if (packageInfo.PackageID == Null.NullInteger)
            {
                return string.Empty;
            }

            if ((packageInfo.PackageType.ToUpper() == "MODULE"))
            {
                if (portalId == Null.NullInteger)
                {
                    return GetPackagesInUse(true).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
                else
                {
                    return GetPackagesInUse(false).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
            }
            return string.Empty;
        }

        internal static IDictionary<int, PackageInfo> GetPackagesInUse(bool forHost)
        {
            return PackageController.GetModulePackagesInUse(PortalController.Instance.GetCurrentPortalSettings().PortalId, forHost);
        }

        internal static string UpgradeRedirect(Version version, string packageType, string packageName)
        {
            return Upgrade.UpgradeRedirect(version, packageType, packageName, string.Empty);
        }

        internal static string UpgradeIndicator(Version version, string packageType, string packageName)
        {
            var url = Upgrade.UpgradeIndicator(version, packageType, packageName, string.Empty, false, false); // last 2 params are unused
            if (string.IsNullOrEmpty(url))
            {
                url = string.Concat(Globals.ApplicationPath, "/images/spacer.gif");
            }
            return url;
        }

        internal static string GetPackageIcon(PackageInfo package)
        {
            return "Not Implemented";
        }
    }
}