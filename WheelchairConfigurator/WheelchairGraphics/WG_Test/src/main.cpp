/*

	WINDOWS testing of graphics library :>>>

*/

#include <iostream>
#include <Windows.h>
#include <stdexcept>

#include <WheelchairGraphics.h>

namespace {
	const UINT c_width = 800, c_height = 600;
	const char* c_appName = "Test";

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
		case WM_CLOSE:
			wgDeinitializeGraphics();
			DestroyWindow(hWnd);
			PostQuitMessage(0);
			break;
		case WM_PAINT:
			ValidateRect(window, NULL);
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
			wgRender();
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