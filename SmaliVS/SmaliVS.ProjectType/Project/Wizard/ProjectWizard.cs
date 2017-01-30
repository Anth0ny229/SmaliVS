using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using SmaliVS.Helpers;

namespace SmaliVS.Project.Wizard
{
    public class WizardImplementation : IWizard
    {
        private WizardWindow _wizardWindow;

        // This method is called before opening any item that   
        // has the OpenInEditor attribute.  
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        private void RecursiveGetFileList(DirectoryInfo curDir, ref List<string> fileList)
        {
            var dirs = curDir.GetDirectories();
            foreach (var d in dirs)
                RecursiveGetFileList(d, ref fileList);

            var files = curDir.GetFiles();
            foreach (var f in files)
                fileList.Add(f.FullName);
        }

        // This method is only called for item templates,  
        // not for project templates.  
        public void ProjectItemFinishedGenerating(ProjectItem
            projectItem)
        {
        }

        // This method is called after the project is created.  
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            // try
            //  {

            if (!Utilities.ApkToolDataExists)
                if (new FrameworkInstaller().ShowDialog() != DialogResult.OK)
                    throw new Exception("Framework needed!");

            replacementsDictionary.Add("$extensiondirectory$", VsPackage.ExtensionDirectory);
            var destDir = replacementsDictionary["$destinationdirectory$"];

            // Display a form to the user. The form collects
            _wizardWindow = new WizardWindow(destDir);
            if (_wizardWindow.ShowDialog() != DialogResult.OK)
                throw new WizardCancelledException("User cancelled");
            //_wizardWindow.ShowDialog();

            var apkPath = _wizardWindow.TextResult;

            // Add custom parameters.  
            replacementsDictionary.Add("$sourceapk$", apkPath);

            // Replace our project vars depending on what our return was           
            if (!string.IsNullOrEmpty(apkPath))
            {
                List<string> files = new List<string>();
                RecursiveGetFileList(new DirectoryInfo(destDir + "\\smali"), ref files);

                StringBuilder sb = new StringBuilder();
                foreach (var f in files)
                    sb.AppendFormat("    <None Include=\"{0}\">\r\n      <SubType>Smali</SubType>\r\n    </None>\r\n", f.Replace(destDir + "\\", ""));
                replacementsDictionary["$includefiles$"] = sb.ToString();

                var apkFi = new FileInfo(apkPath);
                var apkName = apkFi.Name.Substring(0, apkFi.Name.Length - apkFi.Extension.Length);
                replacementsDictionary.Add("$name$", apkName);
            }
            else
            {
                replacementsDictionary["$includefiles$"] = "";
                replacementsDictionary["$name$"] = "$(MSBuildProjectName)";
            }

            // }
            // catch (Exception ex)
            // {
            //     MessageBox.Show(ex.ToString());
            //  }
        }

        // This method is only called for item templates,  
        // not for project templates.  
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}