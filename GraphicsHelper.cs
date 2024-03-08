using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using static System.Net.Mime.MediaTypeNames;
namespace ezikbucur
{
    public class GraphicsHelper
    {
        public static Rectangle GetScreenRect()
        {
            Rectangle rect = new();
            foreach (Screen s in Screen.AllScreens)
            {
                rect = Rectangle.Union(rect, s.Bounds);
            }
            return rect;
        }

        public static Bitmap CaptureScreen()
        {
            // Import Windows API functions
            [DllImport("user32.dll")]
            static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            static extern IntPtr GetWindowDC(IntPtr hwnd);

            [DllImport("user32.dll")]
            static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

            [DllImport("gdi32.dll")]
            static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            static extern bool DeleteDC(IntPtr hdc);

            // Get the desktop window handle and device context
            IntPtr desktopWindowHandle = GetDesktopWindow();
            IntPtr desktopDeviceContext = GetWindowDC(desktopWindowHandle);

            // Get the width and height of the screen
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Create a compatible device context and bitmap
            IntPtr compatibleDeviceContext = CreateCompatibleDC(desktopDeviceContext);
            IntPtr compatibleBitmap = CreateCompatibleBitmap(desktopDeviceContext, screenWidth, screenHeight);

            // Select the bitmap into the device context
            IntPtr oldBitmap = SelectObject(compatibleDeviceContext, compatibleBitmap);

            // Perform the bit-block transfer from the screen to the bitmap
            BitBlt(compatibleDeviceContext, 0, 0, screenWidth, screenHeight, desktopDeviceContext, 0, 0, 0x00CC0020);

            // Create a Bitmap from the compatible bitmap
            Bitmap bitmap = Bitmap.FromHbitmap(compatibleBitmap);

            // Clean up resources
            DeleteObject(SelectObject(compatibleDeviceContext, oldBitmap));
            DeleteDC(compatibleDeviceContext);
            ReleaseDC(desktopWindowHandle, desktopDeviceContext);

            return bitmap;
        }

        public static Bitmap InvertBitmap(Bitmap img)
        {
            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Mat screen = new Mat(img.Height, img.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
            using (Image<Bgra, byte> image = new(img.Width, img.Height, bmpData.Stride, bmpData.Scan0))
            {
                image._Flip(FlipType.Both);
                image._GammaCorrect(1.1d);
                image.Mat.CopyTo(screen);
            }
            CvInvoke.GaussianBlur(screen, screen, new Size(5, 5), 0);
            img.UnlockBits(bmpData);
            img.Dispose();
            GC.Collect();
            return screen.ToBitmap();
        }

    }
}
