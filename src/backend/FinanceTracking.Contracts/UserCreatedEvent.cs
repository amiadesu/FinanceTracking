namespace FinanceTracking.Contracts.Events;

public record UserCreatedEvent(Guid UserId, string Email, string Username);