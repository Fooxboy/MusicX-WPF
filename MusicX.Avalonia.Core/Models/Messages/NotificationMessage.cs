namespace MusicX.Avalonia.Core.Models.Messages;

public record NotificationMessage(string Message, string? Title = null, TimeSpan? Duration = null);