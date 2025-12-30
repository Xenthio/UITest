#if WINDOWS
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;
using Sandbox.UI;
using Sandbox.UI.Skia;

namespace Avalazor.UI;

public unsafe class D3D11Backend : IGraphicsBackend
{
    private D3D11? _d3d11;
    private DXGI? _dxgi;
    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _deviceContext;
    private ComPtr<IDXGISwapChain> _swapChain;
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;

    private GRContext? _grContext;
    private SKSurface? _surface;
    private SkiaPanelRenderer? _renderer;
    private IWindow? _window;
    private Vector2D<int> _currentSize;

    public void Initialize(IWindow window)
    {
        _window = window;
        _d3d11 = D3D11.GetApi(null);
        _dxgi = DXGI.GetApi(null);

        var native = window.Native.Win32;
        if (!native.HasValue)
            throw new PlatformNotSupportedException("D3D11 backend requires Windows.");

        var hwnd = native.Value.Hwnd;
        _currentSize = window.FramebufferSize;

        var swapChainDesc = new SwapChainDesc
        {
            BufferCount = 1, // Single buffer to avoid double-buffer flicker issues
            BufferDesc = new ModeDesc
            {
                Width = (uint)window.FramebufferSize.X,
                Height = (uint)window.FramebufferSize.Y,
                Format = Format.FormatR8G8B8A8Unorm,
                RefreshRate = new Rational(60, 1),
                Scaling = ModeScaling.Unspecified,
                ScanlineOrdering = ModeScanlineOrder.Unspecified
            },
            BufferUsage = DXGI.UsageRenderTargetOutput,
            OutputWindow = hwnd,
            SampleDesc = new SampleDesc(1, 0),
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            Flags = (uint)SwapChainFlag.AllowModeSwitch
        };

        D3DFeatureLevel featureLevel = D3DFeatureLevel.Level110;

        _d3d11.CreateDeviceAndSwapChain(
            null,
            D3DDriverType.Hardware,
            0,
            (uint)CreateDeviceFlag.BgraSupport,
            &featureLevel,
            1,
            D3D11.SdkVersion,
            &swapChainDesc,
            _swapChain.GetAddressOf(),
            _device.GetAddressOf(),
            null,
            _deviceContext.GetAddressOf()
        );

        _renderer = new SkiaPanelRenderer();

        CreateRenderTarget();
    }

    public void Resize(Vector2D<int> size)
    {
        if (_device.Handle == null) return;

        _currentSize = size;

        // Release the old render target view before resizing
        _renderTargetView.Dispose();
        _renderTargetView = default;
        _surface?.Dispose();
        _surface = null;

        _swapChain.ResizeBuffers(0, (uint)size.X, (uint)size.Y, Format.FormatUnknown, 0);

        CreateRenderTarget();
    }

    public void Render(RootPanel panel)
    {
        if (_renderTargetView.Handle == null) return;

        // Set the render target
        var rtv = _renderTargetView.Handle;
        _deviceContext.Handle->OMSetRenderTargets(1, &rtv, (ID3D11DepthStencilView*)null);

        // Set viewport
        var viewport = new Viewport(0, 0, _currentSize.X, _currentSize.Y, 0, 1);
        _deviceContext.Handle->RSSetViewports(1, &viewport);

        // Clear the render target
        var color = stackalloc float[4] { 0.94f, 0.94f, 0.94f, 1.0f };
        _deviceContext.Handle->ClearRenderTargetView(_renderTargetView.Handle, color);

        // Present with sync interval 0 for immediate presentation
        _swapChain.Present(0, 0);
    }

    private void CreateRenderTarget()
    {
        ComPtr<ID3D11Texture2D> backBuffer = default;
        _swapChain.GetBuffer(0, SilkMarshal.GuidPtrOf<ID3D11Texture2D>(), (void**)&backBuffer.Handle);

        _device.Handle->CreateRenderTargetView((ID3D11Resource*)backBuffer.Handle, null, _renderTargetView.GetAddressOf());

        backBuffer.Dispose();
    }

    public void Dispose()
    {
        _renderTargetView.Dispose();
        _surface?.Dispose();
        _grContext?.Dispose();
        _swapChain.Dispose();
        _deviceContext.Dispose();
        _device.Dispose();
        _d3d11?.Dispose();
        _dxgi?.Dispose();
    }
}
#endif