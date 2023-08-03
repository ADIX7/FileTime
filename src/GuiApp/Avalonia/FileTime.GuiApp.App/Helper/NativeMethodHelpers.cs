using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace FileTime.GuiApp.App.Helper;

public static class WindowsNativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal static extern int LoadString(IntPtr hInstance, uint wID, StringBuilder lpBuffer, int nBufferMax);

    [DllImport("user32.dll")]
    public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("kernel32.dll")]
    public static extern int FreeLibrary(IntPtr hLibModule);

    [DllImport("shell32.dll")]
    public static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);
}

public static class NativeMethodHelpers
{
    public static string GetStringResource(string fileName, uint resourceId)
    {
        IntPtr? handle = null;
        try
        {
            handle = WindowsNativeMethods.LoadLibrary(fileName);
            StringBuilder buffer = new(8192); //Buffer for output from LoadString()
            int length = WindowsNativeMethods.LoadString(handle.Value, resourceId, buffer, buffer.Capacity);
            return buffer.ToString(0, length); //Return the part of the buffer that was used.
        }
        finally
        {
            if (handle is IntPtr validHandle)
            {
                WindowsNativeMethods.FreeLibrary(validHandle);
            }
        }
    }

    public static Icon GetIconResource(string fileName, uint resourceId)
    {
        IntPtr? handle = null;
        try
        {
            handle = WindowsNativeMethods.LoadLibrary(fileName);
            IntPtr handle2 = WindowsNativeMethods.LoadIcon(handle.Value, new IntPtr(resourceId));
            return Icon.FromHandle(handle2);
        }
        finally
        {
            if (handle is IntPtr validHandle)
            {
                WindowsNativeMethods.FreeLibrary(validHandle);
            }
        }
    }

    /*public static Icon GetAssociatedIcon()
    {
        ushort uicon;
        StringBuilder strB = new StringBuilder(fileName);
        IntPtr handle = WindowsNativeMethods.ExtractAssociatedIcon(this.Handle, strB, out uicon);
        return Icon.FromHandle(handle);
    }*/
}