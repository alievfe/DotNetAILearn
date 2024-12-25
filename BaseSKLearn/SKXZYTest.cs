using System;
using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Plugins.MathPlg;
using Microsoft.SemanticKernel;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class SKXZYTest(Kernel kernel)
{
    /// <summary>
    /// 测试Plugins翻译
    /// </summary>
    /// <param name="input"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public async Task Translate(string input, string language)
    {
        // import plugin
        var translatePlugin = kernel.ImportPluginFromDefaultPathPromptDirectory("Translate");
        var res = await kernel.InvokeAsync(translatePlugin[language], new() { ["input"] = input });
        Console.WriteLine(res);
    }

    /// <summary>
    /// 测试Plugins计算
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns></returns>
    public async Task Calculate(string num1, string num2)
    {
        //导入本地技能，多参数
        var calculatePlugin = kernel.ImportPluginFromDefaultPathPromptDirectory("Calculate");

        var variables = new KernelArguments { ["num1"] = num1, ["num2"] = num2 };
        var res = await kernel.InvokeAsync(calculatePlugin["Addition"], variables);
        Console.WriteLine(res);
    }

    /// <summary>
    /// 原生函数测试
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns></returns>
    public async Task NativeSub(string num1, string num2)
    {
        //导入原生函数
        var mathPlugin = kernel.ImportPluginFromObject(new MathSK(), "MathPlugin");

        var variables = new KernelArguments { ["num1"] = num1, ["num2"] = num2 };
        var res = await kernel.InvokeAsync(mathPlugin["Subtraction"], variables);
        Console.WriteLine(res);
    }

    /// <summary>
    /// 嵌套函数 prompt func -> native func
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns></returns>
    public async Task NestedFunc(string num1, string num2)
    {
        //嵌套函数使用，在prompty中使用  {{Plugin.Fun}} 可以嵌套调用
        var calculatePlugin = kernel.ImportPluginFromDefaultPathPromptDirectory("Calculate");
        //MathPlg、Multiplication 中可以嵌套其他函数
        kernel.ImportPluginFromObject(new MathSK(), "MathPlg");
        var vars = new KernelArguments { ["num1"] = num1, ["num2"] = num2 };
        var res = await kernel.InvokeAsync(calculatePlugin["Multiplication"], vars);
        Console.WriteLine(res);
    }

    /// <summary>
    /// 原生嵌套，先由native func进入，其中调用其它的包括prompt func执行逻辑
    /// 通过自然语义先找到最大和最小的2个值，然后用最大值减去最小值得到结果返回
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns> <summary>
    public async Task NativeNestedFunc(string msg)
    {
        var NativeNested = kernel.ImportPluginFromObject(new NativeNeastedFirst(kernel));
        var res = await kernel.InvokeAsync(
            NativeNested["AnalysisAndSub"], // 注意异步方法Async后缀不需要加，否则找不到了
            new() { ["input"] = msg }
        );
        Console.WriteLine(res);
    }
}
