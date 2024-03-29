﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace extract_xiso_gui
{
    public partial class MainWindow : Window
    {
        string guiVersion = "1.0.4";
        string onlineVerLink = "https://raw.githubusercontent.com/KilLo445/extract-xiso-gui/master/extract-xiso-gui/version.txt";
        string updateDL = "https://github.com/KilLo445/extract-xiso-gui/releases/latest";
        string updaterDL = "https://github.com/KilLo445/extract-xiso-gui/raw/master/extract-xiso-gui/updater.exe";

        string rootPath;
        string tempPath;
        string xisoTemp;
        string xisoBat;
        string extractXISO;
        string updater;

        string xisoDL = "https://github.com/XboxDev/extract-xiso/releases/latest/download/extract-xiso-win32-release.zip";
        string xisoDLBackup = "https://github.com/KilLo445/extract-xiso-gui/raw/master/extract-xiso-gui/extract-xiso.exe";
        string xisoZip;

        string isoFilename;
        string outputDir;
        string comboBoxSelection;

        string xisoMode;

        string mainCMD;
        string[] finalCMD;

        bool BackupDL;
        bool xisoRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            xisoTemp = Path.Combine(tempPath, "extract-xiso-gui");
            xisoBat = Path.Combine(xisoTemp, "extract-xiso-gui.bat");
            extractXISO = Path.Combine(rootPath, "extract-xiso.exe");

            updater = Path.Combine(xisoTemp, "updater.exe");

            xisoZip = Path.Combine(xisoTemp, "extract-xiso.zip");

            Directory.CreateDirectory(xisoTemp);
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
                            webClient.DownloadFile(new Uri(updaterDL), updater);
                            Process.Start(updater);
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
                    DownloadXISO();
                }
                if (dlXISO == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
        }

        private void DownloadXISO()
        {
            pb.Visibility = Visibility.Visible;

            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadXISOComplete);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(xisoDL), xisoZip);
            }
            catch
            {
                MessageBoxResult dlXISO2 = MessageBox.Show("The download seems to have failed, would you like to try the backup server?", "Download failed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO2 == MessageBoxResult.Yes)
                {
                    DownloadXISOBackup();
                }
                if (dlXISO2 == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
        }

        private void DownloadXISOBackup()
        {
            pb.Visibility = Visibility.Visible;

            try
            {
                BackupDL = true;

                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadXISOComplete);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(xisoDLBackup), extractXISO);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The download failed.\n\nReinstalling extract-xiso-gui may fix the issue.\n\n{ex}", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }

        private void BrowseBTN_Click(object sender, RoutedEventArgs e)
        {
            var isoFileDialog = new Microsoft.Win32.OpenFileDialog();
            isoFileDialog.CheckFileExists = true;
            isoFileDialog.Multiselect = false;
            isoFileDialog.Filter = "Disc Images|*.iso";

            bool? isoResult = isoFileDialog.ShowDialog();

            if (isoResult == true)
            {
                isoFilename = isoFileDialog.FileName;
                ISOPathBox.Text = $"{isoFilename}";
            }
        }

        private void GoBTN_Click(object sender, RoutedEventArgs e)
        {
            CheckForXISO();
            
            comboBoxSelection = ComboBox.Text;

            if (!File.Exists(isoFilename))
            {
                if (isoFilename == null)
                {
                    MessageBox.Show("File does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    MessageBox.Show($"{isoFilename} does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (comboBoxSelection == null || comboBoxSelection == "")
            {
                MessageBox.Show("Please select a mode.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (comboBoxSelection != null)
            {
                if (comboBoxSelection == "List")
                {
                    xisoMode = "l";
                    mainCMD = $"\"{extractXISO}\" -{xisoMode} \"{isoFilename}\"";
                }
                if (comboBoxSelection == "Rewrite")
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

                        MessageBoxResult delISO = MessageBox.Show("Would you like to delete the original ISO after rewrite?", "Delete ISO", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (delISO == MessageBoxResult.Yes)
                        {
                            mainCMD = $"\"{extractXISO}\" -D -d \"{outputDir}\" -r \"{isoFilename}\"";
                        }
                        if (delISO == MessageBoxResult.No)
                        {
                            mainCMD = $"\"{extractXISO}\" -d \"{outputDir}\" -r \"{isoFilename}\"";
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                if (comboBoxSelection == "Extract")
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

                        mainCMD = $"\"{extractXISO}\" -d \"{outputDir}\" \"{isoFilename}\"";
                    }
                    else
                    {
                        return;
                    }
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
                    MessageBoxResult openExtracted = MessageBox.Show("Would you like to open your output directory?", "", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (openExtracted == MessageBoxResult.Yes)
                    {
                        Process.Start(outputDir);
                    }
                }
                Process.Start(xisoBat);
                return;
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        private void DownloadXISOComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (BackupDL == true)
            {
                pb.Visibility = Visibility.Hidden;
                try
                {
                    Directory.Delete(xisoTemp, true);
                    Directory.CreateDirectory(xisoTemp);
                    return;
                }
                catch
                {
                    return;
                }
            }

            try
            {
                ZipFile.ExtractToDirectory(xisoZip, xisoTemp);
            }
            catch
            {
                MessageBoxResult dlXISO = MessageBox.Show("The zip archive seems to be corrupt, this usually means the download failed, would you like to try the backup server?", "Download failed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO == MessageBoxResult.Yes)
                {
                    DownloadXISOBackup();
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
            File.Move(Path.Combine(xisoTemp, "extract-xiso.exe"), extractXISO);
            pb.Visibility = Visibility.Hidden;
            try
            {
                Directory.Delete(xisoTemp, true);
                Directory.CreateDirectory(xisoTemp);
                return;
            }
            catch
            {
                return;
            }
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
