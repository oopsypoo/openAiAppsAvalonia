using System.IO;
using System.Threading.Tasks;
using System.Windows;
using openAiAppsAvalonia.Services;


namespace openAiAppsAvalonia
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public AppSettings Settings { get; }

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            Settings = settings;
            DataContext = settings;
        }

        /* private void BrowseAppRoot_Click(object sender, RoutedEventArgs e)
         {
             var dialog = new Microsoft.Win32.OpenFileDialog
             {
                 ValidateNames = false,
                 CheckFileExists = false,
                 CheckPathExists = true,
                 FileName = "Select Folder"
             };

             if (dialog.ShowDialog() == true)
             {
                 var folder = Path.GetDirectoryName(dialog.FileName)!;
                 if (Directory.Exists(folder))
                 {
                     AppRootTextBox.Text = folder; // updates Settings.AppRoot via binding
                     UpdateSubPathsFromAppRoot();
                 }
             }
         }*/
        private async void BrowseAppRoot_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Application Root Directory",
                InitialDirectory = Directory.Exists(AppRootTextBox.Text) ? AppRootTextBox.Text : string.Empty
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;

                if (await HasWritePermissionAsync(selectedPath))
                {
                    AppRootTextBox.Text = selectedPath;
                    UpdateSubPathsFromAppRoot();
                }
                else
                {
                    await App.Dialogs.ShowMessageAsync(
                        this,
                        "Access Denied",
                        "You don't have permission to write to this folder. Please choose a different location (like your Documents folder).",
                        DialogSeverity.Warning);
                }
            }
        }

        private async Task<bool> HasWritePermissionAsync(string folderPath)
        {
            string tempFilePath = Path.Combine(folderPath, Path.GetRandomFileName());
            try
            {
                await using (var stream = new FileStream(
                    tempFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1,
                    useAsync: true))
                {
                    await stream.FlushAsync();
                }

                File.Delete(tempFilePath);
                return true;
            }
            catch
            {
                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch
                {
                }

                return false;
            }
        }

        private async void BrowseLogs_Click(object sender, RoutedEventArgs e) => await BrowseFolderAsync(LogsTextBox);
        private async void BrowseImages_Click(object sender, RoutedEventArgs e) => await BrowseFolderAsync(ImagesTextBox);
        /// <summary>
        /// Opens a folder browser dialog to select a folder for the given TextBox. It checks if the application has write permission to the selected folder before updating the TextBox.
        /// </summary>
        /// <param name="textBox"></param>
        private async Task BrowseFolderAsync(System.Windows.Controls.TextBox textBox)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select the save location",
                InitialDirectory = Directory.Exists(textBox.Text) ? textBox.Text : string.Empty
            };

            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;

                if (await HasWritePermissionAsync(folder))
                {
                    textBox.Text = folder;
                }
                else
                {
                    await App.Dialogs.ShowMessageAsync(
                        this,
                        "Permission Denied",
                        "The application does not have permission to write to this folder. Please select a different location.",
                        DialogSeverity.Warning);
                }
            }
        }

        private void UpdateSubPathsFromAppRoot()
        {
            var appRoot = AppRootTextBox.Text;

            // If the user clears the Root, we shouldn't try to build subpaths
            if (string.IsNullOrWhiteSpace(appRoot))
                return;

            // We just update the UI text boxes. 
            // Your EnsureSavePaths() will handle the actual directory creation later.
            LogsTextBox.Text = Path.Combine(appRoot, "logs");
            ImagesTextBox.Text = Path.Combine(appRoot, "images");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(Settings);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}
