using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// <see cref="ScriptedUserInputStep"/> 包含一系列交互步骤，使流程因欺诈检测失败而无法通过所有步骤
/// </summary>
public sealed class UserInputFraudFailureInteractionStep : ScriptedUserInputStep
{
    public override void PopulateUserInputs(UserInputState state)
    {
        state.UserInputs.Add("I would like to open an account");
        state.UserInputs.Add("My name is John Contoso, dob 02/03/1990");
        state.UserInputs.Add("I live in Washington and my phone number es 222-222-1234");
        state.UserInputs.Add("My userId is 123-456-7890");
        state.UserInputs.Add("My email is john.contoso@contoso.com, what else do you need?");
    }
}
