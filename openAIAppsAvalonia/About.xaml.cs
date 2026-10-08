using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace openAiAppsAvalonia
{
    /// <summary>
    /// Displays project purpose and build revision information.
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            tbGitCommit.Text = $"Git commit: {GetShortGitCommit() ?? "Not embedded in this build"}";
        }

        private static string GetShortGitCommit()
        {
            string informationalVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (string.IsNullOrWhiteSpace(informationalVersion))
            {
                return null;
            }

            int metadataSeparator = informationalVersion.IndexOf('+');
            if (metadataSeparator < 0)
            {
                return null;
            }

            string revision = informationalVersion.Substring(metadataSeparator + 1);
            if (revision.Length < 7 || !revision.All(Uri.IsHexDigit))
            {
                return null;
            }

            return revision.Substring(0, 7);
        }
    }
}
