#include "VulkanSwapchain.h"

#include <vector>
#include <stdexcept>

using namespace VkEngine;

VkEngine::VulkanSwapchain::VulkanSwapchain(VkInstance& instance, VkPhysicalDevice& physicalDevice, VkDevice& device) :
	m_instance(instance),
	m_physicalDevice(physicalDevice),
	m_device(device)
{
	GET_INSTANCE_PROC_ADDR(instance, GetPhysicalDeviceSurfaceSupportKHR);
	GET_INSTANCE_PROC_ADDR(instance, GetPhysicalDeviceSurfaceCapabilitiesKHR);
	GET_INSTANCE_PROC_ADDR(instance, GetPhysicalDeviceSurfaceFormatsKHR);
	GET_INSTANCE_PROC_ADDR(instance, GetPhysicalDeviceSurfacePresentModesKHR);
	GET_DEVICE_PROC_ADDR(device, CreateSwapchainKHR);
	GET_DEVICE_PROC_ADDR(device, DestroySwapchainKHR);
	GET_DEVICE_PROC_ADDR(device, GetSwapchainImagesKHR);
	GET_DEVICE_PROC_ADDR(device, AcquireNextImageKHR);
	GET_DEVICE_PROC_ADDR(device, QueuePresentKHR);
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

VkResult VkEngine::VulkanSwapchain::CreateSwapchain(VkCommandBuffer& cmdBuffer, uint32_t& width, uint32_t& height)
{
	//TODO: refactor! >:(

	VkSwapchainKHR oldSwapchain = m_swapchain;

	VkSurfaceCapabilitiesKHR surfCaps;
	VKE_CHECK_RESULT(fn_GetPhysicalDeviceSurfaceCapabilitiesKHR(m_physicalDevice, m_surface, &surfCaps));

	uint32_t presentModeCount;
	VKE_CHECK_RESULT(fn_GetPhysicalDeviceSurfacePresentModesKHR(m_physicalDevice, m_surface, &presentModeCount, NULL));
	if (presentModeCount <= 0)
		return VK_ERROR_INITIALIZATION_FAILED;

	std::vector<VkPresentModeKHR> presentModes(presentModeCount);

	VKE_CHECK_RESULT(fn_GetPhysicalDeviceSurfacePresentModesKHR(m_physicalDevice, m_surface, &presentModeCount, presentModes.data()));

	VkExtent2D swapchainExtent = {};

	if (surfCaps.currentExtent.width == -1)
	{
		swapchainExtent.width = width;
		swapchainExtent.height = height;
	}
	else
	{
		swapchainExtent = surfCaps.currentExtent;
		width = surfCaps.currentExtent.width;
		height = surfCaps.currentExtent.height;
	}

	VkPresentModeKHR swapchainPresentMode = VK_PRESENT_MODE_FIFO_KHR;
	for (size_t i = 0; i < presentModeCount; i++)
	{
		if (presentModes[i] == VK_PRESENT_MODE_MAILBOX_KHR)
		{
			swapchainPresentMode = VK_PRESENT_MODE_MAILBOX_KHR;
			break;
		}
		if ((swapchainPresentMode != VK_PRESENT_MODE_MAILBOX_KHR) && (presentModes[i] == VK_PRESENT_MODE_IMMEDIATE_KHR))
		{
			swapchainPresentMode = VK_PRESENT_MODE_IMMEDIATE_KHR;
		}
	}

	uint32_t desiredNumberOfSwapchainImages = surfCaps.minImageCount + 1;
	if ((surfCaps.maxImageCount > 0) && (desiredNumberOfSwapchainImages > surfCaps.maxImageCount))
	{
		desiredNumberOfSwapchainImages = surfCaps.maxImageCount;
	}

	VkSurfaceTransformFlagsKHR preTransform;
	if (surfCaps.supportedTransforms & VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR)
	{
		preTransform = VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR;
	}
	else
	{
		preTransform = surfCaps.currentTransform;
	}

	VkSwapchainCreateInfoKHR swapchainCI = {};
	swapchainCI.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
	swapchainCI.pNext = NULL;
	swapchainCI.surface = m_surface;
	swapchainCI.minImageCount = desiredNumberOfSwapchainImages;
	swapchainCI.imageFormat = m_colorFormat;
	swapchainCI.imageColorSpace = m_colorSpace;
	swapchainCI.imageExtent = { swapchainExtent.width, swapchainExtent.height };
	swapchainCI.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
	swapchainCI.preTransform = (VkSurfaceTransformFlagBitsKHR)preTransform;
	swapchainCI.imageArrayLayers = 1;
	swapchainCI.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
	swapchainCI.queueFamilyIndexCount = 0;
	swapchainCI.pQueueFamilyIndices = NULL;
	swapchainCI.presentMode = swapchainPresentMode;
	swapchainCI.oldSwapchain = oldSwapchain;
	swapchainCI.clipped = true;
	swapchainCI.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;

	VKE_CHECK_RESULT(fn_CreateSwapchainKHR(m_device, &swapchainCI, nullptr, &m_swapchain));

	if (oldSwapchain != VK_NULL_HANDLE)
	{
		for (uint32_t i = 0; i < m_imageCount; i++)
		{
			vkDestroyImageView(m_device, m_buffers[i].view, nullptr);
		}
		fn_DestroySwapchainKHR(m_device, oldSwapchain, nullptr);
	}

	VKE_CHECK_RESULT(fn_GetSwapchainImagesKHR(m_device, m_swapchain, &m_imageCount, NULL));

	m_images.resize(m_imageCount);
	VKE_CHECK_RESULT(fn_GetSwapchainImagesKHR(m_device, m_swapchain, &m_imageCount, m_images.data()));

	m_buffers.resize(m_imageCount);
	for (uint32_t i = 0; i < m_imageCount; i++)
	{
		VkImageViewCreateInfo colorAttachmentView = {};
		colorAttachmentView.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
		colorAttachmentView.pNext = NULL;
		colorAttachmentView.format = m_colorFormat;
		colorAttachmentView.components = {
			VK_COMPONENT_SWIZZLE_R,
			VK_COMPONENT_SWIZZLE_G,
			VK_COMPONENT_SWIZZLE_B,
			VK_COMPONENT_SWIZZLE_A
		};
		colorAttachmentView.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		colorAttachmentView.subresourceRange.baseMipLevel = 0;
		colorAttachmentView.subresourceRange.levelCount = 1;
		colorAttachmentView.subresourceRange.baseArrayLayer = 0;
		colorAttachmentView.subresourceRange.layerCount = 1;
		colorAttachmentView.viewType = VK_IMAGE_VIEW_TYPE_2D;
		colorAttachmentView.flags = 0;

		m_buffers[i].image = m_images[i];

		SetImageLayoutInfo setImageLayoutInfo{};
		setImageLayoutInfo.cmdbuffer = cmdBuffer;
		setImageLayoutInfo.image = m_buffers[i].image;
		setImageLayoutInfo.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		setImageLayoutInfo.oldImageLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		setImageLayoutInfo.newImageLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

		SetImageLayout(setImageLayoutInfo);

		colorAttachmentView.image = m_buffers[i].image;

		VKE_CHECK_RESULT(vkCreateImageView(m_device, &colorAttachmentView, nullptr, &m_buffers[i].view));
	}
	return VK_SUCCESS;
}

uint32_t VkEngine::VulkanSwapchain::GetQueueNodeIndex() const
{
	return m_queueNodeIndex;
}

uint32_t VkEngine::VulkanSwapchain::GetImageCount() const
{
	return m_imageCount;
}

VkFormat VkEngine::VulkanSwapchain::GetColorFormat() const
{
	return m_colorFormat;
}

SwapChainBuffer& VkEngine::VulkanSwapchain::GetSwapchainBuffer(int index)
{
	return m_buffers[index];
}

void VkEngine::VulkanSwapchain::SetImageLayout(SetImageLayoutInfo& info)
{
	VkImageSubresourceRange subresourceRange = {};
	subresourceRange.aspectMask = info.aspectMask;
	subresourceRange.baseMipLevel = 0;
	subresourceRange.levelCount = 1;
	subresourceRange.layerCount = 1;


	VkImageMemoryBarrier imageMemoryBarrier = {};
	imageMemoryBarrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
	imageMemoryBarrier.pNext = NULL;
	imageMemoryBarrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	imageMemoryBarrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	imageMemoryBarrier.oldLayout = info.oldImageLayout;
	imageMemoryBarrier.newLayout = info.newImageLayout;
	imageMemoryBarrier.image = info.image;
	imageMemoryBarrier.subresourceRange = subresourceRange;


	if (info.oldImageLayout == VK_IMAGE_LAYOUT_PREINITIALIZED)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_HOST_WRITE_BIT | VK_ACCESS_TRANSFER_WRITE_BIT;
	}

	if (info.oldImageLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
	}

	if (info.oldImageLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
	}

	if (info.oldImageLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
	}

	if (info.oldImageLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
	}

	if (info.newImageLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
	{
		imageMemoryBarrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
	}

	if (info.newImageLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = imageMemoryBarrier.srcAccessMask | VK_ACCESS_TRANSFER_READ_BIT;
		imageMemoryBarrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
	}

	if (info.newImageLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
	{
		imageMemoryBarrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
	}

	if (info.newImageLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
	{
		imageMemoryBarrier.dstAccessMask = imageMemoryBarrier.dstAccessMask | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
	}

	if (info.newImageLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		imageMemoryBarrier.srcAccessMask = VK_ACCESS_HOST_WRITE_BIT | VK_ACCESS_TRANSFER_WRITE_BIT;
		imageMemoryBarrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
	}

	VkPipelineStageFlags srcStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
	VkPipelineStageFlags destStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;

	vkCmdPipelineBarrier(
		info.cmdbuffer,
		srcStageFlags,
		destStageFlags,
		0,
		0, nullptr,
		0, nullptr,
		1, &imageMemoryBarrier);
}
