using System;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.ComponentModel;
using WinForms = System.Windows.Forms;
using System.Threading;

namespace extract_xiso_gui
{
    enum SelectedMode
    {
        none,
        create,
        rewrite,
        list,
        extract
    }


    public partial class MainWindow : Window
    {
        // Info + links
        public static string guiVersion = "2.0.0";
        public static string githubLink = "https://github.com/KilLo445/extract-xiso-gui";
        string verLink = "https://raw.githubusercontent.com/KilLo445/extract-xiso-gui/master/extract-xiso-gui/version.txt";
        string xisoDL = "https://github.com/KilLo445/extract-xiso-gui/raw/master/extract-xiso-gui/extract-xiso.exe";

        // Paths and files
        string rootPath;
        string xisoBat;
        string eXISO;

        string selectedInput;
        string selectedOutput;

        // Bools
        bool suppressUpdates = false;
        bool isBatch = false;

        private SelectedMode _status;
        internal SelectedMode Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case SelectedMode.none:
                        cbDelISO.IsEnabled = false; cbDelISO.Opacity = 0.2;
                        cbAutoXBE.IsEnabled = false; cbAutoXBE.Opacity = 0.2;
                        cbSkipSys.IsEnabled = false; cbSkipSys.Opacity = 0.2;

                        InputStack.IsEnabled = false; InputBrowse.Opacity = 0.2;
                        OutputStack.IsEnabled = false; OutputBrowse.Opacity = 0.2;

                        GoBTN.IsEnabled = false; GoBTN.Opacity = 0.2;
                        break;
                    case SelectedMode.create:
                        cbDelISO.IsEnabled = false; cbDelISO.Opacity = 0.2;
                        cbAutoXBE.IsEnabled = true; cbAutoXBE.Opacity = 1;
                        cbSkipSys.IsEnabled = false; cbSkipSys.Opacity = 0.2;

                        InputStack.IsEnabled = true; InputBrowse.Opacity = 1;
                        OutputStack.IsEnabled = true; OutputBrowse.Opacity = 1;

                        GoBTN.IsEnabled = true; GoBTN.Opacity = 1;
                        break;
                    case SelectedMode.list:
                        cbDelISO.IsEnabled = false; cbDelISO.Opacity = 0.2;
                        cbAutoXBE.IsEnabled = false; cbAutoXBE.Opacity = 0.2;
                        cbSkipSys.IsEnabled = false; cbSkipSys.Opacity = 0.2;

                        InputStack.IsEnabled = true; InputBrowse.Opacity = 1;
                        OutputStack.IsEnabled = false; OutputBrowse.Opacity = 0.2;

                        GoBTN.IsEnabled = true; GoBTN.Opacity = 1;
                        break;
                    case SelectedMode.rewrite:
                        cbDelISO.IsEnabled = true; cbDelISO.Opacity = 1;
                        cbAutoXBE.IsEnabled = true; cbAutoXBE.Opacity = 1;
                        cbSkipSys.IsEnabled = true; cbSkipSys.Opacity = 1;

                        InputStack.IsEnabled = true; InputBrowse.Opacity = 1;
                        OutputStack.IsEnabled = true; OutputBrowse.Opacity = 1;

                        GoBTN.IsEnabled = true; GoBTN.Opacity = 1;
                        break;
                    case SelectedMode.extract:
                        cbDelISO.IsEnabled = false; cbDelISO.Opacity = 0.2;
                        cbAutoXBE.IsEnabled = false; cbAutoXBE.Opacity = 0.2;
                        cbSkipSys.IsEnabled = true; cbSkipSys.Opacity = 1;

                        InputStack.IsEnabled = true; InputBrowse.Opacity = 1;
                        OutputStack.IsEnabled = true; OutputBrowse.Opacity = 1;

                        GoBTN.IsEnabled = true; GoBTN.Opacity = 1;
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            xisoBat = Path.Combine(Path.GetTempPath(), "extract-xiso-gui", "extract-xiso-gui.bat");
            eXISO = Path.Combine(rootPath, "extract-xiso.exe");

