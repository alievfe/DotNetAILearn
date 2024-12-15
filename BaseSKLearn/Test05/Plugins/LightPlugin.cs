using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.Plugins;

public class LightPlugin
{
    public bool IsOn { get; set; }

    [KernelFunction, Description("Gets the state of the light.")]
    public string GetState() => IsOn ? "On" : "Off";

    [KernelFunction, Description("Changes the state of the light.'")]
    public string ChangeState(bool newState)
    {
        IsOn = newState;
        var state = GetState();

        // Print the state to the console
        Console.WriteLine($"[Light is now {state}]");

        return state;
    }
}
