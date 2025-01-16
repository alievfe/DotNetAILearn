using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 帮助用户填写新账户表单的步骤。<br/>
/// 还为用户提供欢迎消息。
/// </summary>
public class CompleteNewCustomerFormStep : KernelProcessStep<NewCustomerFormState>
{
    public static class Functions
    {
        public const string NewAccountProcessUserInfo = nameof(NewAccountProcessUserInfo);
        public const string NewAccountWelcome = nameof(NewAccountWelcome);
    }

    internal NewCustomerFormState? _state;

    /*
        目标是填写表单所需的所有字段。
        用户可能在一个消息中提供多个表单字段的信息。
        用户需要填写表单，所有表单字段都是必需的。

        <CURRENT_FORM_STATE>
        {{current_form_state}}
        <CURRENT_FORM_STATE>

        指导：
        - 如果有缺失的详细信息，请向用户提供有用的消息，以帮助填写剩余的字段。
        - 您的目标是帮助引导用户提供当前表单中缺失的详细信息。
        - 鼓励用户提供剩余的详细信息，并在必要时提供示例。
        - 值为“Unanswered”的字段需要用户回答。
        - 如果用户未提供预期的格式，请正确格式化电话号码和用户 ID。
        - 如果用户未在电话号码中使用括号，请添加括号。
        - 对于日期字段，如果日期格式不明确，请首先与用户确认。例如，02/03 03/02 可能是 3 月 2 日或 2 月 3 日。
    */
    internal string _formCompletionSystemPrompt = """
        The goal is to fill up all the fields needed for a form.
        The user may provide information to fill up multiple fields of the form in one message.
        The user needs to fill up a form, all the fields of the form are necessary

        <CURRENT_FORM_STATE>
        {{current_form_state}}
        <CURRENT_FORM_STATE>

        GUIDANCE:
        - If there are missing details, give the user a useful message that will help fill up the remaining fields.
        - Your goal is to help guide the user to provide the missing details on the current form.
        - Encourage the user to provide the remainingdetails with examples if necessary.
        - Fields with value 'Unanswered' need to be answered by the user, please do not use this' Unanswered 'string as user input to fill in the field.
        - Format phone numbers and user ids correctly if the user does not provide the expected format.
        - If the user does not make use of parenthesis in the phone number, add them.
        - For date fields, confirm with the user first if the date format is not clear. Example 02/03 03/02 could be March 2nd or February 3rd.
        """;

