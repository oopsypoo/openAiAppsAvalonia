using System.Windows;
using openAiAppsAvalonia.Services;

namespace openAiAppsAvalonia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IDialogService Dialogs { get; set; } = new WpfDialogService();
    }
}
