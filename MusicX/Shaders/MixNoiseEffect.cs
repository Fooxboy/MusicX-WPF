using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace MusicX.Shaders;

public class MixNoiseEffect : ShaderEffect
{
    public MixNoiseEffect()
    {
        PixelShader = new()
        {
            UriSource = new("Shaders/bin/MixNoiseShader.ps", UriKind.Relative)
        };
        UpdateShaderValue(InputProperty);
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background, TimeCallback,
            Dispatcher);
        _timer.Start();
    }

    private void TimeCallback(object? sender, EventArgs e)
    {
        SetValue(TimeProperty, (double)GetValue(TimeProperty) + .1);
    }

    private static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty("Input", typeof(MixNoiseEffect), 0);
    
    private static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(double),
        typeof(MixNoiseEffect), new(PixelShaderConstantCallback(0)));
    
    private static readonly DependencyProperty XSizeProperty = DependencyProperty.Register(nameof(XSize), typeof(double),
        typeof(MixNoiseEffect), new(PixelShaderConstantCallback(1)));
    
    public double XSize
    {
        get => (double) GetValue(XSizeProperty);
        set => SetValue(XSizeProperty, value);
    }
    
    private static readonly DependencyProperty YSizeProperty = DependencyProperty.Register(nameof(YSize), typeof(double),
        typeof(MixNoiseEffect), new(PixelShaderConstantCallback(2)));
    
    public double YSize
    {
        get => (double) GetValue(YSizeProperty);
        set => SetValue(YSizeProperty, value);
    }
    
    private static readonly DependencyProperty ZSizeProperty = DependencyProperty.Register(nameof(ZSize), typeof(double),
        typeof(MixNoiseEffect), new(PixelShaderConstantCallback(3)));
    
    public double ZSize
    {
        get => (double) GetValue(ZSizeProperty);
        set => SetValue(ZSizeProperty, value);
    }

    private readonly DispatcherTimer _timer;
}