            if (File.Exists(Path.Combine(rootPath, "suppress-updates.txt"))) { suppressUpdates = true; }

            Status = SelectedMode.none;

            CreateReg();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!suppressUpdates) { CheckForUpdates(); }
            CheckForXISO();
        }

        private void CreateReg()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            key = key.CreateSubKey(@"KilLo\extract-xiso-gui");
            key.SetValue("Version", $"{guiVersion}");
            key.Close();
        }

        private void CheckForUpdates()
        {
            Version localVersion = new Version(guiVersion);

            try
            {
                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString(verLink));

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    MessageBoxResult updateGUI = MessageBox.Show("An update for extract-xiso-gui has been found.\n\nWould you like to download it?", "Update available!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (updateGUI == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(githubLink + "/releases/latest");
                            Application.Current.Shutdown();
                        }
                        catch (Exception ex) { DisplayErrorMessage(ex); }
                    }
                }

                return;
            }
            catch (Exception ex) { DisplayErrorMessage(ex); }
        }

        private void CheckForXISO()
        {
            if (!File.Exists(eXISO))
            {
                MessageBoxResult dlXISO = MessageBox.Show("extract-xiso has not been found.\n\nWould you like to download it?", "extract-xiso not found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO == MessageBoxResult.Yes)
                {
                    pb.Visibility = Visibility.Visible;
                    try
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(dlComplete);
                        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        webClient.DownloadFileAsync(new Uri(xisoDL), eXISO);
                        return;
                    }
                    catch (Exception ex) { MessageBox.Show("Something went wrong, reinstalling extract-xiso-gui may fix the issue.", "Download failed!", MessageBoxButton.OK, MessageBoxImage.Error); DisplayErrorMessage(ex); Application.Current.Shutdown(); }
                }
                if (dlXISO == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "extract-xiso not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
        }

        private void dlComplete(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("extract-xiso has been downloaded.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            pb.Visibility = Visibility.Hidden;
            return;
        }

        private void SelectedMode_Changed(object sender, RoutedEventArgs e)
        {
            if (rbCreate.IsChecked == true) { Status = SelectedMode.create; }
            if (rbList.IsChecked == true) { Status = SelectedMode.list; }
            if (rbRewrite.IsChecked == true) { Status = SelectedMode.rewrite; }
            if (rbExtract.IsChecked == true) { Status = SelectedMode.extract; }
        }

        private void InputBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (Status == SelectedMode.none) { MessageBox.Show("Please select a mode.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            if (Status == SelectedMode.create) { BrowseForFolder(true); }
            if (Status == SelectedMode.list) { BrowseForISO(true); }
            if (Status == SelectedMode.rewrite) { BrowseForISO(true); }
            if (Status == SelectedMode.extract) { BrowseForISO(true); }
        }

        private void OutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (Status == SelectedMode.none) { MessageBox.Show("Please select a mode.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            if (Status == SelectedMode.create) { SaveISO(false); }
            if (Status == SelectedMode.rewrite) { BrowseForFolder(false); }
            if (Status == SelectedMode.extract) { BrowseForFolder(false); }
        }

        private void GoBTN_Click(object sender, RoutedEventArgs e)
        {
            if (selectedInput == null) { MessageBox.Show("Please select an input.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (selectedOutput == null) { MessageBox.Show("Please select an output.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            Directory.Delete(Path.Combine(Path.GetTempPath(), "extract-xiso-gui"), true);
            string delISO = "";
            string disXBE = "";
            string skipSys = "";
            if (cbDelISO.IsChecked == true) { delISO = "-D"; }
            if (cbAutoXBE.IsChecked == true) { disXBE = "-m"; }
            if (cbSkipSys.IsChecked == true) { skipSys = "-s"; }

            if (Status == SelectedMode.none) { MessageBox.Show("Please select a mode.", "extract-xiso-gui", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            if (Status == SelectedMode.create) { RunXISO($"\"{eXISO}\"" + $" {disXBE}" + $" -c \"{selectedInput}\" \"{selectedOutput}\""); }
            if (Status == SelectedMode.list) { RunXISO($"\"{eXISO}\" -l \"{selectedInput}\""); }
            if (Status == SelectedMode.rewrite) { RunXISO($"\"{eXISO}\"" + $" {delISO} {disXBE} {skipSys}" + $" -d \"{selectedOutput}\" " + $" -r {selectedInput}"); }
            if (Status == SelectedMode.extract) { string iso = Path.GetFileNameWithoutExtension(selectedInput); RunXISO($"\"{eXISO}\"" + $" {skipSys}" + $" -d \"{selectedOutput}\\{iso}\" " + $" -x \"{selectedInput}\""); }
        }

        private void BrowseForISO(bool isInput)
        {
            var isoDLG = new Microsoft.Win32.OpenFileDialog();
            isoDLG.CheckFileExists = true;
            if (Status == SelectedMode.rewrite) { isoDLG.Multiselect = true; }
            else { isoDLG.Multiselect = false; }
            isoDLG.Filter = "Disc Images|*.iso";
            bool? isoResult = isoDLG.ShowDialog();
            if (isoResult == true)
            {
                if (isoDLG.Multiselect == true)
                {
                    isBatch = true;
                    string iso = null;
                    string isoClean = null;
                    foreach (String file in isoDLG.FileNames)
                    {
                        iso = iso + " " + $"\"{file}\"";
                        isoClean = isoClean + " " + $"{file}";
                    }
                    InputPath.Text = isoClean; selectedInput = iso;
                }
                else
                {
                    isBatch = false;
                    if (isInput) { InputPath.Text = isoDLG.FileName; selectedInput = isoDLG.FileName; }
                    else { OutputPath.Text = isoDLG.FileName; selectedOutput = isoDLG.FileName; }
                }
            }
        }

        private void SaveISO(bool isInput)
        {
            Microsoft.Win32.SaveFileDialog isoDLGs = new Microsoft.Win32.SaveFileDialog();
            isoDLGs.FileName = "MyXISO";
            isoDLGs.DefaultExt = ".iso";
            isoDLGs.Filter = "Disc Images|*.iso";
            Nullable<bool> isoResultS = isoDLGs.ShowDialog();
            if (isoResultS == true)
            {
                if (isInput) { InputPath.Text = isoDLGs.FileName; selectedInput = isoDLGs.FileName; }
                else { OutputPath.Text = isoDLGs.FileName; selectedOutput = isoDLGs.FileName; }
            }
        }

        private void BrowseForFolder(bool isInput)
        {
            WinForms.FolderBrowserDialog folderDLG = new WinForms.FolderBrowserDialog();
            folderDLG.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
            if (!isInput) { folderDLG.Description = "Please select your output folder."; folderDLG.ShowNewFolderButton = true; }
            else { folderDLG.ShowNewFolderButton = true; }
            WinForms.DialogResult folderResult = folderDLG.ShowDialog();
            if (folderResult == WinForms.DialogResult.OK)
            {
                if (isInput) { InputPath.Text = folderDLG.SelectedPath; selectedInput = folderDLG.SelectedPath; }
                else { OutputPath.Text = folderDLG.SelectedPath; selectedOutput = folderDLG.SelectedPath; }
            }
        }

        private void RunXISO(string cmd)
        {
            string[] finalCMD ={
                            "@echo off",
                            "title extract-xiso",
                            $"{cmd}",
                            "pause",
                            "exit"
                          };
            File.WriteAllLines(xisoBat, finalCMD);
            if (Status == SelectedMode.create || Status == SelectedMode.rewrite || Status == SelectedMode.extract)
            {
                MessageBoxResult openOutput = MessageBox.Show("Would you like to open your output directory?", "extract-xiso-gui", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (openOutput == MessageBoxResult.Yes) { Process.Start(selectedOutput); }
            }
            Process.Start(xisoBat);
            return;
        }

        private void OpenAbout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DisplayErrorMessage(Exception ex)
        {
            MessageBox.Show($"{ex}", "An error occured!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) { pb.Value = e.ProgressPercentage; }

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
