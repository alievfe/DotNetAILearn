using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.Plugins.Finefood;

public class FinefoodPlugin
{
    [KernelFunction, Description("根据城市获取美食推荐")]
    public string GetFinefoodList([Description("城市名")] string city)
    {
        return "石头，岩石，石块等";
    }
}