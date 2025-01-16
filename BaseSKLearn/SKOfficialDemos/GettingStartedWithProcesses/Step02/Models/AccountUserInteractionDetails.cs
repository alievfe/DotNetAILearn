using System;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;

using Microsoft.SemanticKernel;

/// <summary>
/// 表示用户与服务之间交互的详细信息，包括账户的唯一标识符、与用户的对话记录以及用户交互的类型。<br/>
/// 该类在<see cref="Step02a_AccountOpening"/>和<see cref="Step02b_AccountOpening"/>示例中使用。
/// </summary>
public record AccountUserInteractionDetails
{
    /// <summary>
    /// 获取或设置账户的唯一标识符。
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// 获取或设置与用户的对话记录。默认为空列表。
    /// </summary>
    public List<ChatMessageContent> InteractionTranscript { get; set; } =
        new List<ChatMessageContent>();

    /// <summary>
    /// 获取或设置用户交互的类型。
    /// </summary>
    public UserInteractionType UserInteractionType { get; set; }
}

/// <summary>
/// 定义用户交互类型的枚举。
/// </summary>
public enum UserInteractionType
{
    /// <summary>
    /// 投诉交互类型。
    /// </summary>
    Complaint,

    /// <summary>
    /// 请求账户信息的交互类型。
    /// </summary>
    AccountInfoRequest,

    /// <summary>
    /// 开设新账户的交互类型。
    /// </summary>
    OpeningNewAccount,
}
