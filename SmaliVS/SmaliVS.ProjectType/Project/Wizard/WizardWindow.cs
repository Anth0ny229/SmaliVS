using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAdbClient.DeviceCommands;
using SmaliVS.Helpers;

namespace SmaliVS.Project.Wizard
{
    public partial class WizardWindow : Form
    {
        readonly string _projectDir;
        private readonly ListViewColumnSorter _columnSorter;

        public WizardWindow(string projectDirectory)
        {
            _projectDir = projectDirectory;
            InitializeComponent();
            _columnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = _columnSorter;
        }

        public string TextResult => textBox1.Text;

        private async void button1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                HandleLocalApk();
            else
                await HandleRemoteApk();

            Close();
        }

        void HandleLocalApk()
        {
            DialogResult = string.IsNullOrEmpty(textBox1.Text)
                ? DialogResult.OK
                : (DisassembleApk(textBox1.Text) ? DialogResult.OK : DialogResult.Abort);
        }

        private async Task HandleRemoteApk()
        {
            var directoryInfo = new DirectoryInfo(_projectDir).Parent;
            if (directoryInfo == null) throw new Exception("Failed to get solution directory!");
            var localPath = directoryInfo.FullName + "\\base.apk";

            // Get our selected package path
            var remotePath = listView1.SelectedItems[0].Tag as string;
            textBox2.AppendText($"Pulling Package: {remotePath}\r\n\r\n");

            var device = Utilities.ConnectedDevice;
            var progress = new Progress<int>(i => progressBar1.Value = i);
            await device.PullFileAsync(remotePath, localPath, progress);

            // Now disassemble our apk
            DisassembleApk(localPath);
            textBox1.Text = localPath;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Android APK (*apk)|*apk" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ListPackages();
        }

        private void ListPackages()
        {
            // Get our device
            var device = Utilities.ConnectedDevice;
            var pm = new PackageManager(device, !checkBox1.Checked);

            // Create a new list of list view items
            var lvis = pm.Packages.Select(kv => new ListViewItem(kv.Key) { Tag = kv.Value }).ToList();

            listView1.Items.Clear();
            listView1.Items.AddRange(lvis.ToArray());
            listView1.Columns[0].Width = -1;
            listView1.FocusedItem = listView1.Items[0];
        }

        private bool DisassembleApk(string apkPath)
        {
            //java -Xmx%heapy%m -jar apktool.jar d %~dp0place-apk-here-for-modding/%capp% -o %~dp0projects/%capp%
            var p = Utilities.GetApkToolProcess("d \"{0}\" -f -o \"{1}\"", apkPath, _projectDir);
            bool ret;
            if (!(ret = p.Start())) throw new Exception("Failed to start process");

            while (!p.StandardOutput.EndOfStream)
            {
                var str = p.StandardOutput.ReadLine();
                textBox2.AppendText(str + "\r\n");
                Application.DoEvents();
            }

            return ret;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _columnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                _columnSorter.Order = _columnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _columnSorter.SortColumn = e.Column;
                _columnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView1.Sort();
        }
    }
}
