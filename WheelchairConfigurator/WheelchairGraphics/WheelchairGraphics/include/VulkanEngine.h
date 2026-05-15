#pragma once

#include <string>
#include <memory>
#include <unordered_map>

#include "VulkanCommon.h"
#include "VulkanSwapchain.h"
#include "ObjectLoader.h"

#define GLM_FORCE_RADIANS
#define GLM_FORCE_DEPTH_ZERO_TO_ONE
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

namespace VkEngine {

	struct Vertex {
		glm::vec3 pos;
		glm::vec3 normal;
		glm::vec2 uv;
		glm::vec3 color;
	};
	struct UniformData
	{
		VkBuffer buffer;
		VkDeviceMemory memory;
		VkDescriptorBufferInfo descriptor;
		uint32_t allocSize;
		void* mapped = nullptr;
	};

	struct CreateBufferInfo {
		VkBufferUsageFlags usage;
		VkMemoryPropertyFlags memoryPropertyFlags;
		VkDeviceSize size;
		void* data;
		VkBuffer* buffer;
		VkDeviceMemory* memory;
		VkDescriptorBufferInfo* descriptor;
	};

	struct Mesh {
		struct {
			VkBuffer buf;
			VkDeviceMemory mem;
		} vertices;
		struct {
			int count;
			VkBuffer buf;
			VkDeviceMemory mem;
		} indices;
	};

	class VulkanEngine
	{
	public:
		VulkanEngine();
		~VulkanEngine();

		VkResult SetCamera(float zoom, glm::vec3 position, glm::vec3 rotation);

		VkResult AddObject(std::string objectId);
		VkResult AddObjectFromFiles(std::string objectId, std::string geometryPath, std::string texturePath,
			float scale,
			float anchorX, float anchorY, float anchorZ,
			float rotationX, float rotationY, float rotationZ);

		VkResult InitVulkan(std::string appName, uint32_t width, uint32_t height);

		VkResult Prepare();
		
		VkResult Render(const char** imagedata);

	private:

		VkResult CreateInstance(std::string appName);
		VkResult CheckPhysicalDevices(uint32_t& outGraphicsQueueIndex);
		VkResult CreateDevice(uint32_t graphicsQueueIndex);
		VkResult CreateVulkanSemaphore();
		
		VkResult CreateCommandPool();
		VkResult CreateSetupCommandBuffer();
		VkResult CreateCommandBuffers();
		VkResult SetupDepthStencil();
		VkResult SetupRenderPass();
		VkResult CreatePipelineCache();
		VkResult SetupFrameBuffer();
		VkResult FlushSetupCommandBuffer();

		void SetupVertexDescriptions();
		VkResult PrepareUniformBuffers();
		VkResult SetupDescriptorSetLayout();
		VkResult PreparePipelines();
		VkResult SetupDescriptorPool();
		VkResult SetupDescriptorSet(VulkanTexture& vulkanTexture, VkDescriptorSet& descriptorSet);
		VkResult BuildCommandBuffers();

		VkResult SubmitPostPresentBarrier(VkImage image);
		VkResult SubmitPrePresentBarrier(VkImage image);

		VkResult UpdateUniformBuffers();
		//Todo: shorter parameters >:(
		VkResult CreateBuffer(VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryPropertyFlags, VkDeviceSize size, void* data, VkBuffer* buffer, VkDeviceMemory* memory, VkDescriptorBufferInfo* descriptor);
		VkResult CreateBuffer(VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryPropertyFlags, VkDeviceSize size, void* data, VkBuffer* buffer, VkDeviceMemory* memory);
		uint32_t GetMemoryType(uint32_t typeBits, VkFlags properties);

		VkResult CreateCommandBuffer(VkCommandBufferLevel level, bool begin, VkCommandBuffer& out);

		void CreateSumbitInfo();
		bool GetDepthFormat();

		VkResult FlushCommandBuffer(VkCommandBuffer commandBuffer, VkQueue queue, bool free);

		VkResult LoadResources();

		VkResult CopySwapchainImageToCPU(VkImage image, const char** imagedata);

