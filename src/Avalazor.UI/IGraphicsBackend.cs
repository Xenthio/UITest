using Silk.NET.Windowing;
using Silk.NET.Maths;
using Sandbox.UI;

namespace Avalazor.UI;

public interface IGraphicsBackend : IDisposable
{
    void Initialize(IWindow window);
    void Resize(Vector2D<int> size);
    void Render(RootPanel panel);
}