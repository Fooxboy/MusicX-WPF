using System;
using System.Collections.Generic;

namespace MusicX.Models
{
    public class ConfigModel
    {
        public string? AnonToken { get; set; }
        public string? AccessToken { get; set; }
        
        public DateTimeOffset AccessTokenTtl { get; set; } = DateTimeOffset.Now + TimeSpan.FromHours(2.5);

        public long UserId { get; set; }

        public string UserName { get; set; }

        public bool? ShowRPC { get; set; }

        public int? FullScreenMonitor { get; set; }

        public int? Volume { get; set; }
        
        public bool IsMuted { get; set; }

        public bool? BroadcastVK { get; set; }
        
        public string? DownloadDirectory { get; set; }

        public string BoomToken { get; set; }
        
        public DateTimeOffset BoomTokenTtl { get; set; }

        public string BoomUuid { get; set; }

        public NotifyMessagesConfig NotifyMessages { get; set; }

        public List<string>? IgnoredArtists { get; set; }

        public bool? AnimatedBackground { get; set; }

        public bool? WinterTheme { get; set; }

        public int? MixerVolume { get; set; }

        public bool? MinimizeToTray { get; set; }
        
        public bool? GetBetaUpdates { get; set; }

        public double Width { get; set; } = 1440;

        public double Height { get; set; } = 960;
        
        public string? DeviceId { get; set; }
        
        public string? ExchangeToken { get; set; }
    }
}
