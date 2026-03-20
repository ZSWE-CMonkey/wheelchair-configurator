#pragma once

#include <VulkanCommon.h>

class ObjectLoader{
public:
#if defined(__ANDROID__)
	static VkResult LoadShader(AAssetManager* assetManager, const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out);
#else
	static VkResult LoadShader(const char* fileName, VkDevice device, VkShaderStageFlagBits stage, VkShaderModule& out);
#endif
private:

};