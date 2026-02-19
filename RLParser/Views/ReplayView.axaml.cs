using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace RLParser.Views;

public partial class ReplaysView : UserControl
{
    public ReplaysView()
    {
        InitializeComponent();

        Loaded += ReplaysView_Loaded;
    }

    private void ReplaysView_Loaded(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var filesService = new Services.FilesService(topLevel);
        var viewModel = new ViewModels.ReplaysViewModel(filesService);
        DataContext = viewModel;
        Loaded -= ReplaysView_Loaded;
    }

}