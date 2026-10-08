using System.Threading.Tasks;
using System.Windows;

namespace openAiAppsAvalonia.Services
{
    public sealed class WpfDialogService : IDialogService
    {
        public Task ShowMessageAsync(object owner, string title, string message, DialogSeverity severity = DialogSeverity.Information)
        {
            ShowDialog(owner, new PromptDialogWindow(title, message, severity, showConfirmationButtons: false));
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmAsync(object owner, string title, string message)
        {
            var dialog = new PromptDialogWindow(title, message, DialogSeverity.Question, showConfirmationButtons: true);
            return Task.FromResult(ShowDialog(owner, dialog) == true);
        }

        public Task ShowModalAsync(object owner, object dialogWindow)
        {
            ShowDialog(owner, dialogWindow as Window);
            return Task.CompletedTask;
        }

        public Task<bool?> ShowModalForResultAsync(object owner, object dialogWindow)
        {
            return Task.FromResult(ShowDialog(owner, dialogWindow as Window));
        }

        private static bool? ShowDialog(object owner, Window dialog)
        {
            if (dialog == null)
                return null;

            if (owner is Window ownerWindow && !ReferenceEquals(ownerWindow, dialog))
            {
                dialog.Owner = ownerWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else if (Application.Current?.MainWindow is Window mainWindow && !ReferenceEquals(mainWindow, dialog))
            {
                dialog.Owner = mainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return dialog.ShowDialog();
        }
    }
}
