using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace SantaGiftAdvisor;

public static class GiftTools
{
    private sealed record GiftItem(string Id, string Name, string Description, int MinAge, int MaxAge, decimal Price, string[] Interests, bool InStock);

    private static readonly GiftItem[] Catalogue =
    [
        new("G001", "Beginner Robotics Kit", "A hands-on robotics kit with simple programmable projects.", 9, 15, 89m, ["technology", "STEM", "robotics", "coding"], true),
        new("G002", "Wireless Headphones", "Comfortable over-ear headphones for music, games, and study.", 12, 99, 79m, ["music", "gaming", "technology"], true),
        new("G003", "Watercolour Starter Set", "A starter set with paints, brushes, and a paper pad.", 8, 99, 42m, ["art", "painting", "creativity"], true),
        new("G004", "Adventure Board Game", "A cooperative strategy game for family game nights.", 10, 99, 55m, ["games", "family", "strategy"], true),
        new("G005", "Junior Basketball", "A durable basketball for outdoor play.", 8, 16, 29m, ["sport", "basketball", "outdoors"], false),
        new("G006", "Mechanical Keyboard", "A compact mechanical keyboard for gaming and productivity.", 13, 99, 99m, ["technology", "gaming", "computers"], true),
        new("G007", "Build-Your-Own Telescope", "An introductory telescope kit for exploring the night sky.", 10, 99, 74m, ["science", "space", "astronomy"], true),
        new("G008", "Premium Sketchbook", "A hardcover sketchbook with heavyweight paper.", 8, 99, 24m, ["art", "drawing", "writing"], true)
    ];

    public static readonly AIFunction SearchGiftsFunction = AIFunctionFactory.Create(SearchGifts);
    public static readonly AIFunction CheckGiftAvailabilityFunction = AIFunctionFactory.Create(CheckGiftAvailability);

    [Description("Search the sample gift catalogue by recipient age, interests, and maximum budget.")]
    public static string SearchGifts(
        [Description("Recipient age in years.")] int age,
        [Description("Comma-separated interests, such as gaming, music, sport, or art.")] string interests,
        [Description("Maximum budget in dollars.")] decimal maximumBudget)
    {
        var requested = interests.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var matches = Catalogue
            .Where(g => age >= g.MinAge && age <= g.MaxAge && g.Price <= maximumBudget)
            .Select(g => new { Gift = g, Score = g.Interests.Count(tag => requested.Any(r => tag.Contains(r, StringComparison.OrdinalIgnoreCase) || r.Contains(tag, StringComparison.OrdinalIgnoreCase))) })
            .Where(x => requested.Length == 0 || x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Gift.Price)
            .Take(5)
            .Select(x => x.Gift)
            .ToArray();

        return matches.Length == 0
            ? "No matching gifts found. Try broadening the interests or increasing the budget."
            : string.Join(Environment.NewLine, matches.Select(g => $"{g.Id}: {g.Name} — {g.Description} Price: ${g.Price:0.00}."));
    }

    [Description("Check the sample catalogue stock status for a gift using its gift ID.")]
    public static string CheckGiftAvailability([Description("Gift ID returned by SearchGifts, for example G001.")] string giftId)
    {
        var gift = Catalogue.FirstOrDefault(g => g.Id.Equals(giftId, StringComparison.OrdinalIgnoreCase));
        if (gift is null) return $"Gift {giftId} was not found in the sample catalogue.";
        return gift.InStock
            ? $"{gift.Name} ({gift.Id}) is marked as in stock in the sample catalogue."
            : $"{gift.Name} ({gift.Id}) is currently marked as out of stock in the sample catalogue.";
    }
}
