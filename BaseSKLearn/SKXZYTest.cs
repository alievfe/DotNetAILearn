using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BaseSKLearn.Plugins.MathPlg;
using HandlebarsDotNet;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;

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

    /// <summary>
    /// 计划测试
    /// 分步和 Handlebars 规划器仍可在语义内核中使用。 但是，建议对大多数任务使用函数调用，因为它更强大且更易于使用。 后续版本的语义内核中将弃用分步和 Handlebars 规划器。
    /// </summary>
    /// <param name="msg">
    /// 案例输入：1.小明有7个冰淇淋，我有2个冰淇淋，他比我多几个冰淇淋？
    /// 2.小明有7个冰淇淋，我有2个冰淇淋，我们一共有几个冰淇淋？
    /// </param>
    /// <returns></returns> <summary>
    public async Task PlanTest(string msg)
    {
        var planner = new HandlebarsPlanner(
            new HandlebarsPlannerOptions()
            {
                // 如果您想使用循环进行测试，而不管模型选择如何，请更改此设置。
                AllowLoops = true,
            }
        );
        kernel.ImportPluginFromDefaultPathPromptDirectory("Calculate");
        var plan = await planner.CreatePlanAsync(kernel, msg);
        Console.WriteLine("Plan:\n");
        Console.WriteLine(JsonSerializer.Serialize(plan));

        var res = await plan.InvokeAsync(kernel);
        System.Console.WriteLine(res);
    }
}
