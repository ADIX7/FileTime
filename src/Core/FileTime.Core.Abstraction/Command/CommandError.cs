namespace FileTime.Core.Command;

public record CommandError(string Message, Exception? Exception);
