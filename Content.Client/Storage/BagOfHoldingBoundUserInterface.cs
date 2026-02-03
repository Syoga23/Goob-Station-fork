using Content.Goobstation.Shared.Storage;
using Jetbrains.annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Storage;

[UsedImplicitly]
public sealed class BagOfHoldingBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BagOfHoldingConfirmWindow? _window;
    public BagOfHoldingBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<BagOfHoldingConfirmWindow>();

        _window.OnConfirmed += () =>
        {
            SendMessage(new BagOfHoldingConfirmMessage(true));
            Close();
        };

        _window.OnCancelled += () =>
        {
            SendMessage(new BagOfHoldingConfirmMessage(false));
            Close();
        };
    }
}