    /*
        您好，我可以帮助您填写开设新账户所需的信息。
        请提供一些个人信息，例如名字和姓氏，以开始。
    */
    internal string _welcomeMessage = """
        Hello there, I can help you out fill out the information needed to open a new account with us.
        Please provide some personal information like first name and last name to get started.
        """;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public override ValueTask ActivateAsync(KernelProcessStepState<NewCustomerFormState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    // 初始assistant打招呼
    [KernelFunction(Functions.NewAccountWelcome)]
    public async Task NewAccountWelcomeMessageAsync(
        KernelProcessStepContext context,
        Kernel _kernel
    )
    {
        _state?.conversation.Add(
            new ChatMessageContent { Role = AuthorRole.Assistant, Content = _welcomeMessage }
        );
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.NewCustomerFormWelcomeMessageComplete,
                Data = _welcomeMessage,
            }
        );
    }

    private Kernel CreateNewCustomerFormKernel(Kernel _baseKernel)
    {
        // 创建另一个仅使用私有函数来填写新客户表单的 Kernel
        Kernel kernel = new(_baseKernel.Services);
        kernel.ImportPluginFromFunctions(
            "FillForm",
            [
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedFirstName,
                    functionName: nameof(OnUserProvidedFirstName)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedLastName,
                    functionName: nameof(OnUserProvidedLastName)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedDOBDetails,
                    functionName: nameof(OnUserProvidedDOBDetails)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedStateOfResidence,
                    functionName: nameof(OnUserProvidedStateOfResidence)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedPhoneNumber,
                    functionName: nameof(OnUserProvidedPhoneNumber)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedUserId,
                    functionName: nameof(OnUserProvidedUserId)
                ),
                KernelFunctionFactory.CreateFromMethod(
                    OnUserProvidedEmailAddress,
                    functionName: nameof(OnUserProvidedEmailAddress)
                ),
            ]
        );

        return kernel;
    }

    // 这个function记录用户历史消息和表单状态，。如果回复成功获得，则将其添加到对话历史中并检查是否已经完成了用户信息收集。如果是，就发出“用户表单已完成”和“客户交互转录已准备好”的事件；否则，发出“需要更多用户详情”的事件以继续收集信息。
    [KernelFunction(Functions.NewAccountProcessUserInfo)]
    public async Task CompleteNewCustomerFormAsync(
        KernelProcessStepContext context,
        string userMessage,
        Kernel _kernel
    )
    {
        // 跟踪所有用户交互
        _state?.conversation.Add(
            new ChatMessageContent { Role = AuthorRole.User, Content = userMessage }
        );

        // 创建一个使用私有函数填写表单的kernel
        Kernel kernel = CreateNewCustomerFormKernel(_kernel);

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.7,
            MaxTokens = 2048,
        };

        ChatHistory chatHistory = new();
        
        // 将当前表单状态序列化放入提示词中用于检测未填写项
        chatHistory.AddSystemMessage(
            _formCompletionSystemPrompt.Replace(
                "{{current_form_state}}",
                JsonSerializer.Serialize(
                    _state!.newCustomerForm.CopyWithDefaultValues(),
                    _jsonOptions
                )
            )
        );
        // 同步之前存储的历史对话记录
        chatHistory.AddRange(_state.conversation);
        IChatCompletionService chatService =
            kernel.Services.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response = await chatService
            .GetChatMessageContentAsync(chatHistory, settings, kernel)
            .ConfigureAwait(false);
        var assistantResponse = "";

        if (response != null)
        {
            assistantResponse = response.Items[0].ToString();
            // 跟踪所有助手交互
            _state?.conversation.Add(
                new ChatMessageContent { Role = AuthorRole.Assistant, Content = assistantResponse }
            );
        }

        // 如果检测表单已经填写完成...
        if (_state?.newCustomerForm != null && _state.newCustomerForm.IsFormCompleted())
        {
            Console.WriteLine(
                $"[NEW_USER_FORM_COMPLETED]: {JsonSerializer.Serialize(_state?.newCustomerForm)}"
            );
            // 所有用户信息已收集，携带表单信息发送NewCustomerFormCompleted事件，携带聊天交互信息发送CustomerInteractionTranscriptReady事件
            await context.EmitEventAsync(
                new()
                {
                    Id = AccountOpeningEvents.NewCustomerFormCompleted,
                    Data = _state?.newCustomerForm,
                    Visibility = KernelProcessEventVisibility.Public,
                }
            );
            await context.EmitEventAsync(
                new()
                {
                    Id = AccountOpeningEvents.CustomerInteractionTranscriptReady,
                    Data = _state?.conversation,
                    Visibility = KernelProcessEventVisibility.Public,
                }
            );
            return;
        }

        // 而如果未填写完成，则发出事件：NewCustomerFormNeedsMoreDetails
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.NewCustomerFormNeedsMoreDetails,
                Data = assistantResponse,
            }
        );
    }

    // 用户提供名字的详细信息
    [Description("User provided details of first name")]
    private Task OnUserProvidedFirstName(string firstName)
    {
        if (!string.IsNullOrEmpty(firstName) && _state != null)
        {
            _state.newCustomerForm.UserFirstName = firstName;
        }

        return Task.CompletedTask;
    }

    // 用户提供姓氏详细信息
    [Description("User provided details of last name")]
    private Task OnUserProvidedLastName(string lastName)
    {
        if (!string.IsNullOrEmpty(lastName) && _state != null)
        {
            _state.newCustomerForm.UserLastName = lastName;
        }

        return Task.CompletedTask;
    }


    // 用户提供居住州的详细信息，必须是2个字母的大写州缩写格式
    [Description("User provided details of USA State the user lives in, must be in 2-letter Uppercase State Abbreviation format")]
    private Task OnUserProvidedStateOfResidence(string stateAbbreviation)
    {
        if (!string.IsNullOrEmpty(stateAbbreviation) && _state != null)
        {
            _state.newCustomerForm.UserState = stateAbbreviation;
        }

        return Task.CompletedTask;
    }

    // 用户提供出生日期的详细信息，必须是MM/DD/YYYY格式
    [Description("User provided details of date of birth, must be in the format MM/DD/YYYY")]
    private Task OnUserProvidedDOBDetails(string date)
    {
        if (!string.IsNullOrEmpty(date) && _state != null)
        {
            _state.newCustomerForm.UserDateOfBirth = date;
        }

        return Task.CompletedTask;
    }
    
    // 用户提供电话号码的详细信息，必须是(\\d{3})-\\d{3}-\\d{4}格式
    [Description("User provided details of phone number, must be in the format (\\d{3})-\\d{3}-\\d{4}")]
    private Task OnUserProvidedPhoneNumber(string phoneNumber)
    {
        if (!string.IsNullOrEmpty(phoneNumber) && _state != null)
        {
            _state.newCustomerForm.UserPhoneNumber = phoneNumber;
        }

        return Task.CompletedTask;
    }

    // 用户提供用户ID的详细信息，必须是\\d{3}-\\d{3}-\\d{4}格式
    [Description("User provided details of userId, must be in the format \\d{3}-\\d{3}-\\d{4}")]
    private Task OnUserProvidedUserId(string userId)
    {
        if (!string.IsNullOrEmpty(userId) && _state != null)
        {
            _state.newCustomerForm.UserId = userId;
        }

        return Task.CompletedTask;
    }

    // 用户提供电子邮件地址，必须是有效的电子邮件格式
    [Description("User provided email address, must be in the an email valid format")]
    private Task OnUserProvidedEmailAddress(string emailAddress)
    {
        if (!string.IsNullOrEmpty(emailAddress) && _state != null)
        {
            _state.newCustomerForm.UserEmail = emailAddress;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// <see cref="CompleteNewCustomerFormStep"/> 的状态对象。
/// </summary>
public class NewCustomerFormState
{
    internal NewCustomerForm newCustomerForm { get; set; } = new();
    internal List<ChatMessageContent> conversation { get; set; } = [];
}
