namespace MusicX.Avalonia.Core.Models;

public record CatalogProfile(
    long Id,
    CatalogCity City,
    CatalogCountry Country,
    string Photo200,
    string Activity,
    int FollowersCount,
    ICollection<CatalogCareer> Career,
    int University,
    string UniversityName,
    int Faculty,
    string FacultyName,
    int Graduation,
    string ScreenName,
    string Photo50,
    string Photo100,
    int Verified,
    int Trending,
    int FriendStatus,
    string FirstName,
    string LastName,
    bool CanAccessClosed,
    bool IsClosed,
    string EducationForm,
    string EducationStatus
);