using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Interop;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using MusicX.Helpers;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Extensions;
using DataTransferManagerInterop = MusicX.Helpers.DataTransferManagerInterop;

namespace MusicX.Services;

public class ShareService(Logger logger, DownloaderService downloaderService, ISnackbarService snackbarService)
{
    private (PlaylistTrack track, FileInfo file)? _pendingTrack;
    private WindowInteropHelper? _window;

    public void AssignWindow(WindowInteropHelper window)
    {
        _window = window;
        
        var manager = DataTransferManagerInterop.GetForWindow(window.Handle);
        
        manager.DataRequested += TransferManagerOnDataRequested;
    }

    private async void TransferManagerOnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
    {
        var deferral = args.Request.GetDeferral();

        try
        {
            var data = args.Request.Data;

            SetApplicationDetails(data);

            if (_pendingTrack is not null)
            {
                var (track, file) = _pendingTrack.Value;
                await SetTrackAsync(data, track, file);
            }
            else
                throw new InvalidOperationException("No pending data to share");
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to respond to a share request");
            
            args.Request.FailWithDisplayText("Упс! Что-то пошло не так");
        }
        finally
        {
            deferral.Complete();
        }
    }

    private async Task SetTrackAsync(DataPackage data, PlaylistTrack track, FileInfo file)
    {
        data.Properties.Title = $"{track.GetArtistsString()} - {track.Title}";
        if (track.AlbumId?.CoverUrl is not null)
            data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(track.AlbumId.CoverUrl));
       
        // todo url-only option
        // if (track.Data is VkTrackData trackData)
        //     data.SetWebLink(new Uri($"https://vk.com/audio{trackData.Info}"));
        
        data.SetStorageItems([await StorageFile.GetFileFromPathAsync(file.FullName)]);
    }

    private static void SetApplicationDetails(DataPackage data)
    {
        data.Properties.ApplicationName = "MusicX Player";
        data.Properties.Square30x30Logo = new EmbeddedFileStreamReference("MusicX.StoreLogo.scale-30.png", "image/png");
    }

    public async void ShareTrack(PlaylistTrack track)
    {
        snackbarService.Show("Подождите...", "Мы готовим трек для отправки", TimeSpan.FromSeconds(5));
        
        var file = new FileInfo(Path.Join(Directory.CreateTempSubdirectory("MusicX").FullName, $"{track.GetArtistsString()} - {track.Title}.mp3"));

        // todo fix ffmpeg blocking thread on start
        await Task.Run(() => downloaderService.DownloadAudioAsync(track, destinationFile: file));
        
        _pendingTrack = (track, file);
        
        if (_window is not null)
            DataTransferManagerInterop.ShowForWindow(_window.Handle);
    }
}