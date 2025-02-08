namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;

/// <summary>
/// 可由 PrepareSingleFoodItemProcess 流程准备的食物项。
/// 该枚举列出了几种常见的食物类型，可用于表示订单中的食物选择。
/// </summary>
public enum FoodItem
{
    // 薯条
    PotatoFries,

    // 炸鱼
    FriedFish,

    // 鱼三明治
    FishSandwich,

    // 炸鱼薯条
    FishAndChips,
}

/// <summary>
/// <see cref="FoodItem"/> 枚举的扩展类，用于获取友好的字符串名称。
/// 提供了将 FoodItem 枚举值转换为更易读的字符串的功能。
/// </summary>
public static class FoodItemExtensions
{
    private static readonly Dictionary<FoodItem, string> s_foodItemsStrings = new()
    {
        { FoodItem.PotatoFries, "Potato Fries" },
        { FoodItem.FriedFish, "Fried Fish" },
        { FoodItem.FishSandwich, "Fish Sandwich" },
        { FoodItem.FishAndChips, "Fish & Chips" },
    };

    /// <summary>
    /// 将 <see cref="FoodItem"/> 枚举值转换为友好的字符串表示。
    /// </summary>
    /// <param name="item">要转换的食物项枚举值。</param>
    /// <returns>友好的字符串名称。</returns>
    public static string ToFriendlyString(this FoodItem item)
    {
        return s_foodItemsStrings[item];
    }
}
