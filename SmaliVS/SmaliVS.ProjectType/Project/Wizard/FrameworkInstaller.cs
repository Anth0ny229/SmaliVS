using System;
using System.Linq;
using System.Windows.Forms;
using SmaliVS.Helpers;

namespace SmaliVS.Project.Wizard
{
    public partial class FrameworkInstaller : Form
    {
        public FrameworkInstaller()
        {
            InitializeComponent();
            DialogResult = DialogResult.Abort;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Create our framework data path
            Utilities.CreateApkToolDataPath();

            // Get list of our files
            var device = Utilities.ConnectedDevice;
            var files = device.GetDirectoryListing(Utilities.FrameworkRemoteDirectory);

            // Now lets go through our list of files
            var filesToPull = from f in files where Utilities.FrameworkPackageNames.Contains(f.Path) select f;
            foreach (var f in filesToPull)
            {
                var localPath = Utilities.ApkToolDataPath + "\\" + f;
                var remotePath = Utilities.FrameworkRemoteDirectory + "/" + f;

                textBox1.AppendText($"Pulling file {remotePath}\r\n");
                progressBar1.Value = 0;

                var progress = new Progress<int>(i => progressBar1.Value = i);
                await device.PullFileAsync(remotePath, localPath, progress);

                // Now install our package
                //apktool if framework-res.apk
                textBox1.AppendText($"Installing framework {f}\r\n");
                var p = Utilities.GetApkToolProcess("if \"{0}\"", localPath);
                p.Start();

                // Print out our output
                while (!p.StandardOutput.EndOfStream)
                {
                    textBox1.AppendText(p.StandardOutput.ReadLine() + "\r\n");
                    Application.DoEvents();
                }
                textBox1.AppendText("\r\n");
            }

            textBox1.AppendText("Frameworks installed!\r\n\r\nDone!");
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}