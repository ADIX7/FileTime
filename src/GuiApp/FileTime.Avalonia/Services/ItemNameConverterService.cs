using Avalonia.Media;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;

namespace FileTime.Avalonia.Services
{
    [ViewModel]
    [Inject(typeof(AppState))]
    public partial class ItemNameConverterService
    {
        public List<ItemNamePart> GetDisplayName(IItemViewModel itemViewModel)
        {
            var nameParts = new List<ItemNamePart>();
            var rapidTravelText = AppState.RapidTravelText.ToLower();

            var name = itemViewModel.Item is IElement ? GetFileName(itemViewModel.Item.Name) : itemViewModel.Item.Name;
            if (AppState.ViewMode == ViewMode.RapidTravel && rapidTravelText.Length > 0)
            {
                var nameLeft = name;

                while (nameLeft.ToLower().IndexOf(rapidTravelText, StringComparison.Ordinal) is int rapidTextStart && rapidTextStart != -1)
                {
                    var before = rapidTextStart > 0 ? nameLeft.Substring(0, rapidTextStart) : null;
                    var rapidTravel = nameLeft.Substring(rapidTextStart, rapidTravelText.Length);

                    nameLeft = nameLeft.Substring(rapidTextStart + rapidTravelText.Length);

                    if (before != null)
                    {
                        nameParts.Add(new ItemNamePart(before));
                    }

                    nameParts.Add(new ItemNamePart(rapidTravel, true));
                }

                if (nameLeft.Length > 0)
                {
                    nameParts.Add(new ItemNamePart(nameLeft));
                }
            }
            else
            {
                nameParts.Add(new ItemNamePart(name));
            }
            return nameParts;
        }

        public string GetFileName(string fullName)
        {
            var parts = fullName.Split('.');
            var fileName = string.Join('.', parts[..^1]);
            return string.IsNullOrEmpty(fileName) ? fullName : fileName;
        }

        public string GetFileExtension(string fullName)
        {
            var parts = fullName.Split('.');
            return parts.Length == 1 || (parts.Length == 2 && string.IsNullOrEmpty(parts[0])) ? "" : parts[^1];
        }
    }
}
