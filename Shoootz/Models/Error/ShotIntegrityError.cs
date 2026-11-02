namespace Shoootz.Models.Error;

internal record ShotIntegrityError(int ShotId, ShotIntegrityErrorType ErrorType);
