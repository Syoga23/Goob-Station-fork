using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Weapons.PowerFist;

[RegisterComponent, NetworkedComponent]
public sealed partial class PowerFistComponent : Component
{
    public const string TankSlotId = "gas_tank";
    [DataField]
    public float GasPerPunch = 0.05f;
    [DataField]
    public PowerFistPowerMode Mode = PowerFistPowerMode.Low;
    [DataField]
    public float ThrowSpeed = 25f;
    [DataField]
    public float ThrowDistance = 11f;


}

public enum PowerFistPowerMode
{
    Low,
    Medium,
    High
}
