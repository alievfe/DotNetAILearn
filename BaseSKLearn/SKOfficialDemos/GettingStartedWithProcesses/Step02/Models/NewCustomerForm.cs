using System.Reflection;
using System.Text.Json.Serialization;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;

/// <summary>
/// 表示用于捕获新客户详细信息的数据结构，包括个人信息和联系方式。<br/>
/// 该类用于 <see cref="Step02a_AccountOpening"/> 和 <see cref="Step02b_AccountOpening"/> 示例。
/// </summary>
public class NewCustomerForm
{
    /// <summary>
    /// 获取或设置用户的名字。
    /// </summary>
    [JsonPropertyName("userFirstName")]
    public string UserFirstName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户的姓氏。
    /// </summary>
    [JsonPropertyName("userLastName")]
    public string UserLastName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户的出生日期。
    /// </summary>
    [JsonPropertyName("userDateOfBirth")]
    public string UserDateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户所在的州。
    /// </summary>
    [JsonPropertyName("userState")]
    public string UserState { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户的电话号码。
    /// </summary>
    [JsonPropertyName("userPhoneNumber")]
    public string UserPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户的 ID。
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置用户的电子邮件地址。
    /// </summary>
    [JsonPropertyName("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// 创建一个带有默认值的表单副本。
    /// </summary>
    /// <param name="defaultStringValue">默认字符串值，用于替换空字符串。</param>
    /// <returns>返回带有默认值的表单副本。</returns>
    public NewCustomerForm CopyWithDefaultValues(string defaultStringValue = "Unanswered")
    {
        NewCustomerForm copy = new();
        PropertyInfo[] properties = typeof(NewCustomerForm).GetProperties();

        foreach (PropertyInfo property in properties)
        {
            // 获取属性的值
            string? value = property.GetValue(this) as string;

            // 检查值是否为空字符串
            if (string.IsNullOrEmpty(value))
            {
                property.SetValue(copy, defaultStringValue);
            }
            else
            {
                property.SetValue(copy, value);
            }
        }

        return copy;
    }

    /// <summary>
    /// 检查表单是否已填写完整。
    /// </summary>
    /// <returns>如果所有字段都已填写，则返回 true；否则返回 false。</returns>
    public bool IsFormCompleted()
    {
        return !string.IsNullOrEmpty(UserFirstName)
            && !string.IsNullOrEmpty(UserLastName)
            && !string.IsNullOrEmpty(UserId)
            && !string.IsNullOrEmpty(UserDateOfBirth)
            && !string.IsNullOrEmpty(UserState)
            && !string.IsNullOrEmpty(UserEmail)
            && !string.IsNullOrEmpty(UserPhoneNumber);
    }
}
