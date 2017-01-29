using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace SmaliVS.Project.TreeMonikers
{
    public static class AndroidMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("5b06881e-308d-4f73-ae5d-295828fdc174");

        private const int ProjectIcon = 0;

        public static ImageMoniker ProjectIconImageMoniker => new ImageMoniker { Guid = ManifestGuid, Id = ProjectIcon };
    }
}
