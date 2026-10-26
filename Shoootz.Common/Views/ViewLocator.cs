using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Shoootz.Common.ViewModels;
using Shoootz.Common.ViewModels.Error;

namespace Shoootz.Common.Views;

public class ViewLocator(Assembly appAssembly) : IDataTemplate
{
    public ViewLocator()
        : this(Assembly.GetCallingAssembly()) { }

    public Control? Build(object? param)
    {
        switch (param)
        {
            case null:
                return null;
        }

        Type paramType = param.GetType();

        if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(SettingsErrorViewModel<>))
        {
            return new Error.SettingsErrorView();
        }

        string name = paramType.FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        Type? type = appAssembly.GetType(name);

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
