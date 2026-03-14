#include "WheelchairGraphics.h"

#include "GraphicsPlugin.h"

#include <stdexcept>

using namespace GraphicsPlugin;

namespace {

	GraphicsPluginPtr g_graphicsPlugin = nullptr;

}


WG_API void wgInitializeVulkanGraphicsWIN32(const char* appName, void* platformHandle, void* platformWindow)
{
	g_graphicsPlugin = GraphicsPluginFactory::CreateVulkanGraphicsPlugin();
	
	if (!g_graphicsPlugin)
		throw std::runtime_error("Graphics plugin was not created");

	g_graphicsPlugin->SetHandles(platformHandle, platformWindow);

	GP_THROW_IF_FAIL(g_graphicsPlugin->Initialize(std::string(appName)));
}

WG_API void wgInitializeMoltenVulkanGraphics(const char* appName)
{
	throw std::runtime_error("MoltenVulkan Graphics Plugin NOT Implemented");
}

WG_API void wgRender()
{
	GP_THROW_IF_FAIL(g_graphicsPlugin->Render());
}

WG_API void wgDeinitializeGraphics() {
	GP_THROW_IF_FAIL(g_graphicsPlugin->DeInitialize());
	g_graphicsPlugin = nullptr;
}
