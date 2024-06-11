using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace RuSharpX;

public class Interop
{
    #region Windows System Internals / Interop

    // ReSharper disable InconsistentNaming
    private const int WM_COPYDATA = 74;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

    [StructLayout(LayoutKind.Sequential)]
    private struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }

    private enum HRESULT : uint
    {
        S_OK = 0x0000
    }

    // ReSharper enable InconsistentNaming

    #endregion

    /// <summary>
    /// A <see cref="IntPtr"/> pointing to a window handle of an active FTP Rush window.
    /// </summary>
    public static IntPtr FtpRushHdl { get; private set; } = IntPtr.Zero;

    /// <summary>
    /// A list of site names that FTP Rush has loaded. This will only be set after
    /// <see cref="TryGetSiteNames"/> has been successfully called at least once.
    /// </summary>
    public static string[]? SiteNames { get; private set; }

    /// <summary>
    /// Tries to find the first FTP Rush window and set <see cref="FtpRushHdl"/> to its handle.
    /// </summary>
    /// <returns>A boolean determining whether a valid handle was found.</returns>
    public static bool TryFindFtpRushWindow()
    {
        FtpRushHdl = FindWindow("TfmRush", null);
        return FtpRushHdl != IntPtr.Zero;
    }

    /// <summary>
    /// Tries to find the first FTP Rush window and set <see cref="FtpRushHdl"/> to its handle.
    /// </summary>
    /// <param name="hdl">A <see cref="IntPtr"/> out-param to set to the determined handle.</param>
    /// <returns>A boolean determining whether a valid handle was found.</returns>
    public static bool TryFindFtpRushWindow(out IntPtr? hdl)
    {
        FtpRushHdl = FindWindow("TfmRush", null);
        hdl = FtpRushHdl;

        return FtpRushHdl != IntPtr.Zero;
    }

    /// <summary>
    /// Check whether the currently-selected FTP Rush window (see <see cref="FtpRushHdl"/>) is connected to any site.
    /// </summary>
    /// <returns>Whether the window is connected to any site.</returns>
    public static bool IsConnectedToSite()
    {
        StringBuilder title = new StringBuilder(256);
        int result = GetWindowText(FtpRushHdl, title, 256);
        if (result != (int)HRESULT.S_OK) return false;

        return !title.ToString().Contains("blank");
    }

    /// <summary>
    /// Check whether the currently-selected FTP Rush window (see <see cref="FtpRushHdl"/>) is connected to the provided site name.
    /// <br/>
    /// <br/>NOTE: This does NOT support tabbed clients, and will only return the current tab.
    /// <br/>NOTE: This method matches based on substringing (.Contains) and may return false positives.
    /// </summary>
    /// <param name="siteName">A site name to check against.</param>
    /// <returns>A boolean determining whether the provided site name was found in the window title.</returns>
    public static bool IsConnectedToSite(string siteName)
    {
        StringBuilder title = new StringBuilder(256);
        int result = GetWindowText(FtpRushHdl, title, 256);
        if (result != (int)HRESULT.S_OK) return false;

        return title.ToString().Contains(siteName);
    }

    /// <summary>
    /// Gets the name of the currently-connected site for the currently-selected FTP Rush window (see <see cref="FtpRushHdl"/>)
    /// <br/>
    /// <br/>NOTE: This does NOT support tabbed clients, and will only return the current tab. 
    /// </summary>
    /// <returns>A string containing the connected site name</returns>
    public static string GetConnectedSite()
    {
        StringBuilder title = new StringBuilder(256);
        int result = GetWindowText(FtpRushHdl, title, 256);
        if (result != (int)HRESULT.S_OK) return string.Empty;

        return title.ToString().Split("FTP Rush   ")[1];
    }

    /// <summary>
    /// Attempts to load a list of site available site names from the FTP Rush site configuration file (RushSite.xml) and
    /// sets the <see cref="SiteNames"/> variable to the result.
    /// </summary>
    /// <param name="rushSiteFilePath">An optional path to a RushSite.xml file. Defaults to %APPDATA%/FTPRush/RushSite.xml.</param>
    /// <returns>Whether the attempt to load sites was successful.
    /// This does not mean that any site was found, but that no exception occured.
    /// </returns>
    public static bool TryGetSiteNames(string? rushSiteFilePath = null)
    {
        try
        {
            if (string.IsNullOrEmpty(rushSiteFilePath))
                rushSiteFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FTPRush", "RushSite.xml");

            XDocument doc =
                XDocument.Load(rushSiteFilePath);

            SiteNames = doc.Descendants("GROUP")
                .Where(group => group.Attribute("NAME")?.Value != "History") // Filter out History group
                .Descendants("SITE")
                .Select(site => site.Attribute("NAME")?.Value)
                .ToArray()!;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// This method will send a window message to the currently active FTP Rush window (see <see cref="FtpRushHdl"/>)
    /// in order to run a Pascal script (see <paramref name="cmd"/>).
    /// <br/>
    /// <br/>A callback with the result of the script may be provided (see <paramref name="ourHdl"/>) 
    /// </summary>
    /// <param name="ourHdl">
    /// A possibly-null pointer to a Window handle that will receive the result of this operation as a callback window message.
    /// <br/>WARNING: You must be able to process Window Messages in the window handle you pass (through WndProc),
    /// else your callback is as good as throwing it into /dev/null.
    /// </param>
    /// <param name="cmd">
    /// A fully-interpreted, pre-processed Pascal Script Environment (rfScriptEnv) script to be run by
    /// FTP Rush. This supports processor-time constants such as RS_LOGIN.
    /// </param>
    public static void SendScript(IntPtr? ourHdl, string cmd)
    {
        IntPtr hGlobalAnsi = Marshal.StringToHGlobalAnsi(cmd);
        COPYDATASTRUCT structure = new COPYDATASTRUCT
        {
            dwData = 1000, // action (run script)
            cbData = cmd.Length + 1, // length + null terminator for strings
            lpData = hGlobalAnsi // pointer to our script cmd
        };

        IntPtr gStructPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(COPYDATASTRUCT)));
        Marshal.StructureToPtr((object)structure, gStructPtr, false);

        if (ourHdl == null)
            SendMessage(FtpRushHdl, WM_COPYDATA, IntPtr.Zero, gStructPtr);
        else
            SendMessage(FtpRushHdl, WM_COPYDATA, (IntPtr)ourHdl, gStructPtr);

        Marshal.FreeHGlobal(hGlobalAnsi);
        Marshal.FreeHGlobal(gStructPtr);
    }
}