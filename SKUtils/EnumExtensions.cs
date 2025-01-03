using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SKUtils;

/// <summary>
/// 提供枚举类型的扩展方法。
/// </summary>
public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> DisplayNameCache = [];

    /// <summary>
    /// 获取枚举字段值上的指定类型属性。
    /// </summary>
    /// <typeparam name="T">要检索的属性类型。</typeparam>
    /// <param name="enumValue">枚举值。</param>
    /// <returns>指定类型的属性实例或null。</returns>
    /// <remarks>
    /// 如果提供的枚举值对应的字段不存在，或者该字段没有指定类型的属性，则返回null。
    /// </remarks>
    public static T? GetAttributeOfType<T>(this Enum enumValue)
        where T : Attribute
    {
        // 使用反射获取枚举值对应的字段信息
        FieldInfo? field = enumValue
            .GetType()
            .GetField(enumValue.ToString(), BindingFlags.Static | BindingFlags.Public); // 将枚举值转换为字符串形式，用于匹配字段名称，指定搜索公共静态字段

        return field?.GetCustomAttributes<T>(inherit: false).FirstOrDefault();
    }

    /// <summary>
    /// 获取枚举值的显示名称。先找有无DisplayAttribute，没有就返回其字符串表示值
    /// </summary>
    /// <param name="enumValue">枚举值。</param>
    /// <returns>
    /// 如果存在 <see cref="DisplayAttribute"/>，则返回其 <see cref="DisplayAttribute.Name"/> 属性；
    /// 否则，返回枚举值的标准字符串表示形式。
    /// </returns>
    public static string? GetDisplayName(this Enum enumValue) =>
        DisplayNameCache.GetOrAdd(
            enumValue,
            e =>
            {
                DisplayAttribute? attributeOfType = e.GetAttributeOfType<DisplayAttribute>();
                return attributeOfType?.Name ?? e.ToString();
            }
        );
}
