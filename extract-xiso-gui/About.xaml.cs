using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace extract_xiso_gui
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            VersionText.Text = "v" + MainWindow.guiVersion;
        }

        private void OpenGitHub(object sender, MouseButtonEventArgs e)
        {
            Process.Start(MainWindow.githubLink);
        }
    }
}
