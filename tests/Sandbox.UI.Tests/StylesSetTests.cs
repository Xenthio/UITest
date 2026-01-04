using System;
using Xunit;
using Sandbox.UI;

namespace Sandbox.UI.Tests;

/// <summary>
/// Tests for new style properties added from s&box
/// </summary>
public class StylesSetTests
{
	[Fact]
	public void FontWeight_ParsesNamedValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("font-weight", "thin"));
		Assert.Equal(100, styles.FontWeight);

		Assert.True(styles.Set("font-weight", "light"));
		Assert.Equal(300, styles.FontWeight);

		Assert.True(styles.Set("font-weight", "normal"));
		Assert.Equal(400, styles.FontWeight);

		Assert.True(styles.Set("font-weight", "bold"));
		Assert.Equal(700, styles.FontWeight);

		Assert.True(styles.Set("font-weight", "black"));
		Assert.Equal(900, styles.FontWeight);
	}

	[Fact]
	public void FontWeight_ParsesNumericValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("font-weight", "100"));
		Assert.Equal(100, styles.FontWeight);

		Assert.True(styles.Set("font-weight", "600"));
		Assert.Equal(600, styles.FontWeight);
	}

	[Fact]
	public void TextTransform_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("text-transform", "capitalize"));
		Assert.Equal(TextTransform.Capitalize, styles.TextTransform);

		Assert.True(styles.Set("text-transform", "uppercase"));
		Assert.Equal(TextTransform.Uppercase, styles.TextTransform);

		Assert.True(styles.Set("text-transform", "lowercase"));
		Assert.Equal(TextTransform.Lowercase, styles.TextTransform);

		Assert.True(styles.Set("text-transform", "none"));
		Assert.Equal(TextTransform.None, styles.TextTransform);
	}

	[Fact]
	public void TextDecoration_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("text-decoration-line", "underline"));
		Assert.True(styles.TextDecorationLine.HasValue);
		Assert.True((styles.TextDecorationLine.Value & TextDecoration.Underline) != 0);
	}

	[Fact]
	public void TextFilter_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("text-filter", "bilinear"));
		Assert.Equal(Rendering.FilterMode.Bilinear, styles.TextFilter);

		Assert.True(styles.Set("text-filter", "point"));
		Assert.Equal(Rendering.FilterMode.Point, styles.TextFilter);
	}

	[Fact]
	public void ImageRendering_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("image-rendering", "pixelated"));
		Assert.Equal(ImageRendering.Point, styles.ImageRendering); // pixelated maps to Point in s&box

		Assert.True(styles.Set("image-rendering", "auto"));
		Assert.Equal(ImageRendering.Anisotropic, styles.ImageRendering);
	}

	[Fact]
	public void BackgroundRepeat_ParsesClamp()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("background-repeat", "clamp"));
		Assert.Equal(BackgroundRepeat.Clamp, styles.BackgroundRepeat);
	}

	[Fact]
	public void MaskMode_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("mask-mode", "alpha"));
		Assert.Equal(MaskMode.Alpha, styles.MaskMode);

		Assert.True(styles.Set("mask-mode", "luminance"));
		Assert.Equal(MaskMode.Luminance, styles.MaskMode);
	}

	[Fact]
	public void PerspectiveOrigin_ParsesValues()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("perspective-origin", "50% 50%"));
		Assert.NotNull(styles.PerspectiveOriginX);
		Assert.NotNull(styles.PerspectiveOriginY);
	}

	[Fact]
	public void Flex_SupportsKeywords()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert - "none" expands to 0 0 auto
		Assert.True(styles.Set("flex", "none"));
		Assert.Equal(0f, styles.FlexGrow);
		Assert.Equal(0f, styles.FlexShrink);
		Assert.Equal(LengthUnit.Auto, styles.FlexBasis?.Unit);

		// "auto" expands to 1 1 auto
		styles = new Styles();
		Assert.True(styles.Set("flex", "auto"));
		Assert.Equal(1f, styles.FlexGrow);
		Assert.Equal(1f, styles.FlexShrink);
		Assert.Equal(LengthUnit.Auto, styles.FlexBasis?.Unit);

		// "initial" expands to 0 1 auto (s&box behavior: shrink=0, grow=1)
		styles = new Styles();
		Assert.True(styles.Set("flex", "initial"));
		Assert.Equal(1f, styles.FlexGrow); // s&box has grow=1
		Assert.Equal(0f, styles.FlexShrink); // s&box has shrink=0
		Assert.Equal(LengthUnit.Auto, styles.FlexBasis?.Unit);
	}

	[Fact]
	public void AnimationIterationCount_SupportsInfinite()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("animation-iteration-count", "infinite"));
		Assert.True(float.IsPositiveInfinity(styles.AnimationIterationCount.GetValueOrDefault()));
	}

	[Fact]
	public void CaretColor_ParsesColor()
	{
		// Arrange
		var styles = new Styles();

		// Act & Assert
		Assert.True(styles.Set("caret-color", "#ff0000"));
		Assert.NotNull(styles.CaretColor);
		Assert.Equal(1.0f, styles.CaretColor.Value.r);
		Assert.Equal(0.0f, styles.CaretColor.Value.g);
		Assert.Equal(0.0f, styles.CaretColor.Value.b);
	}

	[Fact]
	public void Padding_ProgressiveParsing()
	{
		// Arrange
		var styles = new Styles();

		// Act - Single value
		Assert.True(styles.Set("padding", "10px"));
		Assert.Equal(10f, styles.PaddingTop?.Value);
		Assert.Equal(10f, styles.PaddingRight?.Value);
		Assert.Equal(10f, styles.PaddingBottom?.Value);
		Assert.Equal(10f, styles.PaddingLeft?.Value);

		// Act - Two values
		styles = new Styles();
		Assert.True(styles.Set("padding", "10px 20px"));
		Assert.Equal(10f, styles.PaddingTop?.Value);
		Assert.Equal(20f, styles.PaddingRight?.Value);
		Assert.Equal(10f, styles.PaddingBottom?.Value);
		Assert.Equal(20f, styles.PaddingLeft?.Value);
	}

	[Fact]
	public void Margin_ProgressiveParsing()
	{
		// Arrange
		var styles = new Styles();

		// Act - Four values
		Assert.True(styles.Set("margin", "10px 20px 30px 40px"));
		Assert.Equal(10f, styles.MarginTop?.Value);
		Assert.Equal(20f, styles.MarginRight?.Value);
		Assert.Equal(30f, styles.MarginBottom?.Value);
		Assert.Equal(40f, styles.MarginLeft?.Value);
	}

	[Fact]
	public void BorderRadius_ProgressiveParsing()
	{
		// Arrange
		var styles = new Styles();

		// Act - Single value
		Assert.True(styles.Set("border-radius", "5px"));
		Assert.Equal(5f, styles.BorderTopLeftRadius?.Value);
		Assert.Equal(5f, styles.BorderTopRightRadius?.Value);
		Assert.Equal(5f, styles.BorderBottomRightRadius?.Value);
		Assert.Equal(5f, styles.BorderBottomLeftRadius?.Value);
	}
}
