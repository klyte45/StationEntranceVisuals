using Game.Effects;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public static class EffectsUtils
{
    private static EffectFlagSystem effectFlagSystem;

    public static float GetEffectiveLuminance(Entity _)
    {
        effectFlagSystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EffectFlagSystem>();
            return effectFlagSystem.GetData().m_IsNightTime ? 7 : 0;
    }
}