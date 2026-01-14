namespace SportsCenter.Domain.Entities.Enums;

public enum CancellationResult
{
    Success = 1,
    NotFound = 2,
    AlreadyCancelled = 3,
    TooLateToCancel = 4
}