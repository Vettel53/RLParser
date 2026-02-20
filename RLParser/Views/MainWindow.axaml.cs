using Avalonia.Controls;
using RLParser.Services;
using RLParser.ViewModels;

namespace RLParser.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var filesService = new FilesService(this);
            var viewModel = new MainWindowViewModel(filesService);
            DataContext = viewModel;
        }
    }
}