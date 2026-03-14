#pragma once

#include "GraphicsPlugin.h"
#include "VulkanEngine.h"

#include <memory>

#include <vulkan/vulkan.h>

namespace GraphicsPlugin {

	class VulkanGraphicsPlugin : IGraphicsPlugin
	{
	public:
		VulkanGraphicsPlugin();
		~VulkanGraphicsPlugin();

		GPluginResult Initialize() override;
		GPluginResult SetObject(/*TODO: parameter/s*/) override;
		GPluginResult Render() override;
		GPluginResult DeInitialize() override;
	private:
		void CleanUp();

		std::unique_ptr<VulkanEngine> m_vulkanEngine = nullptr;
	};

}