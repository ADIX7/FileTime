﻿using FileTime.App.Core.UserCommand;

namespace FileTime.Tools.Compression;

public class CompressUserCommand : IIdentifiableUserCommand
{
    public const string CommandName = "compress";
    public static readonly CompressUserCommand Instance = new();
    private CompressUserCommand()
    {
    }

    public string UserCommandID => CommandName;
    public string Title => "Compress";
}