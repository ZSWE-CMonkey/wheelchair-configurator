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

		GPluginResult Initialize(std::string appName) override;
		GPluginResult SetObject(/*TODO: parameter/s*/) override;
		GPluginResult Render() override;
		GPluginResult DeInitialize() override;
	private:
		void CleanUp();

		std::unique_ptr<VkEngine::VulkanEngine> m_vulkanEngine = nullptr;



#ifdef _WIN32
		void* m_platformHandle;
		void* m_platformWindow;
#elif __ANDROID__
		ANativeWindow* m_window;
#endif
	};

}