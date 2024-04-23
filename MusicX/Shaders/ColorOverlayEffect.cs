using System;
using System.Windows;
using System.Windows.Media.Effects;

namespace MusicX.Shaders;

public class ColorOverlayEffect : ShaderEffect
{
    public ColorOverlayEffect()
    {
        PixelShader = new()
        {
            UriSource = new("Shaders/bin/ColorOverlayShader.ps", UriKind.Relative)
        };
        UpdateShaderValue(InputProperty);
    }

    private static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty("Input", typeof(ColorOverlayEffect), 0);
}