using System;
using SkiaSharp;

namespace Avalazor.UI.Controls
{
    /// <summary>
    /// Interactive button control with click events
    /// Based on s&box/XGUI-3 button elements
    /// </summary>
    public class Button : Panel
    {
        public string Text { get; set; } = "";
        public Action? OnClick { get; set; }

        private bool _isHovered = false;
        private bool _isPressed = false;

        public Button()
        {
            Tag = "button";
            
            // Default button styling if not specified
            if (string.IsNullOrEmpty(Style))
            {
                Style = "background-color: #4CAF50; color: white; padding: 10px 20px; border-radius: 4px; cursor: pointer;";
            }
        }

        protected override void OnPaint(SKCanvas canvas)
        {
            base.OnPaint(canvas);

            // Get button state colors
            var baseColor = ComputedStyle?.BackgroundColor ?? new SKColor(76, 175, 80); // Green default
            var textColor = ComputedStyle?.Color ?? SKColors.White;

            // Adjust color based on state
            SKColor bgColor = baseColor;
            if (_isPressed)
            {
                // Darken when pressed
                bgColor = new SKColor(
                    (byte)(baseColor.Red * 0.7),
                    (byte)(baseColor.Green * 0.7),
                    (byte)(baseColor.Blue * 0.7),
                    baseColor.Alpha
                );
            }
            else if (_isHovered)
            {
                // Slightly lighter when hovered
                bgColor = new SKColor(
                    (byte)Math.Min(255, baseColor.Red * 1.1),
                    (byte)Math.Min(255, baseColor.Green * 1.1),
                    (byte)Math.Min(255, baseColor.Blue * 1.1),
                    baseColor.Alpha
                );
            }

            // Draw button background
            using var bgPaint = new SKPaint
            {
                Color = bgColor,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var borderRadius = ComputedStyle?.BorderRadius ?? 4f;
            canvas.DrawRoundRect(ComputedRect, borderRadius, borderRadius, bgPaint);

            // Draw button border if specified
            var borderWidth = ComputedStyle?.BorderWidth ?? 0f;
            if (borderWidth > 0 && ComputedStyle?.BorderColor != null)
            {
                using var borderPaint = new SKPaint
                {
                    Color = ComputedStyle.BorderColor.Value,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = borderWidth
                };
                canvas.DrawRoundRect(ComputedRect, borderRadius, borderRadius, borderPaint);
            }

            // Draw button text
            if (!string.IsNullOrEmpty(Text))
            {
                var fontSize = ComputedStyle?.FontSize ?? 14f;
                
                using var textPaint = new SKPaint
                {
                    Color = textColor,
                    TextSize = fontSize,
                    IsAntialias = true,
                    SubpixelText = true,
                    LcdRenderText = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
                    TextAlign = SKTextAlign.Center
                };

                // Center text in button
                var textBounds = new SKRect();
                textPaint.MeasureText(Text, ref textBounds);
                
                var textX = ComputedRect.MidX;
                var textY = ComputedRect.MidY - textBounds.MidY;

                canvas.DrawText(Text, textX, textY, textPaint);
            }
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            _isPressed = true;
            
            // Invoke click handler
            OnClick?.Invoke();
            
            base.OnMouseDown(e);
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            _isPressed = false;
            base.OnMouseUp(e);
        }

        public override void OnMouseEnter(MouseEventArgs e)
        {
            _isHovered = true;
            base.OnMouseEnter(e);
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            base.OnMouseLeave(e);
        }
    }
}
