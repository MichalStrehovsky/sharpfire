using System;
using System.Runtime.InteropServices;

unsafe static class Win32
{
#if CORERT
    const string User32 = "__Internal";
    const string Gdi32 = "__Internal";
    const string Kernel32 = "__Internal";
#else
    const string User32 = "User32";
    const string Gdi32 = "Gdi32";
    const string Kernel32 = "Kernel32";
#endif

    public enum BOOL : int { FALSE = 0 }

    [DllImport(User32, CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern ushort RegisterClassW(WNDCLASS* lpWndClass);

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WNDCLASS
    {
        public int style;
        public delegate* stdcall<IntPtr, uint, IntPtr, IntPtr, IntPtr> lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public char* lpszMenuName;
        public char* lpszClassName;
    }

    [DllImport(User32, CharSet = CharSet.Unicode)]
    public unsafe static extern IntPtr CreateWindowExW(
        int dwExStyle,
        char* lpClassName,
        char* lpWindowName,
        int dwStyle,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInst,
        IntPtr lParam);

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL GetMessageW(
        MSG* lpMsg,
        IntPtr hWnd = default,
        uint wMsgFilterMin = 0,
        uint wMsgFilterMax = 0);

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL TranslateMessage(MSG* msg);

    [DllImport(User32, ExactSpelling = true)]
    public static extern IntPtr DispatchMessageW(MSG* msg);

    [DllImport(User32, ExactSpelling = true)]
    public static extern IntPtr DefWindowProcW(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport(User32, ExactSpelling = true)]
    public static extern IntPtr SetTimer(IntPtr hWnd, nint nIDEvent, uint uElapse, IntPtr lpTimerFunc);

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL KillTimer(IntPtr hWnd, nint uIDEvent);

    [DllImport(User32, ExactSpelling = true)]
    public static extern void PostQuitMessage(int nExitCode);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public BOOL fErase;
        public RECT rcPaint;
        public BOOL fRestore;
        public BOOL fIncUpdate;
        public fixed byte rgbReserved[32];
    }

    [DllImport(User32, ExactSpelling = true)]
    public static extern IntPtr BeginPaint(IntPtr hWnd, PAINTSTRUCT* lpPaint);

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL EndPaint(IntPtr hWnd, PAINTSTRUCT* lpPaint);

    public readonly struct HRGN
    {
        public IntPtr Handle { get; }
    }

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL InvalidateRgn(IntPtr hWnd, HRGN hrgn, BOOL erase);


    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    public enum BI
    {
        RGB = 0,
        RLE8 = 1,
        RLE4 = 2,
        BITFIELDS = 3,
        JPEG = 4,
        PNG = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public BI biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        public RGBQUAD bmiColors;
    }

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    public enum DIB
    {
        RGB_COLORS = 0,
        PAL_COLORS = 1,
    }

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern IntPtr CreateDIBSection(IntPtr hdc, BITMAPINFO* pbmi, DIB usage, void* ppvBits, IntPtr hSection, uint offset);

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern BOOL DeleteDC(IntPtr hDC);

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern BOOL DeleteObject(IntPtr hObject);

    public enum ROP
    {
        SRCCOPY = 0x00CC0020, // dest = source
        SRCPAINT = 0x00EE0086, // dest = source OR dest
        SRCAND = 0x008800C6, // dest = source AND dest
        SRCINVERT = 0x00660046, // dest = source XOR dest
        SRCERASE = 0x00440328, // dest = source AND (NOT dest)
        NOTSRCCOPY = 0x00330008, // dest = (NOT source)
        NOTSRCERASE = 0x001100A6, // dest = (NOT src) AND (NOT dest)
        MERGECOPY = 0x00C000CA, // dest = (source AND pattern)
        MERGEPAINT = 0x00BB0226, // dest = (NOT source) OR dest
        PATCOPY = 0x00F00021, // dest = pattern
        PATPAINT = 0x00FB0A09, // dest = DPSnoo
        PATINVERT = 0x005A0049, // dest = pattern XOR dest
        DSTINVERT = 0x00550009, // dest = (NOT dest)
        BLACKNESS = 0x00000042, // dest = BLACK
        WHITENESS = 0x00FF0062  // dest = WHITE
    }

    [DllImport(Gdi32, ExactSpelling = true)]
    public static extern BOOL BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, ROP rop);

    [DllImport(Kernel32, ExactSpelling = true)]
    public static extern uint GetTickCount();

    [DllImport(User32, ExactSpelling = true)]
    public static extern BOOL AdjustWindowRectEx(RECT* lpRect, int dwStyle, BOOL bMenu, int dwExStyle);
}