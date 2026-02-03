using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Storage;

[Serializable, NetSerializable]
public enum BagOfHoldingUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class BagOfHoldingConfirmMessage : BoundUserInterfaceMessage
{
    public bool Confirmed;

    public BagOfHoldingConfirmMessage(bool confirmed)
    {
        Confirmed = confirmed;
    }
}