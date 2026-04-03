/*

	WINDOWS testing of graphics library :>>>

*/

#include <Windows.h>
#include <stdexcept>

#include <WheelchairGraphics.h>

namespace {
	const UINT c_width = 800, c_height = 600;
	const char* c_appName = "Test";

	const char* g_pixelBuffer = nullptr;

	HWND* g_window = nullptr;

	HWND setupWindow(HINSTANCE hinstance, WNDPROC wndproc)
	{
		WNDCLASSEX wndClass;
		wndClass.cbSize = sizeof(WNDCLASSEX);
		wndClass.style = CS_HREDRAW | CS_VREDRAW;
		wndClass.lpfnWndProc = wndproc;
		wndClass.cbClsExtra = 0;
		wndClass.cbWndExtra = 0;
		wndClass.hInstance = hinstance;
		wndClass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);
		wndClass.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
		wndClass.lpszMenuName = NULL;
		wndClass.lpszClassName = c_appName;
		wndClass.hIconSm = LoadIcon(NULL, IDI_WINLOGO);

		if (!RegisterClassEx(&wndClass))
		{
			throw std::runtime_error("skibidi, no register :/");
		}

		int screenWidth = GetSystemMetrics(SM_CXSCREEN);
		int screenHeight = GetSystemMetrics(SM_CYSCREEN);

		DWORD dwExStyle = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;
		DWORD dwStyle = WS_OVERLAPPEDWINDOW | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;

		RECT windowRect;
		windowRect.left = (long)screenWidth / 2 - c_width / 2;
		windowRect.right = (long)c_width;
		windowRect.top = (long)screenHeight / 2 - c_height / 2;
		windowRect.bottom = (long)c_height;

		AdjustWindowRectEx(&windowRect, dwStyle, FALSE, dwExStyle);

		std::string windowTitle = c_appName;
		HWND window = CreateWindowEx(0,
			c_appName,
			c_appName,
			dwStyle | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
			windowRect.left,
			windowRect.top,
			windowRect.right,
			windowRect.bottom,
			NULL,
			NULL,
			hinstance,
			NULL);

		if (!window)
		{
			throw std::runtime_error("no window :<");
		}

		ShowWindow(window, SW_SHOW);
		SetForegroundWindow(window);
		SetFocus(window);

		return window;
	}

	void handleMessages(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam, HWND window)
	{
		switch (uMsg)
		{
		case WM_PAINT:
		{
			wgRender(&g_pixelBuffer);
			if (g_pixelBuffer == nullptr)
				break;

			PAINTSTRUCT ps;
			HDC hdc = BeginPaint(hWnd, &ps);
			
			BITMAPINFO bmi{};
			bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
			bmi.bmiHeader.biWidth = c_width;
			bmi.bmiHeader.biHeight = -c_height;
			bmi.bmiHeader.biPlanes = 1;
			bmi.bmiHeader.biBitCount = 32;
			bmi.bmiHeader.biCompression = BI_RGB;

			
			StretchDIBits(
				hdc,
				0, 0, c_width, c_height,
				0, 0, c_width, c_height,
				g_pixelBuffer,
				&bmi,
				DIB_RGB_COLORS,
				SRCCOPY
			);

			EndPaint(hWnd, &ps);

			ValidateRect(window, NULL);
		}
		break;
		case WM_CLOSE:
			wgDeinitializeGraphics();
			DestroyWindow(hWnd);
			PostQuitMessage(0);
			break;
		}
	}

	LRESULT CALLBACK WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
	{
		if (g_window)
			handleMessages(hWnd, uMsg, wParam, lParam, *g_window);
		return (DefWindowProc(hWnd, uMsg, wParam, lParam));
	}

	void renderLoop()
	{
		MSG msg;
		while (TRUE)
		{
			if (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
			{
				if (msg.message == WM_QUIT)
				{
					break;
				}
				else
				{
					TranslateMessage(&msg);
					DispatchMessage(&msg);
				}
			}
		}
	}
}

int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR pCmdLine, int nCmdShow)
{
	HWND window = setupWindow(hInstance, WndProc);
	g_window = &window;

	wgInitializeVulkanGraphicsWIN32(c_appName, hInstance, window, c_width, c_height);
	renderLoop();
	wgDeinitializeGraphics();

	return 0;
}