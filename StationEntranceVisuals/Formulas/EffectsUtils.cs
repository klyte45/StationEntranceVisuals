using Game.Simulation;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public static class EffectsUtils
{
    private static PlanetarySystem planetarySystem;

    public static float GetEffectiveLuminance(Entity _)
    {
        planetarySystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlanetarySystem>();
        return planetarySystem.NightLight.isValid && planetarySystem.NightLight.additionalData.intensity > .5f ? 7 : 0;
    }
}