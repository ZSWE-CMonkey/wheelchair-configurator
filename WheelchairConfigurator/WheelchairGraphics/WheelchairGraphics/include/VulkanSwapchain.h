#pragma once

#include "VulkanCommon.h"

namespace VkEngine {

	class VulkanSwapchain {
	public:
		VulkanSwapchain(VkInstance& instance, VkPhysicalDevice& physicalDevice, VkDevice& device);
		~VulkanSwapchain();

#ifdef _WIN32
		VkResult CreateSurface(void* platformHandle, void* platformWindow);
#elif __ANDROID__
		VkResult CreateSurface(ANativeWindow* window);
#else
		VkResult CreateSurface();
#endif

		VkResult InitSurface();

	private:
		VkInstance& m_instance;
		VkPhysicalDevice& m_physicalDevice; 
		VkDevice& m_device;

		VkSurfaceKHR m_surface;
		VkFormat m_colorFormat;
		VkColorSpaceKHR m_colorSpace;

		VkSwapchainKHR m_swapchain = VK_NULL_HANDLE;

		uint32_t m_queueNodeIndex = UINT32_MAX;

		PFN_vkGetPhysicalDeviceSurfaceSupportKHR fn_GetPhysicalDeviceSurfaceSupportKHR;
		PFN_vkGetPhysicalDeviceSurfaceFormatsKHR fn_GetPhysicalDeviceSurfaceFormatsKHR;
	};

}