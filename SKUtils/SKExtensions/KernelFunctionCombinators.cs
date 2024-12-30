using System;
using Microsoft.SemanticKernel;

namespace SKUtils.SKExtensions;

public static class KernelFunctionCombinators
{
    /// <summary>
    /// 调用一个函数管道，按顺序运行每个函数，并将一个函数的输出作为下一个函数的第一个参数传递。
    /// </summary>
    /// <param name="functions">要调用的函数管道。</param>
    /// <param name="kernel">用于操作的内核。</param>
    /// <param name="arguments">参数。</param>
    /// <param name="cancellationToken">用于监视取消请求的取消令牌。</param>
    /// <returns>KernelFunction 运行结果</returns>
    public static Task<FunctionResult> InvokePipelineAsync(
        IEnumerable<KernelFunction> functions,
        Kernel kernel,
        KernelArguments arguments,
        CancellationToken cancellationToken
    ) => Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);

    /// <summary>
    /// 调用一个函数管道，按顺序运行每个函数，并将一个函数的输出作为命名参数传递给下一个函数。
    /// </summary>
    /// <param name="functions">要调用的函数序列，以及分配给函数调用结果的参数名称。</param>
    /// <param name="kernel">用于操作的内核。</param>
    /// <param name="arguments">参数。</param>
    /// <param name="cancellationToken">用于监视取消请求的取消令牌。</param>
    /// <returns>KernelFunction 运行结果</returns>
    public static Task<FunctionResult> InvokePipelineAsync(
        IEnumerable<(KernelFunction Function, string OutputVariable)> functions,
        Kernel kernel,
        KernelArguments arguments,
        CancellationToken cancellationToken
    ) => Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);

    /// <summary>
    /// 创建一个函数，其调用将依次调用每个提供的函数。
    /// </summary>
    /// <param name="functions">要调用的函数管道。</param>
    /// <param name="functionName">组合操作的名称。</param>
    /// <param name="description">组合操作的描述。</param>
    /// <returns>最后一个函数的结果。</returns>
    /// <remarks>
    /// 一个函数的结果将作为下一个函数的第一个参数传递。
    /// </remarks>
    public static KernelFunction Pipe(
        IEnumerable<KernelFunction> functions,
        string? functionName = null,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(functions);
        KernelFunction[] funcs = functions.ToArray();
        Array.ForEach(funcs, f => ArgumentNullException.ThrowIfNull(f));

        // 创建一个包含函数和输出变量名的元组数组。如果不是最后一个函数，获取下一个函数的第一个参数名
        var funcsAndVars = new (KernelFunction Function, string OutputVariable)[funcs.Length];
        for (int i = 0; i < funcs.Length; i++)
        {
            string p = "";
            if (i < funcs.Length - 1)
            {
                var parameters = funcs[i + 1].Metadata.Parameters;
                if (parameters.Count > 0)
                {
                    p = parameters[0].Name;
                }
            }
            // 将当前函数和下一个函数的第一个参数名存入元组数组
            funcsAndVars[i] = (funcs[i], p);
        }
        return Pipe(funcsAndVars, functionName, description);
    }

    /// <summary>
    /// 创建一个函数，其调用将依次调用每个提供的函数。
    /// </summary>
    /// <param name="functions">要调用的函数管道，以及分配给函数调用结果的参数名称。</param>
    /// <param name="functionName">组合操作的名称。</param>
    /// <param name="description">组合操作的描述。</param>
    /// <returns>最后一个函数的结果。</returns>
    /// <remarks>
    /// 一个函数的结果将作为下一个函数的第一个参数传递。
    /// </remarks>
    public static KernelFunction Pipe(
        IEnumerable<(KernelFunction Function, string OutputVariable)> functions,
        string? functionName = null,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(functions);

        (KernelFunction Function, string OutputVariable)[] arr = functions.ToArray();
        Array.ForEach(
            arr,
            f =>
            {
                ArgumentNullException.ThrowIfNull(f.Function);
                ArgumentNullException.ThrowIfNull(f.OutputVariable);
            }
        );
        // 使用 KernelFunctionFactory 创建一个新的 KernelFunction
        return KernelFunctionFactory.CreateFromMethod(
            async (Kernel kernel, KernelArguments arguments) =>
            {
                FunctionResult? result = null;
                // 遍历函数数组，依次调用每个函数。如果不是最后一个函数，将当前函数的结果存入参数集合中，作为下一个函数的输入
                for (int i = 0; i < arr.Length; i++)
                {
                    result = await arr[i]
                        .Function.InvokeAsync(kernel, arguments)
                        .ConfigureAwait(false);
                    if (i < arr.Length - 1)
                    {
                        arguments[arr[i].OutputVariable] = result.GetValue<object>();
                    }
                }

                return result;
            },
            functionName,
            description
        );
    }
}
