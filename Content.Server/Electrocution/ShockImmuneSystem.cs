using Content.Shared.Electrocution; 
using Content.Shared.Inventory;

namespace Content.Server.Electrocution
{
    public sealed class ElectrocutionImmuneSystem : EntitySystem //Goobstation
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShockImmuneComponent, ElectrocutionAttemptEvent>(OnElectrocutionAttempt);
            SubscribeLocalEvent<ShockImmuneComponent, InventoryRelayedEvent<ElectrocutionAttemptEvent>>((e, comp, ev) =>
                OnElectrocutionAttempt(e, comp, ev.Args));
        }

        private void OnElectrocutionAttempt(EntityUid uid, ShockImmuneComponent comp, ElectrocutionAttemptEvent args)
        {
            args.Cancel();
        }
    }
}