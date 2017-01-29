using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpAdbClient;

namespace SmaliVS.Helpers
{
    public static class Utilities
    {
        public static string[] FrameworkPackageNames =
        {
            "framework-res.apk", "twframework-res.apk", "com.htc.resources.apk", "SemcGenericUxpRes.apk", "SystemUI.apk", "lidroid-res.apk", "mediatek-res.apk", "framework-miui.apk"
        };
        public const string FrameworkRemoteDirectory = "/system/framework";
        public static string ApkToolDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\apktool";
        public static bool ApkToolDataExists => Directory.Exists(ApkToolDataPath);
        public static void CreateApkToolDataPath() { Directory.CreateDirectory(ApkToolDataPath); }
        public static string ExtensionToolsDirectory => VsPackage.ExtensionToolsDirectory;
        public static string AdbPath => ExtensionToolsDirectory + "\\adb.exe";

        private static AdbServer _adbServer;
        public static AdbServer AdbServer => _adbServer ?? (_adbServer = new AdbServer());

        public static StartServerResult StartAdbServer(bool restartServerIfNewer = true) { return AdbServer.StartServer(AdbPath, restartServerIfNewer); }

        public static Process GetToolProcess(string processName, string argumentsFormat, params object[] args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = ExtensionToolsDirectory,
                FileName = ExtensionToolsDirectory + "\\" + processName,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,

                Arguments = string.Format(argumentsFormat, args)
            };
            return new Process { StartInfo = processInfo };
        }

        public static Process GetAdbProcess(string argumentsFormat, params object[] args)
        {
            return GetToolProcess("adb.exe", argumentsFormat, args);
        }

        public static Process GetApkToolProcess(string argumentsFormat, params object[] args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "java.exe",
                WorkingDirectory = ExtensionToolsDirectory,
                Arguments = " -Xmx1024m -jar apktool.jar " + string.Format(argumentsFormat, args),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            return new Process { StartInfo = processInfo };
        }

        public static IEnumerable<FileStatistics> GetDirectoryListing(this DeviceData device, string directory)
        {
            using (var syn = new SyncService(device))
                return syn.GetDirectoryListing(directory);
        }

        public static void PullFile(this DeviceData device, string remotePath, string localPath, IProgress<int> progress = null)
        {
            PullFile(device, remotePath, localPath, CancellationToken.None, progress);
        }

        public static void PullFile(this DeviceData device, string remotePath, string localPath, CancellationToken cancellationToken, IProgress<int> progress = null)
        {
            using (var syn = new SyncService(device))
            using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                syn.Pull(remotePath, fs, progress, cancellationToken);
        }

        public static async Task PullFileAsync(this DeviceData device, string remotePath, string localPath, IProgress<int> progress = null)
        {
            await Task.Run(() => PullFile(device, remotePath, localPath, progress));
        }

        public static async Task PullFileAsync(this DeviceData device, string remotePath, string localPath, CancellationToken cancellationToken, IProgress<int> progress = null)
        {
            await Task.Run(() => PullFile(device, remotePath, localPath, cancellationToken, progress));
        }

        public static DeviceData ConnectedDevice => GetConnectedDevice();
        public static DeviceData GetConnectedDevice()
        {
            StartAdbServer();
            return GetAdbClient().GetDevices().First();
        }

        public static IAdbClient GetAdbClient()
        {
            StartAdbServer();
            return AdbClient.Instance;
        }
    }
}
