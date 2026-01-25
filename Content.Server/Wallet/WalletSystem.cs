using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.Wallet;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Shared.Storage;
using Content.Server.Storage.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Server.Wallet;

public sealed class WalletSystem : EntitySystem
{
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WalletComponent, EntInsertedIntoContainerMessage>(OnChanged);
        SubscribeLocalEvent<WalletComponent, EntRemovedFromContainerMessage>(OnChanged);
        SubscribeLocalEvent<WalletComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<WalletComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, WalletComponent comp, GotEquippedEvent args)
    {
        if (args.Slot != "idcard")
            return;

        UpdateWalletAccess(uid);
        UpdateWalletIdentity(uid);
    }

    private void OnUnequipped(EntityUid uid, WalletComponent comp, GotUnequippedEvent args)
    {
        if (args.Slot != "idcard")
            return;
    }

    private void OnChanged(EntityUid uid, WalletComponent comp, EntityEventArgs args)
    {
        UpdateWalletAccess(uid);
        UpdateWalletIdentity(uid);
    }

    private BaseContainer GetWalletContainer(EntityUid wallet)
    {
        return _container.GetContainer(wallet, "storagebase");
    }

    private EntityUid? GetPrimaryIdCard(EntityUid wallet)
    {
        var container = GetWalletContainer(wallet);

        var ids = container.ContainedEntities
            .Where(e => HasComp<IdCardComponent>(e))
            .ToList();

        if (ids.Count == 0)
            return null;

        return ids[0];
    }

    private void UpdateWalletAccess(EntityUid wallet)
    {
        var container = GetWalletContainer(wallet);

        var access = EnsureComp<AccessComponent>(wallet);
        access.Tags.Clear();

        foreach (var ent in container.ContainedEntities)
        {
            if (!TryComp(ent, out AccessComponent? acc))
                continue;

            foreach (var tag in acc.Tags)
                access.Tags.Add(tag);
        }

        Dirty(wallet, access);
    }

    private void UpdateWalletIdentity(EntityUid wallet)
    {
        var primary = GetPrimaryIdCard(wallet);
        var id = EnsureComp<IdCardComponent>(wallet);

        if (primary is null)
        {
            id.FullName = "";
            id.JobTitle = "";
            id.JobIcon = "JobIconNoId";

            _appearance.SetData(wallet, WalletVisuals.HasId, false);
            Dirty(wallet, id);
            return;
        }

        var src = Comp<IdCardComponent>(primary.Value);

        id.FullName = src.FullName ?? "";
        id.JobTitle = src.LocalizedJobTitle ?? src.JobTitle ?? "";
        id.JobIcon = src.JobIcon;

        var sprite = Comp<SpriteComponent>(primary.Value);
        var idState = sprite.LayerGetState(1);

        _appearance.SetData(wallet, WalletVisuals.HasId, true);
        _appearance.SetData(wallet, WalletVisuals.IdCardState, "default"); // background
        _appearance.SetData(wallet, WalletVisuals.JobIconState, idState);  // job icon

        Dirty(wallet, id);
    }
}


/*
public sealed class WalletSystem : EntitySystem
{
    [Dependency] private readonly StorageSystem _storage = default!;

    [Dependency] private readonly SharedContainerSystem _container = default!;
    private static readonly HashSet<string> CommandAccess = new()
    {
        "Captain",
        "HeadOfSecurity",
        "ChiefEngineer",
        "ChiefMedicalOfficer",
        "HeadOfPersonnel",
        "ResearchDirector"
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<WalletComponent, EntInsertedIntoContainerMessage>(OnChanged);
        SubscribeLocalEvent<WalletComponent, EntRemovedFromContainerMessage>(OnChanged);
        SubscribeLocalEvent<WalletComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<WalletComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, WalletComponent comp, GotEquippedEvent args)
    {
        if (args.Slot != "idcard")
            return;

        UpdateWalletAccess(uid);
        UpdateWalletIdentity(uid);
    }

    private void OnUnequipped(EntityUid uid, WalletComponent comp, GotUnequippedEvent args)
    {
        if (args.Slot != "idcard")
            return;
    }

    private void OnChanged(EntityUid uid, WalletComponent comp, EntityEventArgs args)
    {
        UpdateWalletAccess(uid);
        UpdateWalletIdentity(uid);
        
    }

    private BaseContainer GetWalletContainer(EntityUid wallet)
    {
        return _container.GetContainer(wallet, "storagebase");
    }

    private EntityUid? GetPrimaryIdCard(EntityUid wallet)
    {
    var container = GetWalletContainer(wallet);

    var ids = container.ContainedEntities
        .Where(e => HasComp<IdCardComponent>(e))
        .ToList();

    if (ids.Count == 0)
        return null;

ids.Sort((a, b) =>
{
    var posA = _storage.(wallet, a);
    var posB = _storage.GetItemSlotPosition(wallet, b);

    int cmpY = posA.Y.CompareTo(posB.Y);
    if (cmpY != 0)
        return cmpY;

    return posA.X.CompareTo(posB.X);
});

    var commandIds = ids
        .Where(id =>
        {
            var card = Comp<IdCardComponent>(id);
            return CommandAccess.Contains(card.JobTitle!);
        })
        .ToList();

    if (commandIds.Count == 0)
        return ids[0];

    commandIds.Sort((a, b) =>
    {
        int aAccess = TryComp(a, out AccessComponent? aAcc) ? aAcc.Tags.Count : 0;
        int bAccess = TryComp(b, out AccessComponent? bAcc) ? bAcc.Tags.Count : 0;

        return bAccess.CompareTo(aAccess);
    });

    return commandIds[0];
    }


    private void UpdateWalletAccess(EntityUid wallet)
    {
        var container = GetWalletContainer(wallet);

        var access = EnsureComp<AccessComponent>(wallet);
        access.Tags.Clear();

        foreach (var ent in container.ContainedEntities)
        {
            if (!TryComp(ent, out AccessComponent? acc))
                continue;

            foreach (var tag in acc.Tags)
                access.Tags.Add(tag);
        }

        Dirty(wallet, access);
    }

    private void UpdateWalletIdentity(EntityUid wallet)
    {
    var primary = GetPrimaryIdCard(wallet);

    var id = EnsureComp<IdCardComponent>(wallet);

    if (primary is null)
    {
        id.FullName = "";
        id.JobTitle = "";
        id.JobIcon = "JobIconNoId";
        Dirty(wallet, id);
        return;
    }

    var src = Comp<IdCardComponent>(primary.Value);

    id.FullName = src.FullName ?? "";
    id.JobTitle = src.LocalizedJobTitle ?? src.JobTitle ?? "";
    id.JobIcon = src.JobIcon;

    Dirty(wallet, id);
    }

}
*/