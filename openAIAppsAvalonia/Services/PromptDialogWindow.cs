using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace openAiAppsAvalonia.Services
{
    internal sealed class PromptDialogWindow : Window
    {
        public PromptDialogWindow(string title, string message, DialogSeverity severity, bool showConfirmationButtons)
        {
            Title = title;
            Width = 440;
            MinWidth = 340;
            MaxWidth = 640;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;

            var content = new StackPanel { Margin = new Thickness(20) };
            content.Children.Add(new TextBlock
            {
                Text = severity.ToString(),
                FontWeight = FontWeights.SemiBold,
                Foreground = GetSeverityBrush(severity),
                Margin = new Thickness(0, 0, 0, 8)
            });
            content.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 600,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            if (showConfirmationButtons)
            {
                var yesButton = new Button { Content = "Yes", MinWidth = 80, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
                yesButton.Click += (_, _) => DialogResult = true;
                buttons.Children.Add(yesButton);

                var noButton = new Button { Content = "No", MinWidth = 80, IsCancel = true };
                noButton.Click += (_, _) => DialogResult = false;
                buttons.Children.Add(noButton);
            }
            else
            {
                var okButton = new Button { Content = "OK", MinWidth = 80, IsDefault = true, IsCancel = true };
                okButton.Click += (_, _) => Close();
                buttons.Children.Add(okButton);
            }

            content.Children.Add(buttons);
            Content = content;
        }

        private static Brush GetSeverityBrush(DialogSeverity severity)
        {
            return severity switch
            {
                DialogSeverity.Warning => Brushes.DarkOrange,
                DialogSeverity.Error => Brushes.Firebrick,
                DialogSeverity.Question => Brushes.SteelBlue,
                _ => Brushes.DodgerBlue
            };
        }
    }
}
