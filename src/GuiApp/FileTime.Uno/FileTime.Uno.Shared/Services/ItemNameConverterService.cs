using FileTime.Uno.Application;
using FileTime.Uno.Models;
using FileTime.Uno.ViewModels;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Text;

namespace FileTime.Uno.Services
{
    [ViewModel]
    [Inject(typeof(AppState))]
    public partial class ItemNameConverterService
    {
        public List<ItemNamePart> GetDisplayName(IItemViewModel itemViewModel)
        {
            var nameParts = new List<ItemNamePart>();
            var rapidTravelText = AppState.RapidTravelText.ToLower();

            if (AppState.ViewMode == ViewMode.RapidTravel && rapidTravelText.Length > 0)
            {
                var nameLeft = itemViewModel.Item.Name;

                while (nameLeft.ToLower().Contains(rapidTravelText))
                {
                    var rapidTextStart = nameLeft.ToLower().IndexOf(rapidTravelText);
                    var before = rapidTextStart > 0 ? nameLeft.Substring(0, rapidTextStart) : null;
                    var rapidTravel = nameLeft.Substring(rapidTextStart, rapidTravelText.Length);

                    nameLeft = nameLeft.Substring(rapidTextStart + rapidTravelText.Length);

                    if (before != null)
                    {
                        nameParts.Add(new ItemNamePart(before));
                    }
                    nameParts.Add(new ItemNamePart(rapidTravel) { TextDecorations = TextDecorations.Underline });
                }

                if (nameLeft.Length > 0)
                {
                    nameParts.Add(new ItemNamePart(nameLeft));
                }
            }
            else
            {
                nameParts.Add(new ItemNamePart(itemViewModel.Item.Name));
            }
            return nameParts;
        }
    }
}
