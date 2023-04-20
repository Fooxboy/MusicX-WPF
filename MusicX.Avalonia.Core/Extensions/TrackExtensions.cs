using System.Text;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Core.Extensions;

public static partial class TrackExtensions
{
    public static string GetArtistsString(this PlaylistTrack track)
    {
        var builder = new StringBuilder();
        foreach (var (name, _) in track.MainArtists)
        {
            builder.Append(name).Append(", ");
        }

        if (track.FeaturedArtists?.Any() == true)
        {
            builder.Append("feat. ");
            foreach (var (name, _) in track.MainArtists)
            {
                builder.Append(name).Append(", ");
            }
        }

        builder.Remove(builder.Length - 2, 2);
        return builder.ToString();
    }
}