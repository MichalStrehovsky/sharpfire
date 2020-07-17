using System;
using System.Runtime.InteropServices;

using static Win32;

unsafe class Program
{
    const int Width = 320;
    const int Height = 180;

    static ReadOnlySpan<byte> palette => new byte[]
    {
        0x07,0x07,0x07,
        0x1F,0x07,0x07,
        0x2F,0x0F,0x07,
        0x47,0x0F,0x07,
        0x57,0x17,0x07,
        0x67,0x1F,0x07,
        0x77,0x1F,0x07,
        0x8F,0x27,0x07,
        0x9F,0x2F,0x07,
        0xAF,0x3F,0x07,
        0xBF,0x47,0x07,
        0xC7,0x47,0x07,
        0xDF,0x4F,0x07,
        0xDF,0x57,0x07,
        0xDF,0x57,0x07,
        0xD7,0x5F,0x07,
        0xD7,0x5F,0x07,
        0xD7,0x67,0x0F,
        0xCF,0x6F,0x0F,
        0xCF,0x77,0x0F,
        0xCF,0x7F,0x0F,
        0xCF,0x87,0x17,
        0xC7,0x87,0x17,
        0xC7,0x8F,0x17,
        0xC7,0x97,0x1F,
        0xBF,0x9F,0x1F,
        0xBF,0x9F,0x1F,
        0xBF,0xA7,0x27,
        0xBF,0xA7,0x27,
        0xBF,0xAF,0x2F,
        0xB7,0xAF,0x2F,
        0xB7,0xB7,0x2F,
        0xB7,0xB7,0x37,
        0xCF,0xCF,0x6F,
        0xDF,0xDF,0x9F,
        0xEF,0xEF,0xC7,
        0xFF,0xFF,0xFF
    };

    struct FirePixels
    {
        public fixed byte Data[Width * Height];
    }

    static FirePixels firePixels;

    static MiniRandom rng;

    static void SpreadFire(int src)
    {
        byte pixel = firePixels.Data[src];

        if (pixel == 0)
        {
            firePixels.Data[src - Width] = 0;
        }
        else
        {
            var rand = (int)rng.Next() & 3;
            var dst = (src - rand) + 1;
            firePixels.Data[dst - Width] = (byte)(pixel - (rand & 1));
        }
    }

    private static void RenderEffect(uint tick, byte* framebuf)
    {
        for (int x = 1; x < Width; x++)
        {
            for (int y = 1; y < Height; y++)
            {
                SpreadFire(y * Width + x);
            }
        }

        // Convert palette buffer to RGB and write it to ouput.
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var index = firePixels.Data[y * Width + x];
                framebuf[((Width * y) + x) * 4 + 2] = palette[index * 3 + 0];
                framebuf[((Width * y) + x) * 4 + 1] = palette[index * 3 + 1];
                framebuf[((Width * y) + x) * 4 + 3] = palette[index * 3 + 2];
            }
        }
    }

    private static void Render(IntPtr hDC)
    {
        RenderEffect(GetTickCount(), (byte*)framebuf);
        BitBlt(hDC, 0, 0, Width, Height, pDc, 0, 0, ROP.SRCCOPY);
    }

    static IntPtr ourbitmap;
    static int* framebuf;
    static IntPtr pDc;
    static IntPtr old;

    private static void InitFramebuff()
    {
        IntPtr hDC = CreateCompatibleDC(IntPtr.Zero);
        BITMAPINFO bitmapinfo = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)sizeof(BITMAPINFOHEADER),
                biWidth = Width,
                biHeight = -Height, /* top-down */
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI.RGB,
                biClrUsed = 256,
                biClrImportant = 256,
            }
        };

        int* buf;
        ourbitmap = CreateDIBSection(hDC, &bitmapinfo, DIB.RGB_COLORS, &buf, IntPtr.Zero, 0);
        framebuf = buf;
        pDc = CreateCompatibleDC(IntPtr.Zero);
        old = SelectObject(pDc, ourbitmap);
        DeleteDC(hDC);

        for (int i = 0; i < Width; i++)
            firePixels.Data[(Height - 1) * Width + i] = 36;
    }

    private static void DestroyFramebuff()
    {
        SelectObject(pDc, old);
        DeleteDC(pDc);
        DeleteObject(ourbitmap);
    }

    [UnmanagedCallersOnly]
    private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr lParam, IntPtr wParam)
    {
        switch (msg)
        {
            case 1: /*WM_CREATE*/
                SetTimer(hwnd, 1, 1, default);
                InitFramebuff();
                break;
            case 2: /*WM_DESTROY*/
                PostQuitMessage(0);
                KillTimer(hwnd, 1);
                DestroyFramebuff();
                break;
            case 0xF: /*WM_PAINT*/
                PAINTSTRUCT pS;
                IntPtr hDC = BeginPaint(hwnd, &pS);
                Render(hDC);
                EndPaint(hwnd, &pS);
                break;
            case 0x113: /*WM_TIMER*/
                InvalidateRgn(hwnd, default, BOOL.FALSE);
                break;
            default:
                return DefWindowProcW(hwnd, msg, lParam, wParam);
        }
        return default;
    }

    static int Main()
    {
        rng = new MiniRandom(5005);

        string className = "MyClass";
        IntPtr wnd;
        fixed (char* pClassName = className)
        {
            // C# 9.0 preview feature limitation - need to cast away the "managed" because C# doesn't yet understand
            // UnmanagedCallersOnlyAttribute
            var wndproc = (delegate* stdcall<IntPtr, uint, IntPtr, IntPtr, IntPtr>)(delegate* managed<IntPtr, uint, IntPtr, IntPtr, IntPtr>)&WndProc;

            WNDCLASS wndClass = new WNDCLASS
            {
                lpfnWndProc = wndproc,
                lpszClassName = pClassName,
            };

            if (RegisterClassW(&wndClass) == 0)
            {
                return 1;
            }

            int dwStyle = 0x00080000 | 0x00C00000 | 0x00800000 | 0x10000000 | 0x00020000;

            RECT rect = new RECT
            {
                left = 0,
                top = 0,
                right = Width,
                bottom = Height,
            };
            AdjustWindowRectEx(&rect, dwStyle, BOOL.FALSE, 0);

            wnd = CreateWindowExW(0,
                pClassName,
                pClassName,
                dwStyle,
                unchecked((int)0x80000000), 0,
                rect.right + (-rect.left), rect.bottom + (-rect.top), default, default, default, default);
        }

        ShowWindow(wnd, 10);

        MSG msg;
        while (GetMessageW(&msg) != BOOL.FALSE)
        {
            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
        return 0;
    }
}
