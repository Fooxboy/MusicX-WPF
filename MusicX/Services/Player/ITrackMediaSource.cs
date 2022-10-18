﻿using System.Threading.Tasks;
using Windows.Media.Core;
using MusicX.Services.Player.Playlists;

namespace MusicX.Services.Player;

public interface ITrackMediaSource
{
    Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track);
}