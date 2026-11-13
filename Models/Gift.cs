namespace SantaGiftAdvisor.Models;

public sealed record Gift(
    string Id,
    string Name,
    string Description,
    int MinimumAge,
    int MaximumAge,
    decimal Price,
    string[] Interests,
    bool InStock);
