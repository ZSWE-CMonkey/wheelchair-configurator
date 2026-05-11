#pragma once

#include "GraphicsPlugin.h"
#include "VulkanEngine.h"

#include <memory>

namespace GraphicsPlugin {

	class VulkanGraphicsPlugin : IGraphicsPlugin
	{
	public:
		VulkanGraphicsPlugin();
		~VulkanGraphicsPlugin() override;

		GPluginResult Initialize(std::string appName, uint32_t width, uint32_t height) override;
		GPluginResult SetCamera(CameraSettings setting) override;
		GPluginResult AddObject(std::string objectId) override;
		GPluginResult AddObjectFromFiles(std::string objectId, std::string daePath, std::string ktxPath);
		GPluginResult Render(const char** out) override;
		GPluginResult DeInitialize() override;
	private:
		void CleanUp();

		std::unique_ptr<VkEngine::VulkanEngine> m_vulkanEngine = nullptr;

		uint32_t m_width;
		uint32_t m_height;
	};

}