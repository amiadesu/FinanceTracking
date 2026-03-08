namespace FinanceTracking.Contracts.Events;

public record UserUpdatedEvent(Guid UserId, string NewUsername);