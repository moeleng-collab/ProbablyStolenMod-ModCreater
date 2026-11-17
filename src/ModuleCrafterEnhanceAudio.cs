#nullable disable
using System;

public static class ModuleCrafterEnhanceAudio
{
    public static void Play(ModuleCrafterEnhanceResultKind kind)
    {
        string key = ModuleCrafterEnhanceRules.ResultSoundKey(kind);
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        try
        {
            if (!ModHelper.PlaySound(ModuleCrafterMod.ManifestId, key))
            {
                ModuleCrafterMod.LogMsg("Enhancement sound could not play: " + key + ".");
            }
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Enhancement sound failed: " + key + ": " + ex.Message);
        }
    }
}
