using System.Collections.ObjectModel;
using FileTime.Core.Models;

namespace FileTime.Core.Serialization.Container;

public record ContainerDeserializationResult(
    Models.Container Container, 
    ObservableCollection<Exception> Exceptions, 
    ExtensionCollection Extensions, 
    ObservableCollection<AbsolutePath> Items);