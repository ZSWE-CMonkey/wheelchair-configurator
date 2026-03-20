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

VkResult VulkanEngine::InitVulkan(std::string appName, uint32_t width, uint32_t height)
{
	m_width = width;
	m_height = height;

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
	VKE_CHECK_RESULT(CreateCommandPool());
	VKE_CHECK_RESULT(CreateSetupCommandBuffer());
	VKE_CHECK_RESULT(m_vulkanSwapchain->CreateSwapchain(m_setupCmdBuffer, m_width, m_height));
	VKE_CHECK_RESULT(CreateCommandBuffers());
	VKE_CHECK_RESULT(SetupDepthStencil());
	VKE_CHECK_RESULT(SetupRenderPass());
	VKE_CHECK_RESULT(CreatePipelineCache());
	VKE_CHECK_RESULT(SetupFrameBuffer());
	VKE_CHECK_RESULT(FlushSetupCommandBuffer());
	VKE_CHECK_RESULT(CreateSetupCommandBuffer());

	//Here later load texture and mesh etc.


	//Todo: complete it

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

VkResult VkEngine::VulkanEngine::CreateCommandBuffers()
{
	m_drawCmdBuffers.resize(m_vulkanSwapchain->GetImageCount());

	VkCommandBufferAllocateInfo commandBufferAllocateInfo = {};
	commandBufferAllocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	commandBufferAllocateInfo.commandPool = m_cmdPool;
	commandBufferAllocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	commandBufferAllocateInfo.commandBufferCount = (uint32_t)m_drawCmdBuffers.size();

	VKE_CHECK_RESULT(vkAllocateCommandBuffers(m_device, &commandBufferAllocateInfo, m_drawCmdBuffers.data()));

	commandBufferAllocateInfo.commandBufferCount = 1;

	VKE_CHECK_RESULT(vkAllocateCommandBuffers(m_device, &commandBufferAllocateInfo, &m_prePresentCmdBuffer));
	return vkAllocateCommandBuffers(m_device, &commandBufferAllocateInfo, &m_postPresentCmdBuffer);
}

VkResult VkEngine::VulkanEngine::SetupDepthStencil()
{
	VkImageCreateInfo image = {};
	image.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	image.pNext = NULL;
	image.imageType = VK_IMAGE_TYPE_2D;
	image.format = m_depthFormat;
	image.extent = { m_width, m_height, 1 };
	image.mipLevels = 1;
	image.arrayLayers = 1;
	image.samples = VK_SAMPLE_COUNT_1_BIT;
	image.tiling = VK_IMAGE_TILING_OPTIMAL;
	image.usage = VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
	image.flags = 0;

	VkMemoryAllocateInfo mem_alloc = {};
	mem_alloc.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
	mem_alloc.pNext = NULL;
	mem_alloc.allocationSize = 0;
	mem_alloc.memoryTypeIndex = 0;

	VkImageViewCreateInfo depthStencilView = {};
	depthStencilView.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
	depthStencilView.pNext = NULL;
	depthStencilView.viewType = VK_IMAGE_VIEW_TYPE_2D;
	depthStencilView.format = m_depthFormat;
	depthStencilView.flags = 0;
	depthStencilView.subresourceRange = {};
	depthStencilView.subresourceRange.aspectMask = VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT;
	depthStencilView.subresourceRange.baseMipLevel = 0;
	depthStencilView.subresourceRange.levelCount = 1;
	depthStencilView.subresourceRange.baseArrayLayer = 0;
	depthStencilView.subresourceRange.layerCount = 1;

	VkMemoryRequirements memReqs;

	VKE_CHECK_RESULT(vkCreateImage(m_device, &image, nullptr, &m_depthStencil.image));
	vkGetImageMemoryRequirements(m_device, m_depthStencil.image, &memReqs);
	mem_alloc.allocationSize = memReqs.size;
	mem_alloc.memoryTypeIndex = GetMemoryType(memReqs.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
	VKE_CHECK_RESULT(vkAllocateMemory(m_device, &mem_alloc, nullptr, &m_depthStencil.mem));

	VKE_CHECK_RESULT(vkBindImageMemory(m_device, m_depthStencil.image, m_depthStencil.mem, 0));

	SetImageLayoutInfo setImageLayoutInfo{};
	setImageLayoutInfo.cmdbuffer = m_setupCmdBuffer;
	setImageLayoutInfo.image = m_depthStencil.image;
	setImageLayoutInfo.aspectMask = VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT;
	setImageLayoutInfo.oldImageLayout = VK_IMAGE_LAYOUT_UNDEFINED;
	setImageLayoutInfo.newImageLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

	m_vulkanSwapchain->SetImageLayout(setImageLayoutInfo);


	depthStencilView.image = m_depthStencil.image;
	return vkCreateImageView(m_device, &depthStencilView, nullptr, &m_depthStencil.view);
}

VkResult VkEngine::VulkanEngine::SetupRenderPass()
{
	VkAttachmentDescription attachments[2] = {};

	attachments[0].format = m_vulkanSwapchain->GetColorFormat();
	attachments[0].samples = VK_SAMPLE_COUNT_1_BIT;
	attachments[0].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
	attachments[0].storeOp = VK_ATTACHMENT_STORE_OP_STORE;
	attachments[0].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
	attachments[0].stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
	attachments[0].initialLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
	attachments[0].finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

	attachments[1].format = m_depthFormat;
	attachments[1].samples = VK_SAMPLE_COUNT_1_BIT;
	attachments[1].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
	attachments[1].storeOp = VK_ATTACHMENT_STORE_OP_STORE;
	attachments[1].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
	attachments[1].stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
	attachments[1].initialLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
	attachments[1].finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

	VkAttachmentReference colorReference = {};
	colorReference.attachment = 0;
	colorReference.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

	VkAttachmentReference depthReference = {};
	depthReference.attachment = 1;
	depthReference.layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

	VkSubpassDescription subpass = {};
	subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
	subpass.flags = 0;
	subpass.inputAttachmentCount = 0;
	subpass.pInputAttachments = NULL;
	subpass.colorAttachmentCount = 1;
	subpass.pColorAttachments = &colorReference;
	subpass.pResolveAttachments = NULL;
	subpass.pDepthStencilAttachment = &depthReference;
	subpass.preserveAttachmentCount = 0;
	subpass.pPreserveAttachments = NULL;

	VkRenderPassCreateInfo renderPassInfo = {};
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
	renderPassInfo.pNext = NULL;
	renderPassInfo.attachmentCount = 2;
	renderPassInfo.pAttachments = attachments;
	renderPassInfo.subpassCount = 1;
	renderPassInfo.pSubpasses = &subpass;
	renderPassInfo.dependencyCount = 0;
	renderPassInfo.pDependencies = NULL;

	return vkCreateRenderPass(m_device, &renderPassInfo, nullptr, &m_renderPass);
}

VkResult VkEngine::VulkanEngine::CreatePipelineCache()
{
	VkPipelineCacheCreateInfo pipelineCacheCreateInfo = {};
	pipelineCacheCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_CACHE_CREATE_INFO;
	return vkCreatePipelineCache(m_device, &pipelineCacheCreateInfo, nullptr, &m_pipelineCache);
}

VkResult VkEngine::VulkanEngine::SetupFrameBuffer()
{
	VkImageView attachments[2];

	attachments[1] = m_depthStencil.view;

	VkFramebufferCreateInfo frameBufferCreateInfo = {};
	frameBufferCreateInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
	frameBufferCreateInfo.pNext = NULL;
	frameBufferCreateInfo.renderPass = m_renderPass;
	frameBufferCreateInfo.attachmentCount = 2;
	frameBufferCreateInfo.pAttachments = attachments;
	frameBufferCreateInfo.width = m_width;
	frameBufferCreateInfo.height = m_height;
	frameBufferCreateInfo.layers = 1;

	m_frameBuffers.resize(m_vulkanSwapchain->GetImageCount());
	for (uint32_t i = 0; i < m_frameBuffers.size(); i++)
	{
		attachments[0] = m_vulkanSwapchain->GetSwapchainBuffer(i).view;
		VKE_CHECK_RESULT(vkCreateFramebuffer(m_device, &frameBufferCreateInfo, nullptr, &m_frameBuffers[i]));
	}
	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::FlushSetupCommandBuffer()
{
	if (m_setupCmdBuffer == VK_NULL_HANDLE)
		return VK_SUCCESS;

	VKE_CHECK_RESULT(vkEndCommandBuffer(m_setupCmdBuffer));

	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &m_setupCmdBuffer;

	VKE_CHECK_RESULT(vkQueueSubmit(m_queue, 1, &submitInfo, VK_NULL_HANDLE));
	VKE_CHECK_RESULT(vkQueueWaitIdle(m_queue));

	vkFreeCommandBuffers(m_device, m_cmdPool, 1, &m_setupCmdBuffer);
	m_setupCmdBuffer = VK_NULL_HANDLE;

	return VK_SUCCESS;
}

uint32_t VkEngine::VulkanEngine::GetMemoryType(uint32_t typeBits, VkFlags properties)
{
	for (uint32_t i = 0; i < 32; i++)
	{
		if ((typeBits & 1) == 1)
		{
			if ((m_deviceMemoryProperties.memoryTypes[i].propertyFlags & properties) == properties)
			{
				return i;
			}
		}
		typeBits >>= 1;
	}
	return 0;
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
