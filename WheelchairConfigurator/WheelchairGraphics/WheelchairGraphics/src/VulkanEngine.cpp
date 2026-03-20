#include "VulkanEngine.h"

#include <vector>
#include <array>

#include "ObjectLoader.h"

using namespace VkEngine;

namespace {
	VkVertexInputAttributeDescription GetVertexInputAttributeDescription(
		uint32_t binding,
		uint32_t location,
		VkFormat format,
		uint32_t offset)
	{
		VkVertexInputAttributeDescription vInputAttribDescription = {};
		vInputAttribDescription.location = location;
		vInputAttribDescription.binding = binding;
		vInputAttribDescription.format = format;
		vInputAttribDescription.offset = offset;
		return vInputAttribDescription;
	}
}


VulkanEngine::~VulkanEngine()
{
	m_canRender = false;
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

	//Here later load texture and mesh via ObjectLoader.

	//TESTING: REMOVE AFTER loader is implemented, this is needed bc current setup doesnt allowe null textures:
	CreateDummyTexture();
	//

	SetupVertexDescriptions();
	VKE_CHECK_RESULT(PrepareUniformBuffers());
	VKE_CHECK_RESULT(SetupDescriptorSetLayout());
	VKE_CHECK_RESULT(PreparePipelines());
	VKE_CHECK_RESULT(SetupDescriptorPool());
	VKE_CHECK_RESULT(SetupDescriptorSet());
	VKE_CHECK_RESULT(BuildCommandBuffers());

	m_canRender = true;
	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::Render()
{
	if (!m_canRender)
		return VK_SUCCESS; //bc is not initialized, not problem

	VKE_CHECK_RESULT(m_vulkanSwapchain->AcquireNextImage(m_semaphores.presentComplete, &m_currentBuffer));

	VKE_CHECK_RESULT(SubmitPostPresentBarrier(m_vulkanSwapchain->GetSwapchainBuffer(m_currentBuffer).image));

	m_submitInfo.commandBufferCount = 1;
	m_submitInfo.pCommandBuffers = &m_drawCmdBuffers[m_currentBuffer];

	VKE_CHECK_RESULT(vkQueueSubmit(m_queue, 1, &m_submitInfo, VK_NULL_HANDLE));

	VKE_CHECK_RESULT(SubmitPrePresentBarrier(m_vulkanSwapchain->GetSwapchainBuffer(m_currentBuffer).image));

	VKE_CHECK_RESULT(m_vulkanSwapchain->QueuePresent(m_queue, m_currentBuffer, m_semaphores.renderComplete));

	return vkQueueWaitIdle(m_queue);
}

void VkEngine::VulkanEngine::CreateDummyTexture()
{
	uint32_t pixel = 0xFFFFFFFF;

	VkBuffer stagingBuffer;
	VkDeviceMemory stagingMemory;

	VkBufferCreateInfo bufferInfo{};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.size = sizeof(uint32_t);
	bufferInfo.usage = VK_BUFFER_USAGE_TRANSFER_SRC_BIT;
	bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

	vkCreateBuffer(m_device, &bufferInfo, nullptr, &stagingBuffer);

	VkMemoryRequirements memReq;
	vkGetBufferMemoryRequirements(m_device, stagingBuffer, &memReq);

	VkMemoryAllocateInfo allocInfo{};
	allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
	allocInfo.allocationSize = memReq.size;
	allocInfo.memoryTypeIndex =
		GetMemoryType(
			memReq.memoryTypeBits,
			VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);

	vkAllocateMemory(m_device, &allocInfo, nullptr, &stagingMemory);
	vkBindBufferMemory(m_device, stagingBuffer, stagingMemory, 0);

	void* data;
	vkMapMemory(m_device, stagingMemory, 0, sizeof(uint32_t), 0, &data);
	memcpy(data, &pixel, sizeof(uint32_t));
	vkUnmapMemory(m_device, stagingMemory);

	VkImageCreateInfo imageInfo{};
	imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	imageInfo.imageType = VK_IMAGE_TYPE_2D;
	imageInfo.extent = { 1, 1, 1 };
	imageInfo.mipLevels = 1;
	imageInfo.arrayLayers = 1;
	imageInfo.format = VK_FORMAT_R8G8B8A8_UNORM;
	imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
	imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
	imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_SAMPLED_BIT;
	imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
	imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

	VkImage image;
	vkCreateImage(m_device, &imageInfo, nullptr, &image);

	vkGetImageMemoryRequirements(m_device, image, &memReq);

	allocInfo.allocationSize = memReq.size;
	allocInfo.memoryTypeIndex =
		GetMemoryType(
			memReq.memoryTypeBits,
			VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);

	VkDeviceMemory imageMemory;
	vkAllocateMemory(m_device, &allocInfo, nullptr, &imageMemory);
	vkBindImageMemory(m_device, image, imageMemory, 0);

	VkCommandBufferAllocateInfo cmdAlloc{};
	cmdAlloc.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	cmdAlloc.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	cmdAlloc.commandPool = m_cmdPool;
	cmdAlloc.commandBufferCount = 1;

	VkCommandBuffer cmd;
	vkAllocateCommandBuffers(m_device, &cmdAlloc, &cmd);

	VkCommandBufferBeginInfo beginInfo{};
	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

	vkBeginCommandBuffer(cmd, &beginInfo);

	auto transition = [&](VkImageLayout oldLayout, VkImageLayout newLayout)
		{
			VkImageMemoryBarrier barrier{};
			barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
			barrier.oldLayout = oldLayout;
			barrier.newLayout = newLayout;
			barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
			barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
			barrier.image = image;
			barrier.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
			barrier.subresourceRange.levelCount = 1;
			barrier.subresourceRange.layerCount = 1;

			VkPipelineStageFlags srcStage;
			VkPipelineStageFlags dstStage;

			if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED &&
				newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
			{
				barrier.srcAccessMask = 0;
				barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
				srcStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
				dstStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
			}
			else
			{
				barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
				barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
				srcStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
				dstStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
			}

			vkCmdPipelineBarrier(cmd, srcStage, dstStage, 0, 0, nullptr, 0, nullptr, 1, &barrier);
		};

	transition(VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

	VkBufferImageCopy region{};
	region.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	region.imageSubresource.layerCount = 1;
	region.imageExtent = { 1, 1, 1 };

	vkCmdCopyBufferToImage(cmd, stagingBuffer, image,
		VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
		1, &region);

	transition(VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
		VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);

	vkEndCommandBuffer(cmd);

	VkSubmitInfo submit{};
	submit.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submit.commandBufferCount = 1;
	submit.pCommandBuffers = &cmd;

	vkQueueSubmit(m_queue, 1, &submit, VK_NULL_HANDLE);
	vkQueueWaitIdle(m_queue);

	vkFreeCommandBuffers(m_device, m_cmdPool, 1, &cmd);

	VkImageViewCreateInfo viewInfo{};
	viewInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
	viewInfo.image = image;
	viewInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
	viewInfo.format = VK_FORMAT_R8G8B8A8_UNORM;
	viewInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	viewInfo.subresourceRange.levelCount = 1;
	viewInfo.subresourceRange.layerCount = 1;

	VkImageView view;
	vkCreateImageView(m_device, &viewInfo, nullptr, &view);

	VkSamplerCreateInfo samplerInfo{};
	samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
	samplerInfo.magFilter = VK_FILTER_LINEAR;
	samplerInfo.minFilter = VK_FILTER_LINEAR;
	samplerInfo.addressModeU = VK_SAMPLER_ADDRESS_MODE_REPEAT;
	samplerInfo.addressModeV = VK_SAMPLER_ADDRESS_MODE_REPEAT;
	samplerInfo.addressModeW = VK_SAMPLER_ADDRESS_MODE_REPEAT;

	VkSampler sampler;
	vkCreateSampler(m_device, &samplerInfo, nullptr, &sampler);

	m_colorMap.image = image;
	m_colorMap.view = view;
	m_colorMap.sampler = sampler;

	vkDestroyBuffer(m_device, stagingBuffer, nullptr);
	vkFreeMemory(m_device, stagingMemory, nullptr);
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

void VkEngine::VulkanEngine::SetupVertexDescriptions()
{
	m_vertices.bindingDescriptions.resize(1);

	VkVertexInputBindingDescription inputBindDescription = {};
	inputBindDescription.binding = 0;
	inputBindDescription.stride = sizeof(Vertex);
	inputBindDescription.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

	m_vertices.bindingDescriptions[0] = inputBindDescription;

	m_vertices.attributeDescriptions.resize(4);

	m_vertices.attributeDescriptions[0] =
		GetVertexInputAttributeDescription(
			0,
			0,
			VK_FORMAT_R32G32B32_SFLOAT,
			0);

	m_vertices.attributeDescriptions[1] =
		GetVertexInputAttributeDescription(
			0,
			1,
			VK_FORMAT_R32G32B32_SFLOAT,
			sizeof(float) * 3);

	m_vertices.attributeDescriptions[2] =
		GetVertexInputAttributeDescription(
			0,
			2,
			VK_FORMAT_R32G32_SFLOAT,
			sizeof(float) * 6);

	m_vertices.attributeDescriptions[3] =
		GetVertexInputAttributeDescription(
			0,
			3,
			VK_FORMAT_R32G32B32_SFLOAT,
			sizeof(float) * 8);

	VkPipelineVertexInputStateCreateInfo pipelineVertexInputStateCreateInfo = {};
	pipelineVertexInputStateCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
	pipelineVertexInputStateCreateInfo.pNext = NULL;

	m_vertices.inputState = pipelineVertexInputStateCreateInfo;
	m_vertices.inputState.vertexBindingDescriptionCount = m_vertices.bindingDescriptions.size();
	m_vertices.inputState.pVertexBindingDescriptions = m_vertices.bindingDescriptions.data();
	m_vertices.inputState.vertexAttributeDescriptionCount = m_vertices.attributeDescriptions.size();
	m_vertices.inputState.pVertexAttributeDescriptions = m_vertices.attributeDescriptions.data();
}

VkResult VkEngine::VulkanEngine::PrepareUniformBuffers()
{
	VKE_CHECK_RESULT(CreateBuffer(
		VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT,
		VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT,
		sizeof(m_uboVS),
		&m_uboVS,
		&m_uniformData.buffer,
		&m_uniformData.memory,
		&m_uniformData.descriptor
	));

	return UpdateUniformBuffers();
}

VkResult VkEngine::VulkanEngine::SetupDescriptorSetLayout()
{
	VkDescriptorSetLayoutBinding vertexShaderUniformBuffer = {};
	vertexShaderUniformBuffer.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	vertexShaderUniformBuffer.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;
	vertexShaderUniformBuffer.binding = 0;
	vertexShaderUniformBuffer.descriptorCount = 1;

	VkDescriptorSetLayoutBinding fragmentShaderCombinedSampler = {};
	fragmentShaderCombinedSampler.descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
	fragmentShaderCombinedSampler.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;
	fragmentShaderCombinedSampler.binding = 1;
	fragmentShaderCombinedSampler.descriptorCount = 1;

	std::vector<VkDescriptorSetLayoutBinding> setLayoutBindings =
	{
		vertexShaderUniformBuffer,
		fragmentShaderCombinedSampler
	};

	VkDescriptorSetLayoutCreateInfo descriptorLayout{};
	descriptorLayout.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
	descriptorLayout.pNext = NULL;
	descriptorLayout.pBindings = setLayoutBindings.data();
	descriptorLayout.bindingCount = setLayoutBindings.size();

	VKE_CHECK_RESULT(vkCreateDescriptorSetLayout(m_device, &descriptorLayout, nullptr, &m_descriptorSetLayout));

	VkPipelineLayoutCreateInfo pipelineLayoutCreateInfo{};
	pipelineLayoutCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	pipelineLayoutCreateInfo.pNext = NULL;
	pipelineLayoutCreateInfo.setLayoutCount = 1;
	pipelineLayoutCreateInfo.pSetLayouts = &m_descriptorSetLayout;

	return vkCreatePipelineLayout(m_device, &pipelineLayoutCreateInfo, nullptr, &m_pipelineLayout);
}

VkResult VkEngine::VulkanEngine::PreparePipelines()
{
	VkPipelineInputAssemblyStateCreateInfo inputAssemblyState{};
	inputAssemblyState.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
	inputAssemblyState.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
	inputAssemblyState.flags = 0;
	inputAssemblyState.primitiveRestartEnable = VK_FALSE;

	VkPipelineRasterizationStateCreateInfo rasterizationState{};
	rasterizationState.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
	rasterizationState.polygonMode = VK_POLYGON_MODE_FILL;
	rasterizationState.cullMode = VK_CULL_MODE_BACK_BIT;
	rasterizationState.frontFace = VK_FRONT_FACE_CLOCKWISE;
	rasterizationState.flags = 0;
	rasterizationState.depthClampEnable = VK_TRUE;
	rasterizationState.lineWidth = 1.0f;

	VkPipelineColorBlendAttachmentState blendAttachmentState{};
	blendAttachmentState.colorWriteMask = 0xf;
	blendAttachmentState.blendEnable = VK_FALSE;

	VkPipelineColorBlendStateCreateInfo colorBlendState{};
	colorBlendState.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	colorBlendState.pNext = NULL;
	colorBlendState.attachmentCount = 1;
	colorBlendState.pAttachments = &blendAttachmentState;

	VkPipelineDepthStencilStateCreateInfo depthStencilState{};
	depthStencilState.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
	depthStencilState.depthTestEnable = VK_TRUE;
	depthStencilState.depthWriteEnable = VK_TRUE;
	depthStencilState.depthCompareOp = VK_COMPARE_OP_LESS_OR_EQUAL;
	depthStencilState.front = depthStencilState.back;
	depthStencilState.back.compareOp = VK_COMPARE_OP_ALWAYS;

	VkPipelineViewportStateCreateInfo viewportState{};
	viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
	viewportState.viewportCount = 1;
	viewportState.scissorCount = 1;
	viewportState.flags = 0;

	VkPipelineMultisampleStateCreateInfo multisampleState{};
	multisampleState.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
	multisampleState.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

	std::vector<VkDynamicState> dynamicStateEnables = {
		VK_DYNAMIC_STATE_VIEWPORT,
		VK_DYNAMIC_STATE_SCISSOR
	};
	VkPipelineDynamicStateCreateInfo dynamicState{};
	dynamicState.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
	dynamicState.pDynamicStates = dynamicStateEnables.data();
	dynamicState.dynamicStateCount = dynamicStateEnables.size();

	std::array<VkPipelineShaderStageCreateInfo, 2> shaderStages{};

	VKE_CHECK_RESULT(LoadShader("mesh.vert.spv", VK_SHADER_STAGE_VERTEX_BIT, shaderStages[0]));
	VKE_CHECK_RESULT(LoadShader("mesh.frag.spv", VK_SHADER_STAGE_FRAGMENT_BIT, shaderStages[1]));

	VkGraphicsPipelineCreateInfo pipelineCreateInfo{};
	pipelineCreateInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineCreateInfo.pNext = NULL;
	pipelineCreateInfo.layout = m_pipelineLayout;
	pipelineCreateInfo.renderPass = m_renderPass;
	pipelineCreateInfo.flags = 0;
	pipelineCreateInfo.pVertexInputState = &m_vertices.inputState;
	pipelineCreateInfo.pInputAssemblyState = &inputAssemblyState;
	pipelineCreateInfo.pRasterizationState = &rasterizationState;
	pipelineCreateInfo.pColorBlendState = &colorBlendState;
	pipelineCreateInfo.pMultisampleState = &multisampleState;
	pipelineCreateInfo.pViewportState = &viewportState;
	pipelineCreateInfo.pDepthStencilState = &depthStencilState;
	pipelineCreateInfo.pDynamicState = &dynamicState;
	pipelineCreateInfo.stageCount = shaderStages.size();
	pipelineCreateInfo.pStages = shaderStages.data();

	return vkCreateGraphicsPipelines(m_device, m_pipelineCache, 1, &pipelineCreateInfo, nullptr, &m_pipeline);
}

VkResult VkEngine::VulkanEngine::SetupDescriptorPool()
{
	VkDescriptorPoolSize descriptorPoolSizeUBO{};
	descriptorPoolSizeUBO.type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	descriptorPoolSizeUBO.descriptorCount = 1;

	VkDescriptorPoolSize descriptorPoolSizeSampler{};
	descriptorPoolSizeSampler.type = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
	descriptorPoolSizeSampler.descriptorCount = 1;

	std::vector<VkDescriptorPoolSize> poolSizes =
	{
		descriptorPoolSizeUBO,
		descriptorPoolSizeSampler
	};

	VkDescriptorPoolCreateInfo descriptorPoolInfo{};
	descriptorPoolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
	descriptorPoolInfo.pNext = NULL;
	descriptorPoolInfo.poolSizeCount = poolSizes.size();
	descriptorPoolInfo.pPoolSizes = poolSizes.data();
	descriptorPoolInfo.maxSets = 1;

	return vkCreateDescriptorPool(m_device, &descriptorPoolInfo, nullptr, &m_descriptorPool);
}

VkResult VkEngine::VulkanEngine::SetupDescriptorSet()
{
	VkDescriptorSetAllocateInfo allocInfo{};
	allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
	allocInfo.pNext = NULL;
	allocInfo.descriptorPool = m_descriptorPool;
	allocInfo.pSetLayouts = &m_descriptorSetLayout;
	allocInfo.descriptorSetCount = 1;

	VKE_CHECK_RESULT(vkAllocateDescriptorSets(m_device, &allocInfo, &m_descriptorSet));

	VkDescriptorImageInfo texDescriptor{};
	texDescriptor.sampler = m_colorMap.sampler;
	texDescriptor.imageView = m_colorMap.view;
	texDescriptor.imageLayout = VK_IMAGE_LAYOUT_GENERAL;

	VkWriteDescriptorSet writeDescriptorSetUniform{};
	writeDescriptorSetUniform.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
	writeDescriptorSetUniform.pNext = NULL;
	writeDescriptorSetUniform.dstSet = m_descriptorSet;
	writeDescriptorSetUniform.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	writeDescriptorSetUniform.dstBinding = 0;
	writeDescriptorSetUniform.pBufferInfo = &m_uniformData.descriptor;
	writeDescriptorSetUniform.descriptorCount = 1;

	VkWriteDescriptorSet writeDescriptorSetColorMap = {};
	writeDescriptorSetColorMap.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
	writeDescriptorSetColorMap.pNext = NULL;
	writeDescriptorSetColorMap.dstSet = m_descriptorSet;
	writeDescriptorSetColorMap.descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
	writeDescriptorSetColorMap.dstBinding = 1;
	writeDescriptorSetColorMap.pImageInfo = &texDescriptor;
	writeDescriptorSetColorMap.descriptorCount = 1;


	std::vector<VkWriteDescriptorSet> writeDescriptorSets =
	{
		writeDescriptorSetUniform,
		writeDescriptorSetColorMap
	};

	vkUpdateDescriptorSets(m_device, writeDescriptorSets.size(), writeDescriptorSets.data(), 0, NULL);
	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::BuildCommandBuffers()
{
	VkCommandBufferBeginInfo cmdBufInfo{};
	cmdBufInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	cmdBufInfo.pNext = NULL;

	VkClearValue clearValues[2];
	clearValues[0].color = m_defaultClearColor;
	clearValues[1].depthStencil = { 1.0f, 0 };

	VkRenderPassBeginInfo renderPassBeginInfo{};
	renderPassBeginInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
	renderPassBeginInfo.pNext = NULL;
	renderPassBeginInfo.renderPass = m_renderPass;
	renderPassBeginInfo.renderArea.offset.x = 0;
	renderPassBeginInfo.renderArea.offset.y = 0;
	renderPassBeginInfo.renderArea.extent.width = m_width;
	renderPassBeginInfo.renderArea.extent.height = m_height;
	renderPassBeginInfo.clearValueCount = 2;
	renderPassBeginInfo.pClearValues = clearValues;

	for (int32_t i = 0; i < m_drawCmdBuffers.size(); ++i)
	{
		renderPassBeginInfo.framebuffer = m_frameBuffers[i];

		VKE_CHECK_RESULT(vkBeginCommandBuffer(m_drawCmdBuffers[i], &cmdBufInfo));

		vkCmdBeginRenderPass(m_drawCmdBuffers[i], &renderPassBeginInfo, VK_SUBPASS_CONTENTS_INLINE);

		VkViewport viewport{};
		viewport.width = (float)m_width;
		viewport.height = (float)m_height;
		viewport.minDepth = 0.0f;
		viewport.maxDepth = 1.0f;
		vkCmdSetViewport(m_drawCmdBuffers[i], 0, 1, &viewport);

		VkRect2D scissor{};
		scissor.extent.width = m_width;
		scissor.extent.height = m_height;
		scissor.offset.x = 0;
		scissor.offset.y = 0;
		vkCmdSetScissor(m_drawCmdBuffers[i], 0, 1, &scissor);

		vkCmdBindDescriptorSets(m_drawCmdBuffers[i], VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipelineLayout, 0, 1, &m_descriptorSet, 0, NULL);
		vkCmdBindPipeline(m_drawCmdBuffers[i], VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipeline);

		VkDeviceSize offsets[1] = { 0 };
		vkCmdBindVertexBuffers(m_drawCmdBuffers[i], 0, 1, &m_mesh.vertices.buf, offsets);
		vkCmdBindIndexBuffer(m_drawCmdBuffers[i], m_mesh.indices.buf, 0, VK_INDEX_TYPE_UINT32);
		vkCmdDrawIndexed(m_drawCmdBuffers[i], m_mesh.indices.count, 1, 0, 0, 0);

		vkCmdEndRenderPass(m_drawCmdBuffers[i]);

		VKE_CHECK_RESULT(vkEndCommandBuffer(m_drawCmdBuffers[i]));
	}
	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::SubmitPostPresentBarrier(VkImage image)
{
	VkCommandBufferBeginInfo cmdBufInfo{};
	cmdBufInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	cmdBufInfo.pNext = NULL;

	VKE_CHECK_RESULT(vkBeginCommandBuffer(m_postPresentCmdBuffer, &cmdBufInfo));

	VkImageMemoryBarrier postPresentBarrier{};
	postPresentBarrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
	postPresentBarrier.pNext = NULL;
	postPresentBarrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	postPresentBarrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	postPresentBarrier.srcAccessMask = 0;
	postPresentBarrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
	postPresentBarrier.oldLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
	postPresentBarrier.newLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
	postPresentBarrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	postPresentBarrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	postPresentBarrier.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
	postPresentBarrier.image = image;

	vkCmdPipelineBarrier(
		m_postPresentCmdBuffer,
		VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
		VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
		0,
		0, nullptr,
		0, nullptr,
		1, &postPresentBarrier);

	VKE_CHECK_RESULT(vkEndCommandBuffer(m_postPresentCmdBuffer));

	VkSubmitInfo submitInfo{};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.pNext = NULL;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &m_postPresentCmdBuffer;

	return vkQueueSubmit(m_queue, 1, &submitInfo, VK_NULL_HANDLE);
}

VkResult VkEngine::VulkanEngine::SubmitPrePresentBarrier(VkImage image)
{
	VkCommandBufferBeginInfo cmdBufInfo{};
	cmdBufInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	cmdBufInfo.pNext = NULL;

	VKE_CHECK_RESULT(vkBeginCommandBuffer(m_prePresentCmdBuffer, &cmdBufInfo));

	VkImageMemoryBarrier prePresentBarrier{};
	prePresentBarrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
	prePresentBarrier.pNext = NULL;
	prePresentBarrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	prePresentBarrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	prePresentBarrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
	prePresentBarrier.dstAccessMask = 0;
	prePresentBarrier.oldLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
	prePresentBarrier.newLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
	prePresentBarrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	prePresentBarrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	prePresentBarrier.subresourceRange = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1 };
	prePresentBarrier.image = image;

	vkCmdPipelineBarrier(
		m_prePresentCmdBuffer,
		VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
		VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
		0,
		0, nullptr,
		0, nullptr,
		1, &prePresentBarrier);

	VKE_CHECK_RESULT(vkEndCommandBuffer(m_prePresentCmdBuffer));

	VkSubmitInfo submitInfo{};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.pNext = NULL;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &m_prePresentCmdBuffer;

	return vkQueueSubmit(m_queue, 1, &submitInfo, VK_NULL_HANDLE);
}

VkResult VkEngine::VulkanEngine::UpdateUniformBuffers()
{
	m_uboVS.projection = glm::perspective(glm::radians(60.0f), (float)m_width / (float)m_height, 0.1f, 256.0f);
	glm::mat4 viewMatrix = glm::translate(glm::mat4(), glm::vec3(0.0f, 0.0f, m_zoom));

	m_uboVS.model = viewMatrix * glm::translate(glm::mat4(), m_cameraPos);
	m_uboVS.model = glm::rotate(m_uboVS.model, glm::radians(m_rotation.x), glm::vec3(1.0f, 0.0f, 0.0f));
	m_uboVS.model = glm::rotate(m_uboVS.model, glm::radians(m_rotation.y), glm::vec3(0.0f, 1.0f, 0.0f));
	m_uboVS.model = glm::rotate(m_uboVS.model, glm::radians(m_rotation.z), glm::vec3(0.0f, 0.0f, 1.0f));

	uint8_t* pData;
	VKE_CHECK_RESULT(vkMapMemory(m_device, m_uniformData.memory, 0, sizeof(m_uboVS), 0, (void**)&pData));
	memcpy(pData, &m_uboVS, sizeof(m_uboVS));
	vkUnmapMemory(m_device, m_uniformData.memory);

	return VK_SUCCESS;
}

VkResult VkEngine::VulkanEngine::CreateBuffer(VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryPropertyFlags, VkDeviceSize size, void* data, VkBuffer* buffer, VkDeviceMemory* memory, VkDescriptorBufferInfo* descriptor)
{
	VkMemoryRequirements memReqs;
	VkMemoryAllocateInfo memAlloc{};
	memAlloc.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
	memAlloc.pNext = NULL;
	memAlloc.allocationSize = 0;
	memAlloc.memoryTypeIndex = 0;

	VkBufferCreateInfo bufferCreateInfo{};
	bufferCreateInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferCreateInfo.pNext = NULL;
	bufferCreateInfo.usage = usageFlags;
	bufferCreateInfo.size = size;
	bufferCreateInfo.flags = 0;

	VKE_CHECK_RESULT(vkCreateBuffer(m_device, &bufferCreateInfo, nullptr, buffer));

	vkGetBufferMemoryRequirements(m_device, *buffer, &memReqs);
	memAlloc.allocationSize = memReqs.size;
	memAlloc.memoryTypeIndex = GetMemoryType(memReqs.memoryTypeBits, memoryPropertyFlags);
	VKE_CHECK_RESULT(vkAllocateMemory(m_device, &memAlloc, nullptr, memory));
	if (data != nullptr)
	{
		void* mapped;
		VKE_CHECK_RESULT(vkMapMemory(m_device, *memory, 0, size, 0, &mapped));
		memcpy(mapped, data, size);
		vkUnmapMemory(m_device, *memory);
	}
	VKE_CHECK_RESULT(vkBindBufferMemory(m_device, *buffer, *memory, 0));

	descriptor->offset = 0;
	descriptor->buffer = *buffer;
	descriptor->range = size;
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
	m_submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	m_submitInfo.pNext = NULL;
	m_submitInfo.pWaitDstStageMask = &m_submitPipelineStages;
	m_submitInfo.waitSemaphoreCount = 1;
	m_submitInfo.pWaitSemaphores = &m_semaphores.presentComplete;
	m_submitInfo.signalSemaphoreCount = 1;
	m_submitInfo.pSignalSemaphores = &m_semaphores.renderComplete;
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

VkResult VkEngine::VulkanEngine::LoadShader(std::string fileName, VkShaderStageFlagBits stage, VkPipelineShaderStageCreateInfo& out)
{
	out.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
	out.stage = stage;
#if defined(__ANDROID__)
	VKE_CHECK_RESULT(ObjectLoader::LoadShader(androidApp->activity->assetManager, fileName.c_str(), device, stage, out.module));
#else
	VKE_CHECK_RESULT(ObjectLoader::LoadShader(fileName.c_str(), m_device, stage, out.module));
#endif
	out.pName = "main";
	assert(out.module != NULL);
	m_shaderModules.push_back(out.module);
	return VK_SUCCESS;
}
