#include "VulkanGraphicsPlugin.h"

using namespace GraphicsPlugin;

GraphicsPlugin::VulkanGraphicsPlugin::VulkanGraphicsPlugin()
{
    m_vulkanEngine = std::make_unique<VulkanEngine>();
}

GraphicsPlugin::VulkanGraphicsPlugin::~VulkanGraphicsPlugin()
{
    CleanUp();
}

GPluginResult GraphicsPlugin::VulkanGraphicsPlugin::Initialize()
{
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
    //vkDeviceWaitIdle(device); //must be first!

    return GP_SUCCESS;
}

void GraphicsPlugin::VulkanGraphicsPlugin::CleanUp()
{
    m_vulkanEngine = nullptr;
}
