using System;
using System.Globalization;
using System.Windows.Data;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Converters;

public class TrackArtistsToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PlaylistTrack track)
            return track.GetArtistsString();
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}