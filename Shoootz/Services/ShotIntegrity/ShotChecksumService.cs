using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shoootz.Models.Error;
using Shoootz.Store;
using Shoootz.Store.Entities;

namespace Shoootz.Services.ShotIntegrity;

internal class ShotChecksumService : IShotChecksumService
{
    public async Task StoreAsync(AppDbContext context, ShotEntity shot)
    {
        string hash = ComputeHash(shot);

        ShotChecksumEntity? entity = await context.ShotChecksums.FindAsync(shot.Id).ConfigureAwait(false);

        if (entity is null)
        {
            context.ShotChecksums.Add(new ShotChecksumEntity { Hash = hash, ShotId = shot.Id });
        }
        else
        {
            entity.Hash = hash;
        }
    }

    public async Task<List<ShotIntegrityError>> VerifyAsync(AppDbContext context)
    {
        List<ShotEntity> shots = await context.Shots.Include(shot => shot.Shooter).ToListAsync().ConfigureAwait(false);

        List<ShotChecksumEntity> checksums = await context.ShotChecksums.ToListAsync().ConfigureAwait(false);

        Dictionary<int, string> hashByShotId = checksums.ToDictionary(
            checksum => checksum.ShotId,
            checksum => checksum.Hash
        );

        HashSet<int> existingShotIds = shots.Select(shot => shot.Id).ToHashSet();

        List<ShotIntegrityError> issues = [];

        foreach (ShotEntity shot in shots)
        {
            if (!hashByShotId.TryGetValue(shot.Id, out string? storedHash))
            {
                issues.Add(new ShotIntegrityError(shot.Id, ShotIntegrityErrorType.ChecksumMissing));
                continue;
            }

            if (!string.Equals(ComputeHash(shot), storedHash, StringComparison.Ordinal))
            {
                issues.Add(new ShotIntegrityError(shot.Id, ShotIntegrityErrorType.ChecksumMismatch));
            }
        }

        foreach (ShotChecksumEntity checksum in checksums)
        {
            if (!existingShotIds.Contains(checksum.ShotId))
            {
                issues.Add(new ShotIntegrityError(checksum.ShotId, ShotIntegrityErrorType.OrphanedChecksum));
            }
        }

        return issues;
    }

    private static string ComputeHash(ShotEntity shot)
    {
        string content = string.Join(
            '|',
            shot.Shooter?.Lastname,
            shot.Shooter?.Firstname,
            shot.DecValue.ToString(CultureInfo.InvariantCulture),
            shot.Distance.ToString(CultureInfo.InvariantCulture),
            shot.ShotDateTime.ToString("O", CultureInfo.InvariantCulture),
            shot.Source
        );

        byte[] secretBytes = Encoding.UTF8.GetBytes(ShotHashSecret.Value);
        byte[] contentBytes = Encoding.UTF8.GetBytes(content);

        using HMACSHA256 hmac = new(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(contentBytes);

        return Convert.ToHexString(hashBytes);
    }
}
