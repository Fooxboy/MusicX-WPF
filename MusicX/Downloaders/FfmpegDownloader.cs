using MusicX.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MusicX.Downloaders
{
    public class FfmpegDownloader : IDownloader
    {
        public event DownloadComplete DownloadedEvent;
        public event DownloadProgressChanged DownloadProgressChangedEvent;

        private readonly string ffmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";
        private readonly string ffmpegUrl = "https://fooxboy.blob.core.windows.net/musicx/ffmpeg.exe";
        private WebClient webClient;

        public FfmpegDownloader()
        {
        }

        public async Task DownloadAsync()
        {
            if (!Directory.Exists(ffmpegPath))
            {
                Directory.CreateDirectory(ffmpegPath);
            }

            using (var client = new WebClient())
            {
                StaticService.WebClient = client;
                client.DownloadFileCompleted += FileDownloaded;
                client.DownloadProgressChanged += DownloadProgressChanged;

                await client.DownloadFileTaskAsync(ffmpegUrl, ffmpegPath + "\\ffmpeg.exe");
            }
        }


        private void FileDownloaded(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            DownloadedEvent?.Invoke(ffmpegPath + "\\ffmpeg.exe");
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var max = e.TotalBytesToReceive;
            var current = e.BytesReceived;

            double left = e.TotalBytesToReceive - e.BytesReceived;

            var percent = Convert.ToInt32((current / max) * 100);

            DownloadProgressChangedEvent?.Invoke(percent, current, left);

        }
    }
}
