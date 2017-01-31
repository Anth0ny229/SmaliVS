using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using SmaliVS.Helpers;

namespace SmaliVS.Project.Wizard
{
    public enum WizardProjectType
    {
        EmptyProject,
        NewProject,
        ExistingProject
    }

    [XmlRoot("WizardArgs", Namespace = "http://schemas.microsoft.com/developer/vstemplate/2005")]
    public class WizardArgs
    {
        [XmlElement("ProjectType")]
        public WizardProjectType ProjectType { get; set; }
    }

    //public class Step
    //{
    //    [XmlElement("Name")]
    //    public string Name { get; set; }
    //    [XmlElement("Desc")]
    //    public string Desc { get; set; }
    //}

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

            #region Dump Method Params
            #region Example Dump
            /*
                RunStarted Parameters
                   runKind:
                      AsNewProject
                   customParams:
                      [0] = $targetframeworkversion$=4.5.2
                      [1] = C:\Users\anth0\AppData\Local\Microsoft\VisualStudio\14.0Exp\VTC\208dfeb0ee41abb5edf059f58bcedeab\~PC\ProjectTemplates\Smali\1033\NewProject.zip\NewProject.vstemplate
                   replacementsDictionary:
                      $guid2$ = a2a17325-a3c7-4a22-b6e5-0b9d45cf3670
                      $guid5$ = 69259534-117a-4825-bdd3-72f3a8c684f2
                      $guid6$ = 2fa60255-f7fd-4905-a4e6-2095849b5465
                      $guid7$ = 75720264-12a3-4106-8278-41755e3faa15
                      $guid8$ = b46364ca-1012-48ee-a3ce-a8cdb8ac78f4
                      $guid9$ = 2ae8ae1c-cf76-475e-8752-1a7353cf71a7
                      $guid10$ = c6f7e654-eae4-4854-9a3c-b8c699bac3eb
                      $time$ = 1/30/2017 1:18:37 PM
                      $year$ = 2017
                      $username$ = anth0
                      $userdomain$ = DESKTOP-4U5QSM0
                      $machinename$ = DESKTOP-4U5QSM0
                      $clrversion$ = 4.0.30319.42000
                      $registeredorganization$ = 
                      $runsilent$ = False
                      $wizarddata$ = <ProjectType xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">NewProject</ProjectType>
                      $targetframeworkversion$ = 4.5.2
                      $solutiondirectory$ = c:\users\anth0\documents\visual studio 2015\Projects\NewSmaliProject2
                      $projectname$ = NewSmaliProject2
                      $safeprojectname$ = NewSmaliProject2
                      $currentuiculturename$ = en-US
                      $installpath$ = C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\
                      $specifiedsolutionname$ = NewSmaliProject2
                      $exclusiveproject$ = True
                      $destinationdirectory$ = c:\users\anth0\documents\visual studio 2015\Projects\NewSmaliProject2\NewSmaliProject2
                      $guid4$ = b33e1b31-520a-48e4-9719-84eaa340d316
                      $guid1$ = 19df675e-aa60-405a-a057-70c2ab453699
                      $guid3$ = 7d01f3ab-1427-48ce-8546-5c7d678fb70e
            */
            #endregion
#if true
            // Dump all our vars
            var sb1 = new StringBuilder(); int curIdx = 0;
            sb1.AppendLine("RunStarted Parameters");
            sb1.AppendLine("   runKind:\r\n      " + runKind);
            sb1.AppendLine("   customParams:");
            customParams.AsParallel().ForAll(x => sb1.AppendFormat("      [{0}] = {1}\r\n", curIdx++, x));
            sb1.AppendLine("   replacementsDictionary:");
            replacementsDictionary.AsParallel().ForAll(x => sb1.AppendLine($"      {x.Key} = {x.Value}"));
            var stringOutput = sb1.ToString();
            //Clipboard.SetText(stringOutput);
#endif
            #endregion

#if !DEBUG
            try
#endif
            {
                // Get our Wizard data that should have been passed
                var wd = Utilities.DeserializeXml<WizardArgs>(replacementsDictionary["$wizarddata$"]);

                // No matter what the apk tools need to be installed
                if (!Utilities.ApkToolDataExists)
                    if (new FrameworkInstaller().ShowDialog() != DialogResult.OK)
                        throw new Exception("Framework needed!");

                // We need extension directory
                replacementsDictionary["$extensiondirectory$"] = VsPackage.ExtensionDirectory;
                var destDirectory = replacementsDictionary["$destinationdirectory$"];

                // Now handle the type of project we are dealing with
                switch (wd.ProjectType)
                {
                    case WizardProjectType.NewProject:
                        {
                            // Display a form to the user. The form collects
                            _wizardWindow = new WizardWindow(destDirectory);
                            if (_wizardWindow.ShowDialog() != DialogResult.OK)
                                throw new WizardCancelledException("User canceled New Project Wizard");

                            // Set our source APK and include files
                            replacementsDictionary["$sourceapk$"] = _wizardWindow.TextResult;
                            replacementsDictionary["$includefiles$"] = GetSourceFilesFromDirectory(destDirectory, destDirectory, true, true);
                            break;
                        }
                    case WizardProjectType.ExistingProject:
                        {
                            var fbd = new FolderBrowserDialog { Description = "Select existing decoded apk source directory", ShowNewFolderButton = false, SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
                            fbd.SelectedPath = "C:\\Users\\anth0\\Documents\\Visual Studio 2015\\Projects\\SmaliChess\\SmaliChess";
                            if (fbd.ShowDialog() != DialogResult.OK) throw new WizardCancelledException("User canceled finding existing project.");
                            replacementsDictionary["$includefiles$"] = GetSourceFilesFromDirectory(fbd.SelectedPath, destDirectory, includeLink: true);
                            replacementsDictionary["$sourcedirectory$"] = fbd.SelectedPath;
                            break;
                        }
                    case WizardProjectType.EmptyProject:
                        {
                            replacementsDictionary["$includefiles$"] = "";
                            break;
                        }
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
#endif
        }

        private void RecursiveGetIncludeFileList(DirectoryInfo curDir, ref List<string> fileList, string curRelativePath = "", string fileExtension = ".*")
        {
            // Go through and get all our directories
            foreach (var d in curDir.GetDirectories())
                RecursiveGetIncludeFileList(d, ref fileList, curRelativePath + d.Name + "\\", fileExtension);

            // Now list our files using our relative path
            bool isWildExtension = fileExtension == "*.";
            foreach (var f in curDir.GetFiles().Where(f => isWildExtension || f.Extension == fileExtension))
                fileList.Add(curRelativePath + f.Name);
        }

        private string GetSourceFilesFromDirectory(string directory, string projectDirectory, bool useRelativePath = true, bool useWildcardInclude = false, string fileExtension = ".smali", bool includeLink = false)
        {
            // Decide if we need to use a relative or absolute path
            var dirPath = useRelativePath ? Utilities.GetRelativePath(projectDirectory + "\\", directory + "\\") : directory + "\\";

            // Print out our includes now based on our format and weather we want to use relative path
            StringBuilder sb = new StringBuilder();
            if (useWildcardInclude)
            {
                sb.AppendLine($"    <None Include=\"{dirPath}**\\*{fileExtension}\">");
                if (includeLink) sb.AppendLine("      <Link>%(RecursiveDir)%(Filename)%(Extension)</Link>");
                sb.AppendLine("      <SubType>Smali</SubType>");
                sb.AppendLine("    </None>");
            }
            else
            {
                // Get all our .smali files from our directory
                var files = new List<string>();
                RecursiveGetIncludeFileList(new DirectoryInfo(directory), ref files, "", fileExtension);
                foreach (var f in files)
                {
                    sb.AppendLine($"    <None Include=\"{dirPath}{f}\">");
                    if (includeLink) sb.AppendLine($"      <Link>{f}</Link>");
                    sb.AppendLine("      <SubType>Smali</SubType>");
                    sb.AppendLine("    </None>");
                }
            }
            return sb.ToString();
        }

        // This method is only called for item templates,  
        // not for project templates.  
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}