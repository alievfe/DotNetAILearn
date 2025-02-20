using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using SKUtils.Web;

namespace Microsoft.SemanticKernel;
/// <summary>
/// 用于向 <see cref="IServiceCollection"/> 注册 <see cref="ITextSearch"/> 的扩展方法。
/// </summary>
public static class ShaBingWebServiceCollectionExtensions
{
    /// <summary>
    /// 使用指定的服务 ID 注册一个 <see cref="ITextSearch"/> 实例。
    /// </summary>
    /// <param name="services">要在其上注册 <see cref="ITextSearch"/> 的 <see cref="IServiceCollection"/>。</param>
    /// <param name="options">创建 <see cref="ShaBingSearch"/> 时使用的 <see cref="ShaBingSearchConfig"/> 实例。</param>
    /// <param name="serviceId">用作服务键的可选服务 ID。</param>
    public static IServiceCollection AddShaBingSearch(
        this IServiceCollection services,
        ShaBingSearchConfig? config = null,
        string? serviceId = default
    )
    {
        // 向服务集合中添加键控单例服务
        services.AddKeyedSingleton<ITextSearch>(
            serviceId,
            (sp, obj) =>
            {
                // 如果传入的选项为空，则从服务提供者中获取 BingTextSearchOptions 实例
                var selectedOptions = config ?? sp.GetService<ShaBingSearchConfig>();
                return new ShaBingSearch(selectedOptions);
            }
        );

        return services;
    }
}
