namespace ConfigurationLogic.Graphics
{
    public interface IGraphicsPlugin
    {
        public bool Initialize();
        public void AddResource(string resourceId);
        public void ClearResources();
        public void Render(out byte[] image);
        public void Deinitialize();
    }
}
