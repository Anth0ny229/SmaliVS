using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using SmaliVS.Helpers;

namespace SmaliVS.Project.Deployment
{
    [Export(typeof(IDeployProvider))]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    internal class SmaliDeploymentProvider : IDeployProvider
    {
        [Import]
        private ProjectProperties Properties { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not deploy is currently supported.
        /// </summary>
        public bool IsDeploySupported => true;

        /// <summary>
        /// Signals to start the deploy operation.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that will be set if the deploy is cancelled by the user.
        /// This cancellation token should be passed in as the CancellationToken parameter to the task that is returned.</param>
        /// <param name="outputPaneWriter">A TextWriter that will write to the deployment output pane.</param>
        /// <returns>A task that performs the deploy operation.</returns>
        public async Task DeployAsync(CancellationToken cancellationToken, TextWriter outputPaneWriter)
        {
            outputPaneWriter.WriteLine("Get out your popcorn, folks...");

            await Deploy(outputPaneWriter);

            outputPaneWriter.WriteLine("The party's over. Get back to work!");
        }

        private async Task Deploy(TextWriter outputPaneWriter)
        {
            //IServiceProvider site; // the VS global service provider
            //var componentModel = site.GetService(typeof(SComponentModel)) as IComponentModel;
            //var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
            //ProjectService projectService = projectServiceAccessor.GetProjectService();

            var genProperties = await Properties.GetConfigurationGeneralPropertiesAsync();
            //$(MSBuildProjectDirectory)\$(OutputPath)$(OutputName)
            var projDir = await genProperties.MSBuildProjectDirectory.GetValueAsync();
            var outPath = await genProperties.OutputPath.GetValueAsync();
            var outName = await genProperties.OutputName.GetValueAsync();

            // Fix the path
            var apkPath = $"{projDir}\\{outPath}{outName}";
            apkPath = apkPath.Replace('\\', '/');

            //var p = GetAdbProcess("install -r '/sdcard/{0}'", outName);
            var p = Utilities.GetAdbProcess("install -r \"{0}\"", apkPath);
            if (!p.Start()) throw new Exception("Failed to start process");

            //while (!p.StandardError.EndOfStream)
            //    outputPaneWriter.WriteLine(p.StandardError.ReadLine());

            while (!p.StandardOutput.EndOfStream)
            {
                outputPaneWriter.WriteLine(p.StandardOutput.ReadLine());
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Alerts a project that a deployment operation was successful. Called immediately after the project finishes deployment regardless of the result of other projects in the solution.
        /// </summary>
        public void Commit()
        {

        }

        /// <summary>
        /// Alerts a deployment project that a deployment operation has failed. Called immediately after the project fails deployment regardless of the result of other projects in the solution.
        /// </summary>
        public void Rollback()
        {

        }
    }
}