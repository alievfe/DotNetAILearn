namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;

/// <summary>
/// 包含营销数据库中新条目的详细信息，包括账户标识符、联系人姓名、电话号码和电子邮件地址。<br/>
/// 该类在<see cref="Step02a_AccountOpening"/>和<see cref="Step02b_AccountOpening"/>示例中使用。
/// </summary>
public record MarketingNewEntryDetails
{
    /// <summary>
    /// 获取或设置账户的唯一标识符。
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// 获取或设置联系人的全名。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置联系人的电话号码。
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// 获取或设置联系人的电子邮件地址。
    /// </summary>
    public string Email { get; set; }
}