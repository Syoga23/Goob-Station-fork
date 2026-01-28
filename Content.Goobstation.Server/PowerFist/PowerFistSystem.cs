using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared._Shitcode.Weapons.PowerFist;
using Content.Shared.Damage;
using Robust.Server.Audio;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Atmos.Components;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Content.Server.Stunnable;
using Content.Goobstation.Common.Standing;

namespace Content.Goobstation.Server.PowerFist;

public sealed partial class PowerFistSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem Container = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerFistComponent, GetVerbsEvent<Verb>>(OnGetVerb);
        SubscribeLocalEvent<PowerFistComponent, MeleeHitEvent>(OnHit);
    }

    private void OnGetVerb(EntityUid uid, PowerFistComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;

        var ordered = new[]
        {
            PowerFistPowerMode.Low,
            PowerFistPowerMode.Medium,
            PowerFistPowerMode.High
        };

        int priority = 3;

        foreach (var mode in ordered)
        {
            args.Verbs.Add(new Verb
            {
                Priority = priority--,
                Category = VerbCategory.PowerLevel,
                Text = mode.ToString(),
                Disabled = component.Mode == mode,
                DoContactInteraction = true,
                Act = () =>
                {
                    component.Mode = mode;
                    Dirty(uid, component);
                    _popup.PopupEntity($"Power mode set to {mode}.", uid, args.User);
                }
            });
        }
    }

    private Entity<GasTankComponent>? GetTank(EntityUid uid, PowerFistComponent comp)
    {
        if (!Container.TryGetContainer(uid, PowerFistComponent.TankSlotId, out var container))
            return null;

        if (container is not ContainerSlot slot)
            return null;

        if (slot.ContainedEntity is not EntityUid tankUid)
            return null;

        return TryComp<GasTankComponent>(tankUid, out var gas)
            ? (tankUid, gas)
            : null;
    }

    private void OnHit(EntityUid uid, PowerFistComponent comp, MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var user = args.User;
        var target = args.HitEntities[0];

        var tank = GetTank(uid, comp);
        if (tank == null)
        {
            _popup.PopupEntity("The powerfist can't operate without a gas tank!", uid, user);
            return;
        }

        var (tankUid, tankComp) = tank.Value;
        var mode = comp.Mode;

        var gasNeeded = comp.GasPerPunch * mode.GasMultiplier();

        var removed = _gasTank.RemoveAir((tankUid, tankComp), gasNeeded);

        var environment = _atmos.GetContainingMixture(uid, false, true);

        if (removed == null || removed.TotalMoles < gasNeeded * 0.99f)
        {
            _popup.PopupEntity("The piston lets out a weak hiss — not enough gas!", uid, user);

            _damageable.TryChangeDamage(
                target,
                new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), mode.Damage() / 2)
            );

            if (environment != null && removed != null)
                _atmos.Merge(environment, removed);

            return;
        }

        _damageable.TryChangeDamage(
            target,
            new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), mode.Damage())
        );

        var attackerPos = Transform(uid).WorldPosition;
        var targetPos = Transform(target).WorldPosition;

        var direction = targetPos - attackerPos;
        var throwVector = direction.Normalized() * comp.ThrowDistance;
        var throwSpeed = comp.ThrowSpeed;
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1.5), refresh: false, DropHeldItemsBehavior.NoDrop, standOnRemoval: true);
        _throwing.TryThrow(target, throwVector, throwSpeed, user, unanchor: false);

        if (environment != null)
            _atmos.Merge(environment, removed);
    }
}
