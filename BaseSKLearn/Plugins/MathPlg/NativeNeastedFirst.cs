using System;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.Plugins.MathPlg;

public class NativeNeastedFirst(Kernel kernel)
{
    /// <summary>
    /// 通过自然语义先找到最大和最小的2个值，然后用最大值减去最小值得到结果返回
    /// </summary>
    /// <returns></returns>
    [KernelFunction]
    public async Task<string> AnalysisAndSubAsync(string input)
    {
        var MathPlg = kernel.ImportPluginFromDefaultPathPromptDirectory("MathPlg");
        var NativeMathPlugin = kernel.ImportPluginFromObject(new MathSK(), "NativeMathPlugin");
        var maxmin = await kernel.InvokeAsync(
            MathPlg["FindMaxMin"],
            new KernelArguments() { ["input"] = input }
        );

        var nums = maxmin.GetValue<string>().Split("-");
        var vars = new KernelArguments { ["num1"] = nums[0], ["num2"] = nums[1] };

        var res = await kernel.InvokeAsync(NativeMathPlugin[nameof(MathSK.Subtraction)], vars);
        return res.GetValue<string>();
    }
}
