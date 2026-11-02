namespace Shoootz.Models.Error;

internal enum ShotIntegrityErrorType
{
    ChecksumMismatch,
    ChecksumMissing,
    OrphanedChecksum,
}
