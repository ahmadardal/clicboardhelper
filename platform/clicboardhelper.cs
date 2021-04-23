using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

//https://stackoverflow.com/questions/621577/clipboard-event-c-sharp
//https://stackoverflow.com/questions/17762037/error-while-trying-to-copy-string-to-clipboard
//https://gist.github.com/glombard/7986317

internal static class NativeMethods
{
    //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
    public const int WM_CLIPBOARDUPDATE = 0x031D;
    //Reference https://www.pinvoke.net/default.aspx/Constants.HWND
    public static IntPtr HWND_MESSAGE = new IntPtr(-3);

    //Reference https://www.pinvoke.net/default.aspx/user32/AddClipboardFormatListener.html
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);

    //Reference https://www.pinvoke.net/default.aspx/user32.setparent
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
}

public static class Clipboard
{
    public static string GetText()
    {
        string ReturnValue = string.Empty;
        Thread STAThread = new Thread(
            delegate ()
            {
                // Use a fully qualified name for Clipboard otherwise it
                // will end up calling itself.
                ReturnValue = System.Windows.Forms.Clipboard.GetText();
            });
        STAThread.SetApartmentState(ApartmentState.STA);
        STAThread.Start();
        STAThread.Join();

        return ReturnValue;
    }
}

public sealed class ClipboardNotification
{
    private class NotificationForm : Form
    {
        public string Previous;
        public string Current;
        public NotificationForm()
        {
            //Turn the child window into a message-only window (refer to Microsoft docs)
            NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
            //Place window in the system-maintained clipboard format listener list
            NativeMethods.AddClipboardFormatListener(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            //Listen for operating system messages
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                Current = Clipboard.GetText();
                if(Previous != Current)
                {
                    Previous = Current;
                    Console.WriteLine("CLIPBOARD_CHANGE");
                }
                ////Write to stdout active window
                //IntPtr active_window = NativeMethods.GetForegroundWindow();
                //int length = NativeMethods.GetWindowTextLength(active_window);
                //StringBuilder sb = new StringBuilder(length + 1);
                //NativeMethods.GetWindowText(active_window, sb, sb.Capacity);
                //Console.WriteLine("Clipboard Active Window: " + sb.ToString());

            }
            //Called for any unhandled messages
            base.WndProc(ref m);
        }
    }

    private static void Main(string[] args)
    {
        //starts a message loop on current thread and displays specified form
        Application.Run(new NotificationForm());
    }
}