using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using RLParser.Services;
using RLParser.Services.Parsing;
using System.Threading.Tasks;

namespace RLParser.Views;

public partial class ReplaysView : UserControl
{
    private readonly ReplayParser _replayParser = new();
    public ReplaysView()
    {
        InitializeComponent();
    }

}