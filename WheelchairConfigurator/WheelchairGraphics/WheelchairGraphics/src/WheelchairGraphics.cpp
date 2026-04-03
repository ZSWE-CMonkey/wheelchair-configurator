#include "WheelchairGraphics.h"

#include "GraphicsPlugin.h"

#include <stdexcept>

using namespace GraphicsPlugin;

namespace {

	GraphicsPluginPtr g_graphicsPlugin = nullptr;

}


WG_API void wgInitializeVulkanGraphicsWIN32(const char* appName, void* platformHandle, void* platformWindow, int width, int height)
{
	g_graphicsPlugin = GraphicsPluginFactory::CreateVulkanGraphicsPlugin();
	
	if (!g_graphicsPlugin)
		throw std::runtime_error("Graphics plugin was not created");

	g_graphicsPlugin->SetHandles(platformHandle, platformWindow);

	GP_THROW_IF_FAIL(g_graphicsPlugin->Initialize(std::string(appName), width, height));
}

WG_API void wgInitializeMoltenVulkanGraphics(const char* appName, int width, int height)
{
	throw std::runtime_error("MoltenVulkan Graphics Plugin NOT Implemented");
}

WG_API void wgRender(const char** out)
{
	if (!g_graphicsPlugin)
		return;

	GP_THROW_IF_FAIL(g_graphicsPlugin->Render(out));
}

WG_API void wgDeinitializeGraphics() {
	if (g_graphicsPlugin)
		return;

	GP_THROW_IF_FAIL(g_graphicsPlugin->DeInitialize());
	g_graphicsPlugin = nullptr;
}
