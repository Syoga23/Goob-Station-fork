using Content.Shared.Wallet;
using Robust.Client.GameObjects;

namespace Content.Client.Wallet;

public sealed class WalletAppearanceSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WalletComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, WalletComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp(uid, out SpriteComponent? spriteComp))
            return;

        _appearance.TryGetData(uid, WalletVisuals.HasId, out bool hasId, args.Component);

        _sprite.LayerSetVisible(uid, "closed", !hasId);
        _sprite.LayerSetVisible(uid, "opened", hasId);

        _sprite.LayerSetVisible(uid, "idcard", hasId);
        _sprite.LayerSetVisible(uid, "jobicon", hasId);

        if (!hasId)
            return;

        if (_appearance.TryGetData(uid, WalletVisuals.IdCardState, out string? idState, args.Component))
            _sprite.LayerSetState(uid, "idcard", idState);

        if (_appearance.TryGetData(uid, WalletVisuals.JobIconState, out string? jobState, args.Component))
            _sprite.LayerSetState(uid, "jobicon", jobState);
    }
}
