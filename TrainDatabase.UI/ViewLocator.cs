using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TrainDatabase.Presentation;

namespace TrainDatabase.UI;

/// <summary>
/// ViewModel-first view location: maps a <c>...ViewModel</c> instance to its <c>...View</c>
/// by naming convention (replacing WPF DataTemplates / ObjectDataProvider).
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "(null)" };
        }

        string viewName = $"TrainDatabase.UI.Views.{data.GetType().Name.Replace("ViewModel", "View", StringComparison.Ordinal)}";
        Type? type = Type.GetType(viewName);
        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = $"View not found: {viewName}" };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
