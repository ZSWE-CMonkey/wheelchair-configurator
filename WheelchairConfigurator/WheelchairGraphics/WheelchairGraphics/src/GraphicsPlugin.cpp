#include "GraphicsPlugin.h"

#include <stdexcept>
#include "VulkanGraphicsPlugin.h"

using namespace GraphicsPlugin;

GraphicsPluginPtr GraphicsPluginFactory::CreateVulkanGraphicsPlugin() {
	GraphicsPluginPtr ptr(reinterpret_cast<IGraphicsPlugin*>(new VulkanGraphicsPlugin()));
	return std::move(ptr);
}

GraphicsPluginPtr GraphicsPluginFactory::CreateMoltenVulkanGraphicsPlugin() {
	throw std::runtime_error("Create MoltenVulkan graphics plugin not implemented");
}
