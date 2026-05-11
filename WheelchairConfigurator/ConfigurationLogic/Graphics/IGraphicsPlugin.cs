using ConfigurationLogic.Graphics.Types;

namespace ConfigurationLogic.Graphics
{
    public interface IGraphicsPlugin
    {
        public bool Initialize();
        public void AddResource(string resourceId);
        public void AddResourceFromFiles(string objectId, string daePath, string ktxPath);
        public void SetCamera(float zoom, CameraPosition position, CameraRotation rotation);
        public void ClearResources();
        public void Render(out byte[] image);
        public void Deinitialize();
    }
}
