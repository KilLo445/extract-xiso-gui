using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace extract_xiso_gui
{
    public partial class MainWindow : Window
    {
        string guiVersion = "1.2.0";
        string onlineVerLink = "https://raw.githubusercontent.com/KilLo445/extract-xiso-gui/master/extract-xiso-gui/version.txt";
        string updateDL = "https://github.com/KilLo445/extract-xiso-gui/releases/latest";

        string rootPath;
        string tempPath;
        string xisoBat;
        string extractXISO;

        string xisoFolder;
        string createdISO;

        string xisoDL = "https://github.com/XboxDev/extract-xiso/releases/latest/download/extract-xiso-win32-release.zip";
        string xisoDLBackup = "https://github.com/KilLo445/extract-xiso-gui/raw/master/extract-xiso-gui/extract-xiso.exe";
        string xisoZip;

        string isoFilename;
        string isoFilename2;
        string outputDir;

        string xisoMode;

        string mainCMD;
        string[] finalCMD;

        bool BackupDL;
        bool xisoRunning = false;
        bool isCreate = false;
        bool isBatch = false;

        string delISO = "";
        string disXBE = "";
        string skipUpdate = "";

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.Combine(Path.GetTempPath(), "extract-xiso-gui");
            xisoBat = Path.Combine(tempPath, "extract-xiso-gui.bat");
            extractXISO = Path.Combine(rootPath, "extract-xiso.exe");

            xisoZip = Path.Combine(tempPath, "extract-xiso.zip");

            Directory.CreateDirectory(tempPath);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RegistryKey keyA = Registry.CurrentUser.OpenSubKey(@"Software\KilLo\extract-xiso-gui", true);

            if (keyA == null)
            {
                RegistryKey keyB = Registry.CurrentUser.OpenSubKey(@"Software", true);
                keyB.CreateSubKey("KilLo");
                keyB.Close();

                RegistryKey keyC = Registry.CurrentUser.OpenSubKey(@"Software\KilLo", true);
                keyC.CreateSubKey("extract-xiso-gui");
                keyC.Close();

                DumpPath();
            }
            else if (keyA != null)
            {
                DumpPath();
            }

            CheckForUpdates();
            CheckForXISO();
        }

        private void DumpPath()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\KilLo\extract-xiso-gui", true);
            key.SetValue("InstallPath", $"{rootPath}");
            key.Close();

            return;
        }

        private void CheckForUpdates()
        {
            Version localVersion = new Version(guiVersion);

            try
            {
                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString(onlineVerLink));

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    MessageBoxResult updateGUI = MessageBox.Show("An update for extract-xiso-gui has been found.\n\nWould you like to download it?", "Update found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (updateGUI == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(updateDL);
                            Application.Current.Shutdown();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}");
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        private void CheckForXISO()
        {
            if (!File.Exists(extractXISO))
            {
                MessageBoxResult dlXISO = MessageBox.Show("extract-xiso has not been found.\n\nWould you like to download it?", "extract-xiso not found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO == MessageBoxResult.Yes)
                {
                    DownloadXISO(false);
                }
                if (dlXISO == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "extract-xiso not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
        }

        private void DownloadXISO(bool backup)
        {
            pb.Visibility = Visibility.Visible;

            try
            {
                if (!backup)
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadXISOComplete);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(xisoDL), xisoZip);
                }
                if (backup)
                {
                    BackupDL = true;
                    WebClient webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadXISOComplete);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(xisoDLBackup), extractXISO);
                }
            }
            catch
            {
                MessageBoxResult dlXISO2 = MessageBox.Show("The download seems to have failed, would you like to try the backup server?", "Download failed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO2 == MessageBoxResult.Yes)
                {
                    DownloadXISO(true);
                    return;
                }
                if (dlXISO2 == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DelOldISO.IsChecked = false;
            DisAutoXBE.IsChecked = false;
            SkipSysUpdate.IsChecked = false;
            GoBTN.IsEnabled = true;
            delISO = "";
            disXBE = "";
            skipUpdate = "";
        }

        private void ComboBoxItem_Create_Selected(object sender, RoutedEventArgs e)
        {
            isCreate = true;
            ISOSelect.IsEnabled = true;
            FolderSelect.IsEnabled = true;
            DelOldISO.IsEnabled = false;
            DisAutoXBE.IsEnabled = true;
            SkipSysUpdate.IsEnabled = true;
        }

        private void ComboBoxItem_List_Selected(object sender, RoutedEventArgs e)
        {
            isCreate = false;
            ISOSelect.IsEnabled = true;
            FolderSelect.IsEnabled = false;
            DelOldISO.IsEnabled = false;
            DisAutoXBE.IsEnabled = false;
            SkipSysUpdate.IsEnabled = false;
        }

        private void ComboBoxItem_Rewrite_Selected(object sender, RoutedEventArgs e)
        {
            isCreate = false;
            ISOSelect.IsEnabled = true;
            FolderSelect.IsEnabled = false;
            DelOldISO.IsEnabled = true;
            DisAutoXBE.IsEnabled = true;
            SkipSysUpdate.IsEnabled = true;
        }

        private void ComboBoxItem_Extract_Selected(object sender, RoutedEventArgs e)
        {
            isCreate = false;
            ISOSelect.IsEnabled = true;
            FolderSelect.IsEnabled = false;
            DelOldISO.IsEnabled = false;
            DisAutoXBE.IsEnabled = false;
            SkipSysUpdate.IsEnabled = true;
        }

        private void fBrowseBTN_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folderDLG = new WinForms.FolderBrowserDialog();
            folderDLG.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
            folderDLG.ShowNewFolderButton = false;
            WinForms.DialogResult folderResult = folderDLG.ShowDialog();
            if (folderResult == WinForms.DialogResult.OK)
            {
                xisoFolder = folderDLG.SelectedPath;
                FolderPath.Text = Path.Combine(xisoFolder);
            }
        }

        private void iBrowseBTN_Click(object sender, RoutedEventArgs e)
        {
            if (isCreate)
            {
                Microsoft.Win32.SaveFileDialog isoDLGc = new Microsoft.Win32.SaveFileDialog();
                isoDLGc.FileName = "MyXISO";
                isoDLGc.DefaultExt = ".iso";
                isoDLGc.Filter = "Disc Images|*.iso";
                Nullable<bool> isoResultc = isoDLGc.ShowDialog();
                if (isoResultc == true)
                {
                    createdISO = isoDLGc.FileName;
                    ISOPath.Text = isoDLGc.FileName;
                }
            }
            else
            {
                var isoFileDialog = new Microsoft.Win32.OpenFileDialog();
                isoFileDialog.CheckFileExists = true;
                if (ComboBox.Text == "Rewrite") { isoFileDialog.Multiselect = true; }
                else { isoFileDialog.Multiselect = false; }
                isoFileDialog.Filter = "Disc Images|*.iso";
                bool? isoResult = isoFileDialog.ShowDialog();
                if (isoResult == true)
                {
                    if (isoFileDialog.Multiselect == true)
                    {
                        isBatch = true;
                        isoFilename = null;
                        foreach (String file in isoFileDialog.FileNames)
                        {
                            isoFilename = isoFilename + " " + $"\"{file}\"";
                            isoFilename2 = isoFilename2 + " " + file;
                        }
                        ISOPath.Text = $"{isoFilename2}";
                    }
                    else
                    {
                        isBatch = false;
                        isoFilename = isoFileDialog.FileName;
                        ISOPath.Text = $"{isoFilename}";
                    }
                }
            }
        }

        private void GoBTN_Click(object sender, RoutedEventArgs e)
        {
            CheckForXISO();

            if (ComboBox.Text == "Create")
            {
                if (!Directory.Exists(xisoFolder)) { MessageBox.Show("That folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                if (createdISO == null) { MessageBox.Show("That is not a valid ISO path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

                try
                {
                    if (File.Exists(createdISO))
                    {
                        MessageBoxResult isoExists = MessageBox.Show($"{createdISO} already exists.\n\nWould you like to overwrite it?", "ISO Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (isoExists == MessageBoxResult.Yes)
                        {
                            try { File.Delete(createdISO); }
                            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        else { MessageBox.Show("Creating canceled.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                    }

                    if (DisAutoXBE.IsChecked == true) { delISO = "-m"; }
                    if (SkipSysUpdate.IsChecked == true) { delISO = "-s"; }

                    mainCMD = $"\"{extractXISO}\" {disXBE} {skipUpdate} -c \"{xisoFolder}\" \"{createdISO}\"";

                    string[] finalCMD ={
                            "@echo off",
                            "title extract-xiso",
                            $"{mainCMD}",
                            "pause",
                            "exit"
                          };

                    File.WriteAllLines(xisoBat, finalCMD);
                    Process.Start(xisoBat);
                    MessageBoxResult openExtracted = MessageBox.Show("Would you like to open your output directory?", "", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (openExtracted == MessageBoxResult.Yes) { Process.Start(Path.GetDirectoryName(createdISO)); }
                    return;
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }

            if (!File.Exists(isoFilename) && isBatch == false)
            {
                MessageBox.Show("That file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ComboBox.Text != null)
            {
                if (ComboBox.Text == "List")
                {
                    xisoMode = "l";
                    mainCMD = $"\"{extractXISO}\" -{xisoMode} \"{isoFilename}\"";
                }
                if (ComboBox.Text == "Rewrite")
                {
                    xisoMode = "r";

                    WinForms.FolderBrowserDialog extractOutputDirDialog = new WinForms.FolderBrowserDialog();
                    extractOutputDirDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    extractOutputDirDialog.Description = "Please select your output folder.";
                    extractOutputDirDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult outputResult = extractOutputDirDialog.ShowDialog();

                    if (outputResult == WinForms.DialogResult.OK)
                    {
                        outputDir = Path.Combine(extractOutputDirDialog.SelectedPath);
                        Directory.CreateDirectory(outputDir);

                        if (DelOldISO.IsChecked == true) { delISO = "-D"; }
                        if (DisAutoXBE.IsChecked == true) { disXBE = "-m"; }
                        if (SkipSysUpdate.IsChecked == true) { skipUpdate = "-s"; }

                        mainCMD = $"\"{extractXISO}\" {delISO} {disXBE} {skipUpdate} -d \"{outputDir}\" -r {isoFilename}";
                    }
                    else { return; }
                }

                if (ComboBox.Text == "Extract")
                {
                    xisoMode = "x";

                    WinForms.FolderBrowserDialog extractOutputDirDialog = new WinForms.FolderBrowserDialog();
                    extractOutputDirDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    extractOutputDirDialog.Description = "Please select your output folder.";
                    extractOutputDirDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult outputResult = extractOutputDirDialog.ShowDialog();

                    if (outputResult == WinForms.DialogResult.OK)
                    {
                        outputDir = Path.Combine(extractOutputDirDialog.SelectedPath);
                        Directory.CreateDirectory(outputDir);

                        if (SkipSysUpdate.IsChecked == true) { delISO = "-s"; }

                        mainCMD = $"\"{extractXISO}\" {skipUpdate} -d \"{outputDir}\" \"{isoFilename}\"";
                    }
                    else { return; }
                }

                string[] finalCMD ={
                            "@echo off",
                            "title extract-xiso",
                            $"{mainCMD}",
                            "pause",
                            "exit"
                          };

                File.WriteAllLines(xisoBat, finalCMD);
                if (xisoMode == "r" || xisoMode == "x")
                {
                    MessageBoxResult openExtracted = MessageBox.Show("Would you like to open your output directory?", "extract-xiso-gui", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (openExtracted == MessageBoxResult.Yes) { Process.Start(outputDir); }
                }
                Process.Start(xisoBat);
                return;
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) { pb.Value = e.ProgressPercentage; }

        private void DownloadXISOComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (BackupDL == true)
            {
                pb.Visibility = Visibility.Hidden;
                try
                {
                    Directory.Delete(tempPath, true);
                    Directory.CreateDirectory(tempPath);
                    return;
                }
                catch { return; }
            }

            try
            {
                ZipFile.ExtractToDirectory(xisoZip, tempPath);
            }
            catch
            {
                MessageBoxResult dlXISO = MessageBox.Show("The zip archive seems to be corrupt, this usually means the download failed, would you like to try the backup server?", "Download failed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO == MessageBoxResult.Yes)
                {
                    DownloadXISO(true);
                    MessageBox.Show("Download completed sucsesfully!", "extract-xiso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (dlXISO == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                    return;
                }
            }
            File.Delete(xisoZip);
            File.Move(Path.Combine(tempPath, "extract-xiso.exe"), extractXISO);
            pb.Visibility = Visibility.Hidden;
            MessageBox.Show("Download completed sucsesfully!", "extract-xiso", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
                return;
            }
            catch { return; }
        }

        private void openGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/KilLo445/extract-xiso-gui");
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}
