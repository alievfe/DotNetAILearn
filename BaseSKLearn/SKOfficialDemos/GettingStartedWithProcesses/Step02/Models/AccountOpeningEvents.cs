namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;

/// <summary>
/// 处理与开户场景相关的事件。<br/>
/// 该类用于<see cref="Step02a_AccountOpening"/>, <see cref="Step02b_AccountOpening"/> 示例中
/// </summary>
public static class AccountOpeningEvents
{
    /// <summary>
    /// 开始处理流程
    /// </summary>
    public static readonly string StartProcess = nameof(StartProcess);

    /// <summary>
    /// 新客户表单欢迎消息完成
    /// </summary>
    public static readonly string NewCustomerFormWelcomeMessageComplete = nameof(
        NewCustomerFormWelcomeMessageComplete
    );

    /// <summary>
    /// 新客户表单完成
    /// </summary>
    public static readonly string NewCustomerFormCompleted = nameof(NewCustomerFormCompleted);

    /// <summary>
    /// 新客户表单需要更多信息
    /// </summary>
    public static readonly string NewCustomerFormNeedsMoreDetails = nameof(
        NewCustomerFormNeedsMoreDetails
    );

    /// <summary>
    /// 客户交互记录准备就绪
    /// </summary>
    public static readonly string CustomerInteractionTranscriptReady = nameof(
        CustomerInteractionTranscriptReady
    );

    /// <summary>
    /// 新账户验证检查通过
    /// </summary>
    public static readonly string NewAccountVerificationCheckPassed = nameof(
        NewAccountVerificationCheckPassed
    );

    /// <summary>
    /// 信用评分检查批准
    /// </summary>
    public static readonly string CreditScoreCheckApproved = nameof(CreditScoreCheckApproved);

    /// <summary>
    /// 信用评分检查拒绝
    /// </summary>
    public static readonly string CreditScoreCheckRejected = nameof(CreditScoreCheckRejected);

    /// <summary>
    /// 欺诈检测检查通过
    /// </summary>
    public static readonly string FraudDetectionCheckPassed = nameof(FraudDetectionCheckPassed);

    /// <summary>
    /// 欺诈检测检查失败
    /// </summary>
    public static readonly string FraudDetectionCheckFailed = nameof(FraudDetectionCheckFailed);

    /// <summary>
    /// 新账户详情准备就绪
    /// </summary>
    public static readonly string NewAccountDetailsReady = nameof(NewAccountDetailsReady);

    /// <summary>
    /// 新市场营销记录信息准备就绪
    /// </summary>
    public static readonly string NewMarketingRecordInfoReady = nameof(NewMarketingRecordInfoReady);

    /// <summary>
    /// 新市场营销条目创建
    /// </summary>
    public static readonly string NewMarketingEntryCreated = nameof(NewMarketingEntryCreated);

    /// <summary>
    /// CRM记录信息准备就绪
    /// </summary>
    public static readonly string CRMRecordInfoReady = nameof(CRMRecordInfoReady);

    /// <summary>
    /// CRM记录条目创建
    /// </summary>
    public static readonly string CRMRecordInfoEntryCreated = nameof(CRMRecordInfoEntryCreated);

    /// <summary>
    /// 欢迎包创建
    /// </summary>
    public static readonly string WelcomePacketCreated = nameof(WelcomePacketCreated);

    /// <summary>
    /// 邮件服务发送成功
    /// </summary>
    public static readonly string MailServiceSent = nameof(MailServiceSent);
}
