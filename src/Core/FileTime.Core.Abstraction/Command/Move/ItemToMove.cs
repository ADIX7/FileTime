using FileTime.Core.Models;

namespace FileTime.Core.Command.Move;

public record ItemToMove(FullName Source, FullName Target);