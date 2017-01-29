using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace SmaliVS.Project.TreeMonikers
{
    public static class SmaliMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("72b420ef-4dac-4bd3-a7e3-4fb91b739973");

        private const int ProjectIcon = 0;

        public static ImageMoniker ProjectIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = ProjectIcon };
    }
}
