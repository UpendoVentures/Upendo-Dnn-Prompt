
using Upendo.Modules.UpendoPrompt.Entities.Interfaces;

namespace Upendo.Modules.UpendoPrompt.Entities
{
    using System.Collections.Generic;

    public class AvailablePackagesDto : IAvailablePackagesDto
    {
        public string PackageType { get; set; }

        public List<PackageInfoSlimDto> ValidPackages { get; set; }

        public List<string> InvalidPackages { get; set; }
    }
}