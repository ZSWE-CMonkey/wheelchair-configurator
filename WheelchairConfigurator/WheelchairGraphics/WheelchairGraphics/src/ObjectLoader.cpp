#include "ObjectLoader.h"

#include <iostream>

#include "../assetResources/assetResource.h"

using namespace VkLoader;

std::unique_ptr<TextureHandle> VkLoader::ObjectLoader::CreateTextureHandle(VkPhysicalDevice& physicalDevice, VkDevice& device, VkQueue& queue, VkCommandPool& cmdPool)
{
	std::unique_ptr<TextureHandle> res = std::make_unique<TextureHandle>(physicalDevice, device, queue, cmdPool);
	if (res->Initialize() != VK_SUCCESS) {
		res = nullptr;
		return nullptr;
	}
	return std::move(res);
}

std::unique_ptr<MeshHandle> VkLoader::ObjectLoader::CreateMeshHandle()
{
	std::unique_ptr<MeshHandle> res = std::make_unique<MeshHandle>();
	return std::move(res);
}

VkResult ObjectLoader::LoadShader(const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out)
{
	auto shader = getEmbeddedAsset(fileName);

	VkShaderModuleCreateInfo moduleCreateInfo;
	moduleCreateInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	moduleCreateInfo.pNext = NULL;
	moduleCreateInfo.codeSize = shader->size;
	moduleCreateInfo.pCode = (uint32_t*)shader->data;
	moduleCreateInfo.flags = 0;

	VKE_CHECK_RESULT(vkCreateShaderModule(device, &moduleCreateInfo, NULL, &out));
	return VK_SUCCESS;
}
