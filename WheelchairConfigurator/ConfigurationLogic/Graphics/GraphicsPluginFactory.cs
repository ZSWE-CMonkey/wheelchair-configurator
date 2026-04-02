namespace ConfigurationLogic.Graphics
{
    public static class GraphicsPluginFactory
    {
        public static IGraphicsPlugin CreateImageCompositorGraphicsPlugin(int width, int height)
        {
            return new ImageCompositorGraphicsPluginImpl(width, height);
        }

        public static IGraphicsPlugin CreateVulkanGraphicsPlugin()
        {
            return new VulkanGraphicsPluginImpl();
        }

        public static IGraphicsPlugin CreateMoltenVulkanGraphicsPlugin()
        {
            throw new NotImplementedException();
        }

        //--If needed, add more graphics devices--//
    }
}