		uint32_t GetMemoryTypeIndex(uint32_t typeBits, VkMemoryPropertyFlags properties);

		VkResult CreateOffscreenImage();
		VkResult CreateOffscreenFrameBuffer();

		VkResult LoadMesh(std::string id);
		VkResult LoadMeshFromFile(const std::string& path, float scale,
			float anchorX, float anchorY, float anchorZ,
			float rotationX, float rotationY, float rotationZ);
		VkResult LoadTexture(std::string id);
		VkResult LoadTextureFromFile(const std::string& path);
		VkResult LoadShader(std::string fileName, VkShaderStageFlagBits stage, VkPipelineShaderStageCreateInfo& out);

		std::unique_ptr<VulkanSwapchain> m_vulkanSwapchain = nullptr;
		std::unique_ptr<VkLoader::TextureHandle> m_textureHandle = nullptr;

		//TODO: seperate class
		float m_zoom = -5.5f;
		glm::vec3 m_rotation = { -0.5f, -112.75f, 0.0f };
		glm::vec3 m_cameraPos = { 0.1f, 1.1f, 0.0f };

		bool m_canRender = false;

		uint32_t m_width;
		uint32_t m_height;

		VkInstance m_instance;
		VkPhysicalDevice m_physicalDevice;
		VkDevice m_device;
		VkPhysicalDeviceMemoryProperties m_deviceMemoryProperties;
		VkQueue m_queue;

		VkFormat m_depthFormat;
		VkClearColorValue m_defaultClearColor = { { 1.0f, 0.0f, 1.0f, 1.0f } };

		VkCommandPool m_cmdPool;
		VkCommandBuffer m_setupCmdBuffer;
		std::vector<VkCommandBuffer> m_drawCmdBuffers;
		VkCommandBuffer m_postPresentCmdBuffer = VK_NULL_HANDLE;
		VkCommandBuffer m_prePresentCmdBuffer = VK_NULL_HANDLE;
		VkRenderPass m_renderPass;
		VkPipelineCache m_pipelineCache;
		std::vector<VkFramebuffer> m_frameBuffers;
		VkPipeline m_pipeline;
		std::vector<VkShaderModule> m_shaderModules;

		uint32_t m_currentBuffer = 0;

		VkSubmitInfo m_submitInfo;

		VkDescriptorSetLayout m_descriptorSetLayout;
		VkPipelineLayout m_pipelineLayout;
		VkDescriptorPool m_descriptorPool = VK_NULL_HANDLE;

		std::vector<VkDescriptorSet> m_descriptorSets{};
		std::vector<VulkanTexture> m_colorMaps{};

		VkPipelineStageFlags m_submitPipelineStages = VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;

		UniformData m_uniformData{};

		VkImage m_offscreenImage;
		VkMemoryRequirements m_memReq;
		VkDeviceMemory m_offscreenImageMemory;
		VkFramebuffer m_offscreenFramebuffer;
		std::vector<uint8_t> m_cpuBuffer;

		std::vector<Mesh> m_meshes{};

		std::vector<std::string> m_objectId{};

		struct ObjectFilePaths {
			std::string geometryPath;
			std::string texturePath;
			float scale;
			float anchorX, anchorY, anchorZ;
			float rotationX, rotationY, rotationZ;
		};
		std::unordered_map<std::string, ObjectFilePaths> m_objectFilePaths{};

		std::vector<uint8_t> m_lastEmbeddedTextureBytes{};

		struct {
			VkSemaphore presentComplete;
			VkSemaphore renderComplete;
		} m_semaphores;

		struct {
			VkImage image;
			VkDeviceMemory mem;
			VkImageView view;
		} m_depthStencil;

		struct {
			VkPipelineVertexInputStateCreateInfo inputState;
			std::vector<VkVertexInputBindingDescription> bindingDescriptions;
			std::vector<VkVertexInputAttributeDescription> attributeDescriptions;
		} m_vertices;

		struct {
			glm::mat4 projection;
			glm::mat4 model;
			glm::vec4 lightPos = glm::vec4(25.0f, 5.0f, 5.0f, 1.0f);
		} m_uboVS;

	};

}