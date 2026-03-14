#include "WheelchairGraphics.h"

#include "GraphicsPlugin.h"

#include <stdexcept>

using namespace GraphicsPlugin;

namespace {

	GraphicsPluginPtr g_graphicsPlugin = nullptr;

}


WG_API void wgInitializeVulkanGraphics()
{
	g_graphicsPlugin = GraphicsPluginFactory::CreateVulkanGraphicsPlugin();
	
	if (!g_graphicsPlugin)
		throw std::runtime_error("Graphics plugin was not created");

	GP_THROW_IF_FAIL(g_graphicsPlugin->Initialize());
}

WG_API void wgInitializeMoltenVulkanGraphics()
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
