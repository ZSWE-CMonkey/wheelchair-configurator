#pragma once

#include "GraphicsPlugin.h"
#include "VulkanEngine.h"

#include <memory>

namespace GraphicsPlugin {

	class VulkanGraphicsPlugin : IGraphicsPlugin
	{
	public:
		VulkanGraphicsPlugin();
		~VulkanGraphicsPlugin();

#ifdef _WIN32
		void SetHandles(void* platformHandle, void* platformWindow) override;
#elif __ANDROID__
		void SetHandles(ANativeWindow* window) override;
#else
		void SetHandles() override;
#endif

		GPluginResult Initialize(std::string appName, uint32_t width, uint32_t height) override;
		GPluginResult SetObject(std::string objectId) override;
		GPluginResult Render(const char** out) override;
		GPluginResult DeInitialize() override;
	private:
		void CleanUp();

		std::unique_ptr<VkEngine::VulkanEngine> m_vulkanEngine = nullptr;

		uint32_t m_width;
		uint32_t m_height;

#ifdef _WIN32
		void* m_platformHandle;
		void* m_platformWindow;
#elif __ANDROID__
		ANativeWindow* m_window;
#endif
	};

}