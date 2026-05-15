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

extern "C" WG_API void wgInitializeVulkanGraphicsWIN32(const char* appName, int width, int height);

extern "C" WG_API void wgInitializeVulkanGraphicsANDROID(const char* appName, int width, int height);

extern "C" WG_API void wgInitializeMoltenVulkanGraphics(const char* appName, int width, int height);

//--Common--//

extern "C" WG_API void wgSetCamera(float zoom, float x, float y, float z, float rX, float rY, float rZ);

extern "C" WG_API void wgAddObject(const char* objectId);

extern "C" WG_API void wgAddObjectFromFiles(const char* objectId, const char* geometryAbsolutePath, const char* textureAbsolutePath,
    float scale,
    float anchorX, float anchorY, float anchorZ,
    float rotationX, float rotationY, float rotationZ);

extern "C" WG_API void wgSetHighQualityTextures(bool enabled);

extern "C" WG_API void wgRender(const char** out);

extern "C" WG_API void wgDeinitializeGraphics();

// Internal accessor for high-quality texture flag (used by TextureHandle.cpp + VulkanEngine.cpp).
bool wgGetHighQualityTextures();