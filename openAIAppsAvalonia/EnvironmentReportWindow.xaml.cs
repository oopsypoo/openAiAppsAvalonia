using System.Windows;

namespace openAiAppsAvalonia
{
    public partial class EnvironmentReportWindow : Window
    {
        public EnvironmentReportWindow(string report)
        {
            InitializeComponent();
            ReportTextBox.Text = report ?? string.Empty;
        }
    }
}
