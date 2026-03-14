#pragma once

#ifdef _WIN32
#ifdef _WG_API_EXPORT_
#define WG_API __declspec(dllexport)
#else
#define WG_API __declspec(dllimport)
#endif // _WG_API_IMPORT_
#else
#define WG_API __attribute__((visibility("default")))
#endif

//--Graphics specific--//

extern "C" WG_API void wgInitializeVulkanGraphics();

extern "C" WG_API void wgInitializeMoltenVulkanGraphics();

//--Common--//

extern "C" WG_API void wgRender();

extern "C" WG_API void wgDeinitializeGraphics();