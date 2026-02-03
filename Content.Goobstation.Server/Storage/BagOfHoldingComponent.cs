using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Storage;

[RegisterComponent]
public sealed partial class BagOfHoldingComponent : Component
{
    [DataField]
    public EntityUid? PendingUser;
}
