using openAiAppsAvalonia.Services;
using System.Windows;

namespace openAiAppsAvalonia
{
    public partial class EnvironmentReportWindow : Window
    {
        public EnvironmentReportWindow(EnvironmentCapabilityReport report)
        {
            InitializeComponent();
            ReportTextBox.Text = report?.Text ?? string.Empty;

            bool hasWorkspaceRoot = report?.WorkspaceRoot != null;
            WorkspaceTreeSection.Visibility = hasWorkspaceRoot ? Visibility.Visible : Visibility.Collapsed;
            Height = hasWorkspaceRoot ? 760 : 480;

            if (hasWorkspaceRoot)
                WorkspaceTreeView.Items.Add(report.WorkspaceRoot);
        }
    }
}
