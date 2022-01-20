using FileTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.IconProviders
{
    public class SystemIconProvider : IIconProvider
    {
        public string GetImage(IItem item)
        {
            return "ms-appx:///";
        }
    }
}
