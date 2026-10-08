using Avalonia.Controls;
using oaiResponsesAvalonia;
using oaiResponsesAvalonia.Data;
using System.Collections.ObjectModel;

namespace oaiResponsesAvalonia.Views;

public partial class MainWindow : Window
{
    public ResponsesPanelState ResponsesState { get; } = new();
    public LogsPanelState LogsState { get; } = new();
    public ObservableCollection<ChatMessage> CurrentChatMessages { get; } = new();
    public ObservableCollection<ResponseAttachmentItem> PendingResponseAttachments { get; } = new();
    public ObservableCollection<MediaFile> ResponsePreviewImages { get; } = new();
    public ObservableCollection<DeveloperToolCallLogItem> DeveloperToolCallLogs { get; } = new();
    public ObservableCollection<LogRowViewModel> LogView { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
    }
}