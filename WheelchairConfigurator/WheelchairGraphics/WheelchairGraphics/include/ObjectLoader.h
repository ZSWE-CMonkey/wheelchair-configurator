#pragma once

#include <VulkanCommon.h>
#include <memory>
#include "TextureHandle.h"
#include "MeshHandle.h"

namespace VkLoader {

	class ObjectLoader {
	public:

		static std::unique_ptr<TextureHandle> CreateTextureHandle(VkPhysicalDevice& physicalDevice, VkDevice& device, VkQueue& queue, VkCommandPool& cmdPool);
		
		static std::unique_ptr<MeshHandle> CreateMeshHandle();

		static VkResult LoadShader(const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out);
	private:

	};

}