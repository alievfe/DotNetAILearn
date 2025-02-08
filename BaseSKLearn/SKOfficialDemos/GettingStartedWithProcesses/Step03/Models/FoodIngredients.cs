namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;

/// <summary>
/// 用于如收集食材步骤、切食物步骤、炸食物步骤等的食材枚举。
/// 该枚举列出了烹饪过程中可能用到的各种食材。
/// </summary>
public enum FoodIngredients
{
    // 土豆
    Pototoes,

    // 鱼
    Fish,

    // 面包
    Buns,

    // 酱汁
    Sauce,

    // 调味料
    Condiments,

    // 无
    None,
}

/// <summary>
/// <see cref="FoodIngredients"/> 枚举的扩展类，用于获取友好的字符串名称。
/// 提供了将 FoodIngredients 枚举值转换为更易读的字符串的功能。
/// </summary>
public static class FoodIngredientsExtensions
{
    private static readonly Dictionary<FoodIngredients, string> s_foodIngredientsStrings = new()
    {
        { FoodIngredients.Pototoes, "Potatoes" },
        { FoodIngredients.Fish, "Fish" },
        { FoodIngredients.Buns, "Buns" },
        { FoodIngredients.Sauce, "Sauce" },
        { FoodIngredients.Condiments, "Condiments" },
        { FoodIngredients.None, "None" },
    };

    /// <summary>
    /// 将 <see cref="FoodIngredients"/> 枚举值转换为友好的字符串表示。
    /// </summary>
    /// <param name="ingredient">要转换的食材枚举值。</param>
    /// <returns>友好的字符串名称。</returns>
    public static string ToFriendlyString(this FoodIngredients ingredient)
    {
        return s_foodIngredientsStrings[ingredient];
    }
}
