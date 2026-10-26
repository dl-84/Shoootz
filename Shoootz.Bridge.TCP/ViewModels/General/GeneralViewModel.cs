using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Bridge.TCP.Services.Bridge;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels;

namespace Shoootz.Bridge.TCP.ViewModels.General;

internal partial class GeneralViewModel : ViewModelBase, IDisposable
{
    public GeneralViewModel(IBridgeService bridgeService)
    {
        Bridge = bridgeService;
        Bridge.PropertyChanged += OnBridgePropertyChanged;
    }

    public event Action<string>? ErrorOccurred;

    public IBridgeService Bridge { get; }

    public string LocalIpAddress { get; } = GetLocalIpAddress();

    public void Dispose()
    {
        Bridge.PropertyChanged -= OnBridgePropertyChanged;
    }

    private static string GetLocalIpAddress()
    {
        IPAddress? address = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(networkInterface =>
                networkInterface.OperationalStatus == OperationalStatus.Up
                && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback
            )
            .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
            .Select(unicastAddress => unicastAddress.Address)
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        return address?.ToString() ?? LocalizationService.Instance[nameof(Messages.NotAvailable)];
    }

    private void OnBridgePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IBridgeService.LastError) && Bridge.LastError is { Length: > 0 } message)
        {
            ErrorOccurred?.Invoke(message);
        }
    }

    [RelayCommand]
    private void Start()
    {
        Bridge.Start();
    }

    [RelayCommand]
    private void Stop()
    {
        Bridge.Stop();
    }
}
