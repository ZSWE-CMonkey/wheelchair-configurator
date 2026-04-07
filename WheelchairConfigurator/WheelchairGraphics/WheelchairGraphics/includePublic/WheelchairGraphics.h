#pragma once

#include <cstdint>

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

//TODO: ADD android as well

extern "C" WG_API void wgInitializeVulkanGraphicsWIN32(const char* appName, int width, int height);

extern "C" WG_API void wgInitializeMoltenVulkanGraphics(const char* appName, int width, int height);

//--Common--//

extern "C" WG_API void wgSetCamera(float zoom, float x, float y, float z, float rX, float rY, float rZ);

extern "C" WG_API void wgAddObject(const char* objectId);

extern "C" WG_API void wgRender(const char** out);

extern "C" WG_API void wgDeinitializeGraphics();