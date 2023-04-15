namespace MusicX.Avalonia.Core.Models;

public record CatalogGroup(
    long Id,
    int MemberStatus,
    int MembersCount,
    string Activity,
    int Trending,
    string Name,
    string ScreenName,
    int IsClosed,
    string Type,
    int IsAdmin,
    int IsMember,
    int IsAdvertiser,
    int Verified,
    string Photo50,
    string Photo100,
    string Photo200
);