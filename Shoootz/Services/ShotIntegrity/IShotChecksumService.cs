using System.Collections.Generic;
using System.Threading.Tasks;
using Shoootz.Models.Error;
using Shoootz.Store;
using Shoootz.Store.Entities;

namespace Shoootz.Services.ShotIntegrity;

internal interface IShotChecksumService
{
    Task StoreAsync(AppDbContext context, ShotEntity shot);

    Task<List<ShotIntegrityError>> VerifyAsync(AppDbContext context);
}
