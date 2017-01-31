using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Designers.Properties;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace SmaliVS.Project.PropertyEditors
{
    [Export(typeof(IPropertyPageUIValueEditor))]
    [ExportMetadata("Name", "FilePropertyEditor")]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    internal class FilePropertyEditor : IPropertyPageUIValueEditor
    {
        /// <summary>
        /// Invokes the editor.
        /// </summary>
        /// <param name="serviceProvider">The set of potential services the component can query for, mainly for access back to the host itself.</param>
        /// <param name="ruleProperty">the property being edited</param>
        /// <param name="currentValue">the current value of the property (may be different than property.Value - for example if host UI caches the new values until Apply button)</param>
        /// <returns>The new value.  May be <paramref name="currentValue"/> if no change is intended.</returns>
        public async Task<object> EditValueAsync(IServiceProvider serviceProvider, IProperty ruleProperty, object currentValue)
        {
            // TODO: Provide your own editor implementation
            await Task.Yield();

            string currentString = currentValue as string;

            // Create our dialog using our current file name
            var ofd = new OpenFileDialog { FileName = currentString };

            // Set our filter if we have one
            var meta = ruleProperty.Metadata.Single(x => x.Name.Equals("Filter", StringComparison.CurrentCultureIgnoreCase));
            ofd.Filter = meta == null ? "All files (*.*)|*.*" : meta.Value;

            ofd.ShowDialog();
            return ofd.FileName;
        }
    }
}