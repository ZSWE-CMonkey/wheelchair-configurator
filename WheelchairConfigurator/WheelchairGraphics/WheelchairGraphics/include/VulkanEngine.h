#pragma once

#include <string>
#include <memory>

#include "VulkanCommon.h"
#include "VulkanSwapchain.h"

namespace VkEngine {

	class VulkanEngine
	{
	public:
		VulkanEngine() = default;
		~VulkanEngine();

		VkResult InitVulkan(std::string appName);

#ifdef _WIN32
		VkResult InitSwapchain(void* platformHandle, void* platformWindow);
#elif __ANDROID__
		VkResult InitSwapchain(ANativeWindow* window);
#else
		VkResult InitSwapchain();
#endif

		VkResult Prepare();
		
	private:
		VkResult CreateInstance(std::string appName);
		VkResult CheckPhysicalDevices(uint32_t& outGraphicsQueueIndex);
		VkResult CreateDevice(uint32_t graphicsQueueIndex);
		VkResult CreateVulkanSemaphore();


		VkResult CreateCommandPool();
		VkResult CreateSetupCommandBuffer();


		void CreateSumbitInfo();
		bool GetDepthFormat();

		std::unique_ptr<VulkanSwapchain> m_vulkanSwapchain = nullptr;

		VkInstance m_instance;
		VkPhysicalDevice m_physicalDevice;
		VkDevice m_device;
		VkPhysicalDeviceMemoryProperties m_deviceMemoryProperties;
		VkQueue m_queue;

		VkFormat m_depthFormat;


		VkCommandPool m_cmdPool;
		VkCommandBuffer m_setupCmdBuffer;


		VkPipelineStageFlags m_submitPipelineStages = VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;

		struct {
			VkSemaphore presentComplete;
			VkSemaphore renderComplete;
		} m_semaphores;

	};

}