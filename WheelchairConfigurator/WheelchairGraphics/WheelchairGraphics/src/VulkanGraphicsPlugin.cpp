#include "VulkanGraphicsPlugin.h"

#include <stdexcept>

using namespace GraphicsPlugin;

GraphicsPlugin::VulkanGraphicsPlugin::VulkanGraphicsPlugin()
{
    m_vulkanEngine = std::make_unique<VkEngine::VulkanEngine>();
}

GraphicsPlugin::VulkanGraphicsPlugin::~VulkanGraphicsPlugin()
{
    CleanUp();
}

#if _WIN32
void GraphicsPlugin::VulkanGraphicsPlugin::SetHandles(void* platformHandle, void* platformWindow)
{
    m_platformHandle = platformHandle;
    m_platformWindow = platformWindow;
}
#elif __ANDROID__
void GraphicsPlugin::VulkanGraphicsPlugin::SetHandles(ANativeWindow* window)
{
    throw std::runtime_error("Not implemented");
}
#else
void GraphicsPlugin::VulkanGraphicsPlugin::SetHandles()
{
    throw std::runtime_error("Not implemented");
}
#endif

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::Initialize(std::string appName)
{
    if (m_vulkanEngine->InitVulkan(appName) != VK_SUCCESS)
        return GP_INITIALIZATION_FAILED;

    //TODO: PLATFORM SPECIFIC
    if (m_vulkanEngine->InitSwapchain(m_platformHandle, m_platformWindow) != VK_SUCCESS)
        return GP_INITIALIZATION_FAILED;

    if (m_vulkanEngine->Prepare() != VK_SUCCESS)
        return GP_INITIALIZATION_FAILED;

    return GP_SUCCESS;
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::SetObject()
{
    return GP_SUCCESS;
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::Render()
{
    return GP_SUCCESS;
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::DeInitialize()
{
    CleanUp();
    return GP_SUCCESS;
}

void GraphicsPlugin::VulkanGraphicsPlugin::CleanUp()
{
    m_vulkanEngine = nullptr;
}
