#include "ObjectLoader.h"

#include <iostream>

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



#if defined(__ANDROID__)
VkShaderModule ObjectLoader::LoadShader(AAssetManager* assetManager, const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out)
{
	AAsset* asset = AAssetManager_open(assetManager, fileName, AASSET_MODE_STREAMING);
	assert(asset);
	size_t size = AAsset_getLength(asset);
	assert(size > 0);

	char* shaderCode = new char[size];
	AAsset_read(asset, shaderCode, size);
	AAsset_close(asset);

	VkShaderModuleCreateInfo moduleCreateInfo;
	moduleCreateInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	moduleCreateInfo.pNext = NULL;
	moduleCreateInfo.codeSize = size;
	moduleCreateInfo.pCode = (uint32_t*)shaderCode;
	moduleCreateInfo.flags = 0;

	VK_CHECK_RESULT(vkCreateShaderModule(m_device, &moduleCreateInfo, NULL, &out));

	delete[] shaderCode;

	return VK_SUCCESS;
}
#else
VkResult ObjectLoader::LoadShader(const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out)
{
	size_t size;

	FILE* fp = fopen(fileName, "rb");
	if (!fp)
		return VK_ERROR_UNKNOWN;

	fseek(fp, 0L, SEEK_END);
	size = ftell(fp);

	fseek(fp, 0L, SEEK_SET);

	char* shaderCode = new char[size];
	size_t retval = fread(shaderCode, size, 1, fp);
	if (retval != 1)
		return VK_ERROR_UNKNOWN;
	if (size <= 0)
		return VK_ERROR_UNKNOWN;

	fclose(fp);

	VkShaderModuleCreateInfo moduleCreateInfo;
	moduleCreateInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	moduleCreateInfo.pNext = NULL;
	moduleCreateInfo.codeSize = size;
	moduleCreateInfo.pCode = (uint32_t*)shaderCode;
	moduleCreateInfo.flags = 0;

	VKE_CHECK_RESULT(vkCreateShaderModule(device, &moduleCreateInfo, NULL, &out));

	delete[] shaderCode;

	return VK_SUCCESS;
}
#endif
