#include "VulkanEngine.h"

#include <vector>
#include <array>

using namespace VkEngine;

VulkanEngine::~VulkanEngine()
{
	vkDeviceWaitIdle(m_device);
	vkDestroySemaphore(m_device, m_semaphores.presentComplete, nullptr);
	vkDestroySemaphore(m_device, m_semaphores.renderComplete, nullptr);
	vkDestroyDevice(m_device, nullptr);
	vkDestroyInstance(m_instance, nullptr);
}

VkResult VulkanEngine::InitVulkan(std::string appName)
{
	VKE_CHECK_RESULT(CreateInstance(appName));
	//TODO: for android is needed to load vulkan function, bc android loads it on runtime. place it here
	uint32_t graphicsQueueIndex{};
	VKE_CHECK_RESULT(CheckPhysicalDevices(graphicsQueueIndex));
	VKE_CHECK_RESULT(CreateDevice(graphicsQueueIndex));

	vkGetPhysicalDeviceMemoryProperties(m_physicalDevice, &m_deviceMemoryProperties);
	vkGetDeviceQueue(m_device, graphicsQueueIndex, 0, &m_queue);

	if (!GetDepthFormat())
		return VK_ERROR_INITIALIZATION_FAILED;

	VKE_CHECK_RESULT(CreateVulkanSemaphore());

	CreateSumbitInfo();

	return VK_SUCCESS;
}
#if _WIN32
VkResult VulkanEngine::InitSwapchain(void* platformHandle, void* platformWindow)
#elif __ANDROID__
VkResult VulkanEngine::InitSwapchain(ANativeWindow* window)
#else
VkResult VulkanEngine::InitSwapchain()
#endif
{
	m_vulkanSwapchain = std::make_unique<VulkanSwapchain>(m_instance, m_physicalDevice, m_device);
	
	//TODO: android version implement as well!!
	VKE_CHECK_RESULT(m_vulkanSwapchain->CreateSurface(platformHandle, platformWindow));

	VKE_CHECK_RESULT(m_vulkanSwapchain->InitSurface());
	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::Prepare()
{
	uint32_t width = 600, height = 600;//todo w/h take from device

	VKE_CHECK_RESULT(CreateCommandPool());
	VKE_CHECK_RESULT(CreateSetupCommandBuffer());
	VKE_CHECK_RESULT(m_vulkanSwapchain->CreateSwapchain(m_setupCmdBuffer, width, height));
	//Todo: complete

	return VK_SUCCESS;
}

VkResult VulkanEngine::CreateInstance(std::string appName)
{
	VkApplicationInfo appInfo = {};
	appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
	appInfo.pApplicationName = appName.c_str();
	appInfo.pEngineName = appName.c_str();
	appInfo.apiVersion = VK_API_VERSION_1_0;

	std::vector<const char*> enabledExtensions = { VK_KHR_SURFACE_EXTENSION_NAME };

#if defined(_WIN32)
	enabledExtensions.push_back(VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
#elif defined(__ANDROID__)
	enabledExtensions.push_back(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
#endif

	VkInstanceCreateInfo instanceCreateInfo = {};
	instanceCreateInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
	instanceCreateInfo.pNext = nullptr;
	instanceCreateInfo.pApplicationInfo = &appInfo;
	instanceCreateInfo.enabledExtensionCount = (uint32_t)enabledExtensions.size();
	instanceCreateInfo.ppEnabledExtensionNames = enabledExtensions.data();

	return vkCreateInstance(&instanceCreateInfo, nullptr, &m_instance);
}

VkResult VulkanEngine::CheckPhysicalDevices(uint32_t& outGraphicsQueueIndex)
{
	uint32_t gpuCount = 0;
	VKE_CHECK_RESULT(vkEnumeratePhysicalDevices(m_instance, &gpuCount, nullptr));
	if (gpuCount == 0)
		return VK_ERROR_INITIALIZATION_FAILED;

	std::vector<VkPhysicalDevice> physicalDevices(gpuCount);
	VKE_CHECK_RESULT(vkEnumeratePhysicalDevices(m_instance, &gpuCount, physicalDevices.data()));

	//using first GPU, later handle multiple gpus??? probably not for android devices
	m_physicalDevice = physicalDevices[0];

	uint32_t graphicsQueueIndex = 0;
	uint32_t queueCount;
	vkGetPhysicalDeviceQueueFamilyProperties(m_physicalDevice, &queueCount, NULL);

	if (queueCount < 1)
		return VK_ERROR_INITIALIZATION_FAILED;

	std::vector<VkQueueFamilyProperties> queueProps;
	queueProps.resize(queueCount);
	vkGetPhysicalDeviceQueueFamilyProperties(m_physicalDevice, &queueCount, queueProps.data());

	for (graphicsQueueIndex = 0; graphicsQueueIndex < queueCount; graphicsQueueIndex++)
	{
		if (queueProps[graphicsQueueIndex].queueFlags & VK_QUEUE_GRAPHICS_BIT)
			break;
	}
	if (graphicsQueueIndex >= queueCount)
		return VK_ERROR_INITIALIZATION_FAILED;

	outGraphicsQueueIndex = graphicsQueueIndex;
	return VK_SUCCESS;
}

VkResult VulkanEngine::CreateDevice(uint32_t graphicsQueueIndex)
{
	std::array<float, 1> queuePriorities = { 0.0f };
	VkDeviceQueueCreateInfo queueCreateInfo = {};
	queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
	queueCreateInfo.queueFamilyIndex = graphicsQueueIndex;
	queueCreateInfo.queueCount = 1;
	queueCreateInfo.pQueuePriorities = queuePriorities.data();

	std::vector<const char*> enabledExtensions = { VK_KHR_SWAPCHAIN_EXTENSION_NAME };

	VkDeviceCreateInfo deviceCreateInfo = {};
	deviceCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
	deviceCreateInfo.pNext = NULL;
	deviceCreateInfo.queueCreateInfoCount = 1;
	deviceCreateInfo.pQueueCreateInfos = &queueCreateInfo;
	deviceCreateInfo.pEnabledFeatures = NULL;
	deviceCreateInfo.enabledExtensionCount = (uint32_t)enabledExtensions.size();
	deviceCreateInfo.ppEnabledExtensionNames = enabledExtensions.data();

	return vkCreateDevice(m_physicalDevice, &deviceCreateInfo, nullptr, &m_device);
}

VkResult VulkanEngine::CreateVulkanSemaphore()
{
	VkSemaphoreCreateInfo semaphoreCreateInfo = {};
	semaphoreCreateInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreCreateInfo.pNext = NULL;
	semaphoreCreateInfo.flags = 0;
	
	VKE_CHECK_RESULT(vkCreateSemaphore(m_device, &semaphoreCreateInfo, nullptr, &m_semaphores.presentComplete));
	return vkCreateSemaphore(m_device, &semaphoreCreateInfo, nullptr, &m_semaphores.renderComplete);
}

VkResult VkEngine::VulkanEngine::CreateCommandPool()
{
	VkCommandPoolCreateInfo cmdPoolInfo = {};
	cmdPoolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	cmdPoolInfo.queueFamilyIndex = m_vulkanSwapchain->GetQueueNodeIndex();
	cmdPoolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
	return vkCreateCommandPool(m_device, &cmdPoolInfo, nullptr, &m_cmdPool);
}

VkResult VkEngine::VulkanEngine::CreateSetupCommandBuffer()
{
	VkCommandBufferAllocateInfo commandBufferAllocateInfo = {};
	commandBufferAllocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	commandBufferAllocateInfo.commandPool = m_cmdPool;
	commandBufferAllocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	commandBufferAllocateInfo.commandBufferCount = 1;

	VKE_CHECK_RESULT(vkAllocateCommandBuffers(m_device, &commandBufferAllocateInfo, &m_setupCmdBuffer));

	VkCommandBufferBeginInfo cmdBufInfo = {};
	cmdBufInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

	return vkBeginCommandBuffer(m_setupCmdBuffer, &cmdBufInfo);
}

void VulkanEngine::CreateSumbitInfo()
{
	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.pNext = NULL;
	submitInfo.pWaitDstStageMask = &m_submitPipelineStages;
	submitInfo.waitSemaphoreCount = 1;
	submitInfo.pWaitSemaphores = &m_semaphores.presentComplete;
	submitInfo.signalSemaphoreCount = 1;
	submitInfo.pSignalSemaphores = &m_semaphores.renderComplete;
}

bool VulkanEngine::GetDepthFormat()
{
	std::vector<VkFormat> depthFormats = {
		VK_FORMAT_D32_SFLOAT_S8_UINT,
		VK_FORMAT_D32_SFLOAT,
		VK_FORMAT_D24_UNORM_S8_UINT,
		VK_FORMAT_D16_UNORM_S8_UINT,
		VK_FORMAT_D16_UNORM
	};

	for (auto& format : depthFormats)
	{
		VkFormatProperties formatProps;
		vkGetPhysicalDeviceFormatProperties(m_physicalDevice, format, &formatProps);

		if (formatProps.optimalTilingFeatures & VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT)
		{
			m_depthFormat = format;
			return true;
		}
	}
	return false;
}
