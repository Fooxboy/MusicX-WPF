using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using DynamicData;
using MusicX.Avalonia.Core.Blocks;
using MusicX.Avalonia.Core.Models;
using ReactiveUI;

namespace MusicX.Avalonia.Controls;

public class BigBannersControl : TemplatedControl
{
    public static readonly StyledProperty<CatalogBanner?> ActiveBannerProperty =
        AvaloniaProperty.Register<BigBannersControl, CatalogBanner?>("ActiveBanner");

    public CatalogBanner? ActiveBanner
    {
        get => GetValue(ActiveBannerProperty);
        set => SetValue(ActiveBannerProperty, value);
    }

    public static readonly StyledProperty<BannersBlock> BlockProperty =
        AvaloniaProperty.Register<BigBannersControl, BannersBlock>("Block");

    public BannersBlock Block
    {
        get => GetValue(BlockProperty);
        set => SetValue(BlockProperty, value);
    }

    public static readonly StyledProperty<ICommand> BannerSelectCommandProperty =
        AvaloniaProperty.Register<BigBannersControl, ICommand>("BannerSelectCommand");

    public ICommand BannerSelectCommand
    {
        get => GetValue(BannerSelectCommandProperty);
        set => SetValue(BannerSelectCommandProperty, value);
    }

    static BigBannersControl()
    {
        BlockProperty.Changed.Subscribe(OnBlockChanged);
    }

    private static void OnBlockChanged(AvaloniaPropertyChangedEventArgs<BannersBlock> args)
    {
        if (args.Sender is BigBannersControl control)
            control.ActiveBanner = args.NewValue.Value.Banners.First();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        BannerSelectCommand = ReactiveCommand.Create<CatalogBanner>(SelectBanner);
    }

    private void SelectBanner(CatalogBanner banner)
    {
        var distance = Block.Banners.IndexOf(banner) - Block.Banners.IndexOf(ActiveBanner);
        ActiveBanner = banner;
        var scroller = this.GetVisualDescendants().OfType<ScrollViewer>().Single(b => b.Name == "PART_BannersScroll");
        scroller.Offset += new Vector(distance * (880d / 2), 0);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _changeDisposable?.Dispose();
        _changeDisposable = null;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _changeDisposable = Observable.Timer(TimeSpan.FromSeconds(10))
                                      .ObserveOn(RxApp.MainThreadScheduler)
                                      .Subscribe(_ =>
                                      {
                                          var index = Block.Banners.IndexOf(ActiveBanner) + 1;
                                          if (index >= Block.Banners.Count)
                                              index = 0;
                                          
                                          SelectBanner(Block.Banners.ElementAt(index));
                                      });
    }

    private IDisposable? _changeDisposable;
}