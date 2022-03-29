
namespace Upendo.Modules.UpendoPrompt.Entities.Interfaces
{
    using System.Collections.Generic;

    public interface IAvailablePackagesDto
    {
        string PackageType { get; set; }
        List<PackageInfoSlimDto> ValidPackages { get; set; }
        List<string> InvalidPackages { get; set; }
    }
}