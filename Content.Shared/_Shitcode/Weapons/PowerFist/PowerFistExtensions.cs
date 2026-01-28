namespace Content.Shared._Shitcode.Weapons.PowerFist;

public static class PowerFistExtensions
{
    public static int Damage(this PowerFistPowerMode mode) =>
        mode switch
        {
            PowerFistPowerMode.Low => 20,
            PowerFistPowerMode.Medium => 40,
            PowerFistPowerMode.High => 60,
            _ => 20
        };

    public static float GasMultiplier(this PowerFistPowerMode mode) =>
        mode switch
        {
            PowerFistPowerMode.Low => 1f,
            PowerFistPowerMode.Medium => 2f,
            PowerFistPowerMode.High => 3f,
            _ => 1f
        };
}
