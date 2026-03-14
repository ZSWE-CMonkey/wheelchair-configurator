#pragma once

#ifdef _WIN32
#ifdef _WG_API_IMPORT_
#define WG_API __declspec(dllimport)
#else
#define WG_API __declspec(dllexport)
#endif // _WG_API_IMPORT_
#else
#define WG_API __attribute__((visibility("default")))
#endif

//--Graphics specific--//

extern "C" WG_API int InitializeVulkanGraphics();

extern "C" WG_API int InitializeMoltenVulkanGraphics();

//--Common--//

extern "C" WG_API int DeinitializeGraphics();

//TODO: Complete api