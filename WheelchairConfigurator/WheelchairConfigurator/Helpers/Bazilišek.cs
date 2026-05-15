using ConfigurationLogic.Graphics;
using ConfigurationLogic.Graphics.Types;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace WheelchairConfigurator.Helpers
{
    internal class Bazilišek
    {
        private struct Camera
        {
            public float Zoom;
            public CameraPosition Position;
            public CameraRotation Rotation;

            public Camera(float zoom, CameraPosition position, CameraRotation rotation)
            {
                Zoom = zoom;
                Position = position;
                Rotation = rotation;
            }
        }

        private readonly int _width;
        private readonly int _height;
        private readonly IGraphicsPlugin _graphicsPlugin;
        private readonly object _mutex = new();

        private Camera _camera;
        private bool _isInitialized = false;

        private CancellationTokenSource? _renderCts;
        private Task? _renderTask;
        private Action<SKBitmap>? _onFrame;

        public Bazilišek(string name, int width, int height)
        {
            _width = width;
            _height = height;
            _graphicsPlugin = GraphicsPluginFactory.CreateVulkanGraphicsPlugin(name, width, height);

            _camera = new Camera(
                -5.5f,
                new CameraPosition(0.1f, 1.1f, 0.0f),
                new CameraRotation(-0.5f, -112.75f, 0.0f));
        }

        public void SetHighQualityTextures(bool enabled)
        {
            _graphicsPlugin.SetHighQualityTextures(enabled);
        }

        public async Task RebuildSceneAsync(IReadOnlyList<(string id, string geometryPath, string texturePath, float scale,
            float anchorX, float anchorY, float anchorZ,
            float rotationX, float rotationY, float rotationZ)> models)
        {
            Console.WriteLine($"[Bazilisek] Rebuild: {models.Count} models");
            await StopRenderLoopAsync();

            lock (_mutex)
            {
                if (_isInitialized)
                {
                    _graphicsPlugin.Deinitialize();
                    _isInitialized = false;
                }

                if (models.Count == 0)
                {
                    Console.WriteLine("[Bazilisek] Rebuild done (empty scene)");
                    return;
                }

                foreach (var (id, geom, tex, scale, ax, ay, az, rx, ry, rz) in models)
                    _graphicsPlugin.AddResourceFromFiles(id, geom, tex, scale, ax, ay, az, rx, ry, rz);

                _graphicsPlugin.SetCamera(_camera.Zoom, _camera.Position, _camera.Rotation);
                _graphicsPlugin.Initialize();
                _isInitialized = true;
            }

            Console.WriteLine("[Bazilisek] Rebuild done");

            if (_onFrame != null)
                StartRenderLoopInternal();
        }

        public void StartRenderLoop(Action<SKBitmap> onFrame)
        {
            _onFrame = onFrame;
            if (_isInitialized)
                StartRenderLoopInternal();
        }

        private void StartRenderLoopInternal()
        {
            if (_renderTask != null && !_renderTask.IsCompleted) return;

            _renderCts = new CancellationTokenSource();
            var ct = _renderCts.Token;
            var callback = _onFrame;

            _renderTask = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    SKBitmap? frame = null;
                    lock (_mutex)
                    {
                        if (_isInitialized)
                        {
                            try
                            {
                                _graphicsPlugin.Render(out byte[] pixels);
                                ConvertMagentaToTransparent(pixels);

                                var info = new SKImageInfo(_width, _height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                                var bitmap = new SKBitmap(info);
                                Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
                                frame = bitmap;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[Bazilisek.RenderLoop] EXCEPTION: " + ex.Message);
                                break;
                            }
                        }
                    }

                    if (frame != null)
                    {
                        try { callback?.Invoke(frame); }
                        catch (Exception ex) { Console.WriteLine("[Bazilisek.RenderLoop] callback exception: " + ex.Message); }
                    }

                    try { await Task.Delay(16, ct); }
                    catch (OperationCanceledException) { break; }
                }
            }, ct);
        }

        public async Task StopRenderLoopAsync()
        {
            var cts = _renderCts;
            var task = _renderTask;
            if (cts != null)
            {
                cts.Cancel();
                if (task != null)
                {
                    try { await task; }
                    catch { }
                }
                cts.Dispose();
            }
            _renderCts = null;
            _renderTask = null;
        }

        public async Task ShutdownAsync()
        {
            Console.WriteLine("[Bazilisek] Shutdown begin");
            await StopRenderLoopAsync();
            lock (_mutex)
            {
                if (_isInitialized)
                {
                    _graphicsPlugin.Deinitialize();
                    _isInitialized = false;
                }
            }
            Console.WriteLine("[Bazilisek] Shutdown end");
        }

        public void PomaluSanjski(float x, float y)
        {
            _camera.Rotation.X += x;
            _camera.Rotation.Y += y;

            lock (_mutex)
            {
                if (_isInitialized)
                    _graphicsPlugin.SetCamera(_camera.Zoom, _camera.Position, _camera.Rotation);
            }
        }

        private static void ConvertMagentaToTransparent(byte[] pixels)
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                if (r == 255 && g == 0 && b == 255)
                    pixels[i + 3] = 0;
            }
        }
    }
}
