using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RLParser.ViewModels;
using RLParser.Views;
using System.Diagnostics.CodeAnalysis;

namespace RLParser
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param) => param switch
        {
            HomeViewModel => new HomeView(),
            ReplaysViewModel => new ReplaysView(),
            null => null,
            _ => new TextBlock { Text = $"Not Found: {param.GetType().FullName}" }
        };

        public bool Match(object? data) =>
            data is HomeViewModel or ReplaysViewModel;
    }
}
