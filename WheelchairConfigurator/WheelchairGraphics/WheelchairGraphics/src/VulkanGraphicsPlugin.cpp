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

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::Initialize(std::string appName, uint32_t width, uint32_t height)
{
    if (m_vulkanEngine->InitVulkan(appName, width, height) != VK_SUCCESS)
        return GP_INITIALIZATION_FAILED;

    if (m_vulkanEngine->Prepare() != VK_SUCCESS)
        return GP_INITIALIZATION_FAILED;

    return GP_SUCCESS;
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::AddObject(std::string objectId)
{
    if (m_vulkanEngine->AddObject(objectId) != VK_SUCCESS)
        return GP_ADD_OBJECT_ERROR;

    return GP_SUCCESS;
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::Render(const char** out)
{
    if (out == nullptr)
        return GP_NULL_PARAM_ERROR;

    if (!m_vulkanEngine)
        return GP_SUCCESS;

    if (*out)
        *out = nullptr;

    return m_vulkanEngine->Render(out) == VK_SUCCESS ? GP_SUCCESS : GP_RENDERING_ERROR;
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
