namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;

/// <summary>
/// 表示一个数据结构，用于捕获新客户的信息，包括个人资料、联系方式、账户ID和账户类型。<br/>
/// 该类在<see cref="Step02a_AccountOpening"/>和<see cref="Step02b_AccountOpening"/>示例中使用。
/// </summary>
public class AccountDetails : NewCustomerForm
{
    /// <summary>
    /// 获取或设置账户的唯一标识符。
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// 获取或设置账户类型。
    /// </summary>
    public AccountType AccountType { get; set; }
}

/// <summary>
/// 定义账户类型的枚举。
/// </summary>
public enum AccountType
{
    /// <summary>
    /// PrimeABC 账户类型。
    /// </summary>
    PrimeABC,

    /// <summary>
    /// 其他账户类型。
    /// </summary>
    Other,
}