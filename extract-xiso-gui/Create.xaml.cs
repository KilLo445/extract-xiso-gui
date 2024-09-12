using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace extract_xiso_gui
{
    public partial class Create : Window
    {
        string rootPath;
        string tempPath;
        string xisoTemp;
        string xisoBat;
        string extractXISO;

        string xisoFolder;
        string createdISO;

        string mainCMD;

        public Create()
        {
            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            xisoTemp = Path.Combine(tempPath, "extract-xiso-gui");
            xisoBat = Path.Combine(xisoTemp, "extract-xiso-gui.bat");
            extractXISO = Path.Combine(rootPath, "extract-xiso.exe");

            InitializeComponent();

            if (!File.Exists(extractXISO))
            {
                MessageBoxResult dlXISO = MessageBox.Show("extract-xiso has not been found.\n\nWould you like to download it?", "extract-xiso not found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlXISO == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                }
                if (dlXISO == MessageBoxResult.No)
                {
                    MessageBox.Show("extract-xiso is required for extract-xiso-gui to run.\n\nReinstalling extract-xiso-gui may fix the issue.", "extract-xiso not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
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
            Microsoft.Win32.SaveFileDialog isoDLG = new Microsoft.Win32.SaveFileDialog();
            isoDLG.FileName = "MyXISO";
            isoDLG.DefaultExt = ".iso";
            isoDLG.Filter = "Disc Images|*.iso";
            Nullable<bool> isoResult = isoDLG.ShowDialog();
            if (isoResult == true)
            {
                createdISO = isoDLG.FileName;
                ISOPath.Text = isoDLG.FileName;
            }
        }

        private void GoBTN_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(createdISO))
            {
                MessageBoxResult isoExists = MessageBox.Show($"{createdISO} already exists.\n\nWould you like to overwrite it?", "ISO Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (isoExists == MessageBoxResult.Yes)
                {
                    try { File.Delete(createdISO); }
                    catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                }
                else { return; }
            }

            mainCMD = $"\"{extractXISO}\" -c \"{xisoFolder}\" \"{createdISO}\"";

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

        private void goBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mWindow = new MainWindow();
            this.Close();
            mWindow.Show();
        }
    }
}
