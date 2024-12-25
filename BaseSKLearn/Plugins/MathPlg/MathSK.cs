using System;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.Plugins.MathPlg;

public class MathSK
{
    /// <summary>
    /// 得到负数
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    [KernelFunction, Description("得到负数")]
    public string Negative(string number)
    {
        return (int.Parse(number) * -1).ToString();
    }

    /// <summary>
    /// 两个数相减
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns></returns>
    [KernelFunction, Description("两个数相减")]
    [return: Description("减完后的数")]
    public string Subtraction(
        [Description("The value to subtract")] int num1,
        [Description("Amount to subtract")] int num2
    ) => (num1 - num2).ToString();
}
