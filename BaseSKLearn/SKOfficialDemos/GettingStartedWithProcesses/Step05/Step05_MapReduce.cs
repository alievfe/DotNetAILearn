using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step05;

/// <summary>
/// 演示如何使用 <see cref="KernelProcessMap"/> 进行映射 - 归约操作。
/// </summary>
public class Step05_MapReduce
{
    /// <summary>
    /// 用于增加处理内容规模的因子。
    /// </summary>
    private const int ScaleFactor = 100;

    private readonly string _sourceContent;

    public Step05_MapReduce()
    {
        // 初始化测试内容
        StringBuilder content = new();

        for (int count = 0; count < ScaleFactor; ++count)
        {
            content.AppendLine(
                File.ReadAllText("./Resources/Grimms-The-King-of-the-Golden-Mountain.txt")
            );
            content.AppendLine(File.ReadAllText("./Resources/Grimms-The-Water-of-Life.txt"));
            content.AppendLine(File.ReadAllText("./Resources/Grimms-The-White-Snake.txt"));
        }

        this._sourceContent = content.ToString().ToUpperInvariant();
    }

    [Fact]
    public async Task RunMapReduceAsync()
    {
        // 定义处理流程
        KernelProcess process = SetupMapReduceProcess(nameof(RunMapReduceAsync), "Start");

        // 执行处理流程
        Kernel kernel = new();
        using LocalKernelProcessContext localProcess = await process.StartAsync(
            kernel,
            new KernelProcessEvent { Id = "Start", Data = this._sourceContent }
        );

        // 显示结果
        Dictionary<string, int> results =
            (Dictionary<string, int>?)kernel.Data[ResultStep.ResultKey] ?? [];
        foreach (var result in results)
        {
            Console.WriteLine($"{result.Key}: {result.Value}");
        }
    }

    private KernelProcess SetupMapReduceProcess(string processName, string inputEventId)
    {
        ProcessBuilder process = new(processName);

        ProcessStepBuilder chunkStep = process.AddStepFromType<ChunkStep>();
        process.OnInputEvent(inputEventId).SendEventTo(new ProcessFunctionTargetBuilder(chunkStep));

        ProcessMapBuilder mapStep = process.AddMapStepFromType<CountStep>();
        chunkStep.OnEvent(ChunkStep.EventId).SendEventTo(new ProcessFunctionTargetBuilder(mapStep));

        ProcessStepBuilder resultStep = process.AddStepFromType<ResultStep>();
        mapStep
            .OnEvent(CountStep.EventId)
            .SendEventTo(new ProcessFunctionTargetBuilder(resultStep));

        return process.Build();
    }

    // 将内容分割成块的步骤
    private sealed class ChunkStep : KernelProcessStep
    {
        public const string EventId = "ChunkComplete";

        [KernelFunction]
        public async ValueTask ChunkAsync(KernelProcessStepContext context, string content)
        {
            // 计算每个块的大小，根据内容长度和处理器核心数进行分割
            int chunkSize = content.Length / Environment.ProcessorCount;
            string[] chunks = ChunkContent(content, chunkSize).ToArray();

            await context.EmitEventAsync(new() { Id = EventId, Data = chunks });
        }

        // 用于将内容分割成指定大小的块
        private IEnumerable<string> ChunkContent(string content, int chunkSize)
        {
            for (int index = 0; index < content.Length; index += chunkSize)
            {
                yield return content.Substring(index, Math.Min(chunkSize, content.Length - index));
            }
        }
    }

    // 统计块中单词数量的步骤
    private sealed class CountStep : KernelProcessStep
    {
        public const string EventId = "CountComplete";

        // 输入为string chunk，每次迭代处理一个文本块
        [KernelFunction]
        public async ValueTask ComputeAsync(KernelProcessStepContext context, string chunk)
        {
            Dictionary<string, int> counts = [];

            // 将块内容按指定的分隔符分割成单词数组
            string[] words = chunk.Split(
                [" ", "\n", "\r", ".", ",", "’"],
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (string word in words)
            {
                if (s_notInteresting.Contains(word))
                {
                    continue;
                }
                // 统计增加
                counts.TryGetValue(word.Trim(), out int count);
                counts[word] = ++count;
            }
            Console.WriteLine("数量："+ JsonSerializer.Serialize(counts));
            Console.WriteLine();
            Console.WriteLine();
            await context.EmitEventAsync(new() { Id = EventId, Data = counts });
        }
    }

    // 合并结果的步骤
    private sealed class ResultStep : KernelProcessStep
    {
        public const string ResultKey = "WordCount";

        [KernelFunction]
        public async ValueTask ComputeAsync(
            KernelProcessStepContext context,
            IList<Dictionary<string, int>> results,
            Kernel kernel
        )
        {
            Dictionary<string, int> totals = [];
            // 遍历每个统计结果字典中的键值对合并单词数量
            foreach (Dictionary<string, int> result in results)
            {
                foreach (KeyValuePair<string, int> pair in result)
                {
                    totals.TryGetValue(pair.Key, out int count);
                    totals[pair.Key] = count + pair.Value;
                }
            }
            // 对合并结果字典按单词数量降序排序
            var sorted = from kvp in totals orderby kvp.Value descending select kvp;
            // 取排序后的前 10 个结果，并转换为字典，存储到 Kernel 的 Data 属性中
            kernel.Data[ResultKey] = sorted.Take(10).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    // 要从内容中移除的无意义单词
    private static readonly HashSet<string> s_notInteresting =
    [
        "A",
        "ALL",
        "AN",
        "AND",
        "AS",
        "AT",
        "BE",
        "BEFORE",
        "BUT",
        "BY",
        "CAME",
        "COULD",
        "FOR",
        "GO",
        "HAD",
        "HAVE",
        "HE",
        "HER",
        "HIM",
        "HIMSELF",
        "HIS",
        "HOW",
        "I",
        "IF",
        "IN",
        "INTO",
        "IS",
        "IT",
        "ME",
        "MUST",
        "MY",
        "NO",
        "NOT",
        "NOW",
        "OF",
        "ON",
        "ONCE",
        "ONE",
        "ONLY",
        "OUT",
        "S",
        "SAID",
        "SAW",
        "SET",
        "SHE",
        "SHOULD",
        "SO",
        "THAT",
        "THE",
        "THEM",
        "THEN",
        "THEIR",
        "THERE",
        "THEY",
        "THIS",
        "TO",
        "VERY",
        "WAS",
        "WENT",
        "WERE",
        "WHAT",
        "WHEN",
        "WHO",
        "WILL",
        "WITH",
        "WOULD",
        "UP",
        "UPON",
        "YOU",
    ];
}
