using System;
using System.Collections.Generic;
using Shoootz.Models.Udp;

namespace Shoootz.Services.Data;

internal interface IDataProcessor
{
    event EventHandler<UdpRawModel>? RawDataLogged;

    int ErrorParsedShotCounter { get; }

    int ErrorOnSaveShotToDbCounter { get; }

    IReadOnlyList<UdpRawModel> RawLogEntries { get; }

    int SuccessParsedShotCounter { get; }

    int SuccessSavedShotToDbCounter { get; }

    int UdpRawShotCounter { get; }

    int WarmupShotCounter { get; }

    void Start();
}
