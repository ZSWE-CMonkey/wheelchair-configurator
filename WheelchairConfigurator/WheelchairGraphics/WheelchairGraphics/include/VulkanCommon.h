#pragma once

#include <vulkan/vulkan.h>

#define VKE_CHECK_RESULT(caller) { VkResult res = (caller); if (res != VK_SUCCESS) return res; }