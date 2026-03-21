#pragma once

#include <vulkan/vulkan.h>

#define VKE_CHECK_RESULT(caller) { VkResult res = (caller); if (res != VK_SUCCESS) return res; }

namespace VkEngine {
	struct VulkanTexture
	{
		VkSampler sampler;
		VkImage image;
		VkImageLayout imageLayout;
		VkDeviceMemory deviceMemory;
		VkImageView view;
		uint32_t width, height;
		uint32_t mipLevels;
		uint32_t layerCount;
	};
}