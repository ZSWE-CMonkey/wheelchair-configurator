#include "WheelchairGraphics.h"

#include "GraphicsPlugin.h"

#include <vector>
#include <string>
#include <stdexcept>
#include <unordered_map>

using namespace GraphicsPlugin;

namespace {

	GraphicsPluginPtr g_graphicsPlugin = nullptr;

	std::vector<std::string> g_objects{};

	struct ObjectFilePathsEntry {
		std::string geometryPath;
		std::string texturePath;
		float scale;
		float anchorX, anchorY, anchorZ;
		float rotationX, rotationY, rotationZ;
	};

	std::unordered_map<std::string, ObjectFilePathsEntry> g_objectFilePaths{};

	std::unique_ptr<CameraSettings> g_cameraSettings = nullptr;

	bool g_highQualityTextures = true;
}

bool wgGetHighQualityTextures() { return g_highQualityTextures; }


WG_API void wgInitializeVulkanGraphicsWIN32(const char* appName, int width, int height)
{
	g_graphicsPlugin = GraphicsPluginFactory::CreateVulkanGraphicsPlugin();

	if (!g_graphicsPlugin)
		throw std::runtime_error("Graphics plugin was not created");

	for (auto& id : g_objects) {
		auto it = g_objectFilePaths.find(id);
		if (it != g_objectFilePaths.end())
			g_graphicsPlugin->AddObjectFromFiles(id, it->second.geometryPath, it->second.texturePath,
				it->second.scale,
				it->second.anchorX, it->second.anchorY, it->second.anchorZ,
				it->second.rotationX, it->second.rotationY, it->second.rotationZ);
		else
			g_graphicsPlugin->AddObject(id);
	}

	if (g_cameraSettings)
		g_graphicsPlugin->SetCamera(*g_cameraSettings);

	GP_THROW_IF_FAIL(g_graphicsPlugin->Initialize(std::string(appName), width, height));
}

WG_API void wgInitializeMoltenVulkanGraphics(const char* appName, int width, int height)
{
	throw std::runtime_error("MoltenVulkan Graphics Plugin NOT Implemented");
}

#if defined(__ANDROID__)
WG_API void wgInitializeVulkanGraphicsANDROID(const char* appName, int width, int height)
{
	g_graphicsPlugin = GraphicsPluginFactory::CreateVulkanGraphicsPlugin();

	if (!g_graphicsPlugin)
		throw std::runtime_error("Graphics plugin was not created");

	for (auto& id : g_objects) {
		auto it = g_objectFilePaths.find(id);
		if (it != g_objectFilePaths.end())
			g_graphicsPlugin->AddObjectFromFiles(id, it->second.geometryPath, it->second.texturePath,
				it->second.scale,
				it->second.anchorX, it->second.anchorY, it->second.anchorZ,
				it->second.rotationX, it->second.rotationY, it->second.rotationZ);
		else
			g_graphicsPlugin->AddObject(id);
	}

	if (g_cameraSettings)
		g_graphicsPlugin->SetCamera(*g_cameraSettings);

	GP_THROW_IF_FAIL(g_graphicsPlugin->Initialize(std::string(appName), width, height));
}
#else
WG_API void wgInitializeVulkanGraphicsANDROID(const char* appName, int width, int height)
{
	throw std::runtime_error("wgInitializeVulkanGraphicsANDROID is not supported on this platform");
}
#endif

WG_API void wgSetCamera(float zoom, float x, float y, float z, float rX, float rY, float rZ) {
	if (g_cameraSettings)
		g_cameraSettings = nullptr;

	g_cameraSettings = std::make_unique<CameraSettings>();
	
	g_cameraSettings->zoom = zoom;
	
	g_cameraSettings->position.x = x;
	g_cameraSettings->position.y = y;
	g_cameraSettings->position.z = z;

	g_cameraSettings->rotation.x = rX;
	g_cameraSettings->rotation.y = rY;
	g_cameraSettings->rotation.z = rZ;

	if (g_graphicsPlugin)
		g_graphicsPlugin->SetCamera(*g_cameraSettings);
}

WG_API void wgAddObject(const char* objectId) {
	g_objects.push_back(std::string(objectId));
}

WG_API void wgAddObjectFromFiles(const char* objectId, const char* geometryAbsolutePath, const char* textureAbsolutePath,
	float scale,
	float anchorX, float anchorY, float anchorZ,
	float rotationX, float rotationY, float rotationZ) {
	g_objects.push_back(std::string(objectId));
	g_objectFilePaths[std::string(objectId)] = {
		std::string(geometryAbsolutePath),
		textureAbsolutePath ? std::string(textureAbsolutePath) : std::string(),
		scale,
		anchorX, anchorY, anchorZ,
		rotationX, rotationY, rotationZ
	};
}

WG_API void wgSetHighQualityTextures(bool enabled) {
	g_highQualityTextures = enabled;
}

WG_API void wgRender(const char** out)
{
	if (!g_graphicsPlugin)
		return;

	GP_THROW_IF_FAIL(g_graphicsPlugin->Render(out));
}

WG_API void wgDeinitializeGraphics() {
	g_objects.clear();
	g_objectFilePaths.clear();
	g_cameraSettings = nullptr;

	if (!g_graphicsPlugin)
		return;

	GP_THROW_IF_FAIL(g_graphicsPlugin->DeInitialize());
	g_graphicsPlugin = nullptr;
}
