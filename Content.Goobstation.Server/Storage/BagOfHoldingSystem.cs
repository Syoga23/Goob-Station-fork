using Content.Goobstation.Server.Storage;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Content.Goobstation.Shared.Storage;
using Content.Server.Popups;

namespace Content.Goobstation.Server.Storage;

public sealed class BagOfHoldingSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BagOfHoldingComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BagOfHoldingComponent, BagOfHoldingConfirmMessage>(OnConfirmMessage);
    }

    private void OnInteractUsing(EntityUid uid, BagOfHoldingComponent component, InteractUsingEvent args)
    {
        if (!TryComp<BagOfHoldingComponent>(args.Used, out _))
            return;

        args.Handled = true;

        _popup.PopupEntity(
            Loc.GetString("bag-of-holding-warning"),
            uid,
            args.User,
            PopupType.LargeCaution
        );

        component.PendingUser = args.User;

        _uiSystem.TryOpenUi(uid, BagOfHoldingUiKey.Key, args.User);
    }

    private void OnConfirmMessage(EntityUid uid, BagOfHoldingComponent component, BagOfHoldingConfirmMessage msg)
    {
        var user = component.PendingUser;
        if (!msg.Confirmed)
        {
            component.PendingUser = null;
            return;
        }
        if (user == null || !Exists(user.Value))
            return;
        var transform = Transform(user.Value);
        Spawn("Singularity", transform.Coordinates);
        _popup.PopupEntity(
            Loc.GetString("bag-of-holding-singularity-spawn"),
            user.Value,
            PopupType.LargeCaution
        );
        component.PendingUser = null;
    }
}