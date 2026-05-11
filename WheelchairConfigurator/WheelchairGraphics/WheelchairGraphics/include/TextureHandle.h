#pragma once

#include "VulkanCommon.h"
#include <string>

namespace VkLoader {
	class TextureHandle {
	public:
		TextureHandle(VkPhysicalDevice& physicalDevice, VkDevice& device, VkQueue& queue, VkCommandPool& cmdPool);
		VkResult Initialize();

		VkResult LoadTexture(std::string filename, VkFormat format, VkEngine::VulkanTexture* texture);
		VkResult LoadTextureFromFile(std::string filepath, VkFormat format, VkEngine::VulkanTexture* texture);

		void SetImageLayout(VkCommandBuffer cmdbuffer, VkImage image, VkImageAspectFlags aspectMask, VkImageLayout oldImageLayout, VkImageLayout newImageLayout, VkImageSubresourceRange subresourceRange);

	private:
		VkResult LoadFallbackTexture(VkEngine::VulkanTexture* texture);
		uint32_t GetMemoryType(uint32_t typeBits, VkFlags properties);

		VkPhysicalDevice& m_physicalDevice;
		VkDevice& m_device;
		VkQueue& m_queue;
		VkCommandPool& m_cmdPool;


		VkCommandBuffer m_cmdBuffer;
		VkPhysicalDeviceMemoryProperties m_deviceMemoryProperties;
	};
}