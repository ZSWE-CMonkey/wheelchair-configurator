#pragma once

#include "VulkanCommon.h"

#include <vector>


#define GET_INSTANCE_PROC_ADDR(inst, entrypoint)                        \
{                                                                       \
    fn_##entrypoint = (PFN_vk##entrypoint) vkGetInstanceProcAddr(inst, "vk"#entrypoint); \
    if (fn_##entrypoint == NULL)                                         \
	{																    \
        throw std::runtime_error("Function was not loaded");            \
    }                                                                   \
}

#define GET_DEVICE_PROC_ADDR(dev, entrypoint)                           \
{                                                                       \
    fn_##entrypoint = (PFN_vk##entrypoint) vkGetDeviceProcAddr(dev, "vk"#entrypoint);   \
    if (fn_##entrypoint == NULL)                                         \
	{																    \
        throw std::runtime_error("Function was not loaded");            \
    }                                                                   \
}


namespace VkEngine {
	typedef struct {
		VkImage image;
		VkImageView view;
	} SwapChainBuffer;

	typedef struct {
		VkCommandBuffer cmdbuffer;
		VkImage image;
		VkImageAspectFlags aspectMask;
		VkImageLayout oldImageLayout;
		VkImageLayout newImageLayout;
	} SetImageLayoutInfo;

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

		VkResult CreateSwapchain(VkCommandBuffer& cmdBuffer, uint32_t& width, uint32_t& height);

		void SetImageLayout(SetImageLayoutInfo& info);

		VkResult AcquireNextImage(VkSemaphore presentCompleteSemaphore, uint32_t* currentBuffer);
		VkResult QueuePresent(VkQueue queue, uint32_t currentBuffer, VkSemaphore waitSemaphore);

		uint32_t GetQueueNodeIndex() const;
		uint32_t GetImageCount() const;
		VkFormat GetColorFormat() const;
		SwapChainBuffer& GetSwapchainBuffer(int index);

	private:

		VkInstance& m_instance;
		VkPhysicalDevice& m_physicalDevice; 
		VkDevice& m_device;

		VkSurfaceKHR m_surface;
		VkFormat m_colorFormat;
		VkColorSpaceKHR m_colorSpace;

		uint32_t m_imageCount;

		std::vector<VkImage> m_images;
		std::vector<SwapChainBuffer> m_buffers;

		VkSwapchainKHR m_swapchain = VK_NULL_HANDLE;

		uint32_t m_queueNodeIndex = UINT32_MAX;

		PFN_vkGetPhysicalDeviceSurfaceSupportKHR fn_GetPhysicalDeviceSurfaceSupportKHR;
		PFN_vkGetPhysicalDeviceSurfaceFormatsKHR fn_GetPhysicalDeviceSurfaceFormatsKHR;
		PFN_vkGetPhysicalDeviceSurfaceCapabilitiesKHR fn_GetPhysicalDeviceSurfaceCapabilitiesKHR;
		PFN_vkGetPhysicalDeviceSurfacePresentModesKHR fn_GetPhysicalDeviceSurfacePresentModesKHR;
		PFN_vkCreateSwapchainKHR fn_CreateSwapchainKHR;
		PFN_vkDestroySwapchainKHR fn_DestroySwapchainKHR;
		PFN_vkGetSwapchainImagesKHR fn_GetSwapchainImagesKHR;
		PFN_vkAcquireNextImageKHR fn_AcquireNextImageKHR;
		PFN_vkQueuePresentKHR fn_QueuePresentKHR;
	};

}