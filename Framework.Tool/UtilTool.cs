namespace Framework.Tool
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class UtilTool
    {
        public static void OpenVisualStudioCode(string folderName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo info = new ProcessStartInfo(Framework.Build.ConnectionManager.VisualStudioCodeFileName, folderName);
                info.CreateNoWindow = true;
                Process.Start(info);
            }
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")); // Works ok on windows
            }
        }
    }
}
