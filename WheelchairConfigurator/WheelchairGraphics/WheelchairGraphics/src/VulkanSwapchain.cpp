#include "VulkanSwapchain.h"

#include <vector>
#include <stdexcept>

using namespace VkEngine;

VkEngine::VulkanSwapchain::VulkanSwapchain(VkInstance& instance, VkPhysicalDevice& physicalDevice, VkDevice& device) :
	m_instance(instance),
	m_physicalDevice(physicalDevice),
	m_device(device)
{
	fn_GetPhysicalDeviceSurfaceSupportKHR = (PFN_vkGetPhysicalDeviceSurfaceSupportKHR)vkGetInstanceProcAddr(instance, "vk""GetPhysicalDeviceSurfaceSupportKHR"); 
	if (fn_GetPhysicalDeviceSurfaceSupportKHR == 0) {
		throw std::runtime_error("Function was not loaded");
	}
	fn_GetPhysicalDeviceSurfaceFormatsKHR = (PFN_vkGetPhysicalDeviceSurfaceFormatsKHR)vkGetInstanceProcAddr(instance, "vk""GetPhysicalDeviceSurfaceFormatsKHR"); 
	if (fn_GetPhysicalDeviceSurfaceFormatsKHR == 0) {
		throw std::runtime_error("Function was not loaded");
	}
}

VkEngine::VulkanSwapchain::~VulkanSwapchain()
{
}

#ifdef _WIN32
VkResult VkEngine::VulkanSwapchain::CreateSurface(void* platformHandle, void* platformWindow)
#elif __ANDROID__
VkResult VkEngine::VulkanSwapchain::CreateSurface(ANativeWindow* window)
#else
VkResult VkEngine::VulkanSwapchain::CreateSurface()
#endif
{
#ifdef _WIN32
	VkWin32SurfaceCreateInfoKHR surfaceCreateInfo = {};
	surfaceCreateInfo.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
	surfaceCreateInfo.hinstance = (HINSTANCE)platformHandle;
	surfaceCreateInfo.hwnd = (HWND)platformWindow;
	return vkCreateWin32SurfaceKHR(m_instance, &surfaceCreateInfo, nullptr, &m_surface);
#elif __ANDROID__;
	VkAndroidSurfaceCreateInfoKHR surfaceCreateInfo = {};
	surfaceCreateInfo.sType = VK_STRUCTURE_TYPE_ANDROID_SURFACE_CREATE_INFO_KHR;
	surfaceCreateInfo.window = window;
	return vkCreateAndroidSurfaceKHR(m_instance, &surfaceCreateInfo, NULL, &m_surface);
#else
	return VK_ERROR_INITIALIZATION_FAILED;
#endif
}

VkResult VkEngine::VulkanSwapchain::InitSurface()
{
	uint32_t queueCount;
	vkGetPhysicalDeviceQueueFamilyProperties(m_physicalDevice, &queueCount, NULL);
	if (queueCount < 1)
		return VK_ERROR_INITIALIZATION_FAILED;

	std::vector<VkQueueFamilyProperties> queueProps(queueCount);
	vkGetPhysicalDeviceQueueFamilyProperties(m_physicalDevice, &queueCount, queueProps.data());

	std::vector<VkBool32> supportsPresent(queueCount);
	for (uint32_t i = 0; i < queueCount; i++)
	{
		fn_GetPhysicalDeviceSurfaceSupportKHR(m_physicalDevice, i, m_surface, &supportsPresent[i]);
	}

	uint32_t graphicsQueueNodeIndex = UINT32_MAX;
	uint32_t presentQueueNodeIndex = UINT32_MAX;
	for (uint32_t i = 0; i < queueCount; i++)
	{
		if ((queueProps[i].queueFlags & VK_QUEUE_GRAPHICS_BIT) != 0)
		{
			if (graphicsQueueNodeIndex == UINT32_MAX)
			{
				graphicsQueueNodeIndex = i;
			}

			if (supportsPresent[i] == VK_TRUE)
			{
				graphicsQueueNodeIndex = i;
				presentQueueNodeIndex = i;
				break;
			}
		}
	}
	if (presentQueueNodeIndex == UINT32_MAX)
	{
		for (uint32_t i = 0; i < queueCount; ++i)
		{
			if (supportsPresent[i] == VK_TRUE)
			{
				presentQueueNodeIndex = i;
				break;
			}
		}
	}

	if (graphicsQueueNodeIndex == UINT32_MAX || presentQueueNodeIndex == UINT32_MAX)
	{
		return VK_ERROR_INITIALIZATION_FAILED;
	}

	if (graphicsQueueNodeIndex != presentQueueNodeIndex)
	{
		return VK_ERROR_INITIALIZATION_FAILED;
	}

	m_queueNodeIndex = graphicsQueueNodeIndex;

	uint32_t formatCount;
	VKE_CHECK_RESULT(fn_GetPhysicalDeviceSurfaceFormatsKHR(m_physicalDevice, m_surface, &formatCount, NULL));
	if (formatCount <= 0)
		return VK_ERROR_INITIALIZATION_FAILED;

	std::vector<VkSurfaceFormatKHR> surfaceFormats(formatCount);
	VKE_CHECK_RESULT(fn_GetPhysicalDeviceSurfaceFormatsKHR(m_physicalDevice, m_surface, &formatCount, surfaceFormats.data()));

	if ((formatCount == 1) && (surfaceFormats[0].format == VK_FORMAT_UNDEFINED))
	{
		m_colorFormat = VK_FORMAT_B8G8R8A8_UNORM;
	}
	else
	{
		m_colorFormat = surfaceFormats[0].format;
	}
	m_colorSpace = surfaceFormats[0].colorSpace;

	return VK_SUCCESS;
}
