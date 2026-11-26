#nullable disable
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModuleCrafterEnhanceVisuals
{
    private sealed class ResultAnimation
    {
        public GameItem Target;
        public ModuleCrafterEnhanceResultKind Kind;
        public double StartedAt;
    }

    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    private static readonly Dictionary<GameItem, Func<GameItem, List<ItemSpriteModifier>>> PreviousProviders = new Dictionary<GameItem, Func<GameItem, List<ItemSpriteModifier>>>();
    private static readonly Dictionary<GameItem, ResultAnimation> Animations = new Dictionary<GameItem, ResultAnimation>();
    private static bool _running;
    private static string _visualErrorSignature;

    public static void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        ScheduleTick();
    }

    public static void Attach(GameItem item)
    {
        if (item == null || PreviousProviders.ContainsKey(item))
        {
            return;
        }

        Func<GameItem, List<ItemSpriteModifier>> previous = item.getItemSpriteModifierForItemFunc;
        PreviousProviders[item] = previous;
        item.getItemSpriteModifierForItemFunc = target => GetModifiers(target, previous);
    }

    public static void Detach(GameItem item)
    {
        if (item == null || !PreviousProviders.TryGetValue(item, out Func<GameItem, List<ItemSpriteModifier>> previous))
        {
            return;
        }

        item.getItemSpriteModifierForItemFunc = previous;
        PreviousProviders.Remove(item);
    }

    public static void Play(GameItem machine, GameItem target, ModuleCrafterEnhanceResult result)
    {
        if (machine == null || target == null)
        {
            return;
        }

        Attach(target);
        if (Animations.TryGetValue(machine, out ResultAnimation previous) && previous.Target != null)
        {
            RefreshModifiers(previous.Target);
        }

        Animations[machine] = new ResultAnimation
        {
            Target = target,
            Kind = result.Kind,
            StartedAt = Time.unscaledTimeAsDouble
        };
        RefreshModifiers(target);
        Start();
    }

    private static void RefreshModifiers(GameItem item)
    {
        try
        {
            RefreshModifiersCore(item);
        }
        catch (Exception ex)
        {
            ReportVisualError(ex);
        }
    }

    private static void RefreshModifiersCore(GameItem item)
    {
        if (item == null)
        {
            return;
        }

        if (item is GameItemElement element)
        {
            // Scene transitions can destroy individual UI objects before their GameItem is removed.
            if (element.background == null || element.background.image == null ||
                element.fakeBackground == null || element.fakeBackground.image == null ||
                element.middle == null || element.fakeMiddle == null)
            {
                return;
            }

            // ValidateSelect(false) still calls UpdateRectTransform(), which snaps fakeMiddle
            // back to the inventory position while it is being dragged. Refresh only visuals.
            bool realVisible = element.background.image.enabled;
            bool dragVisible = element.fakeBackground.image.enabled;
            Vector4 dragColor = element.fakeBackground.image.color;
            element.HandleInnateModSprite();
            SpriteAnimationHelper.SyncSpriteAnimation(element);

            // HandleInnateModSprite enables real overlays and disables fake ones. Restore the
            // pre-refresh visibility so the original stays hidden and the dragged copy stays visible.
            IEnumerable<TreeNodeRender> activeModifiers = element.GetActiveInnateSpriteMods();
            if (activeModifiers != null)
            {
                foreach (TreeNodeRender modifier in activeModifiers)
                {
                    if (modifier != null && modifier.image != null)
                    {
                        modifier.image.enabled = realVisible;
                    }
                }
            }

            if (dragVisible)
            {
                // Re-enable fake overlays without forcing a drag-end or changing its position.
                element.ToggleFakeBackground(true, dragColor);
            }
        }
        else
        {
            item.Validate();
        }
    }

    private static void ReportVisualError(Exception ex)
    {
        string signature = ex.GetType().FullName + "|" + ex.StackTrace;
        if (signature == _visualErrorSignature)
        {
            return;
        }

        _visualErrorSignature = signature;
        ModuleCrafterMod.LogMsg("Enhancement visual refresh failed; identical errors will be suppressed. " + ex);
    }

    public static void Shutdown()
    {
        _running = false;
        _visualErrorSignature = null;
        try
        {
            ModHelper.StopAllCoroutines(ModuleCrafterMod.ManifestId);
        }
        catch
        {
        }

        Animations.Clear();
        foreach (KeyValuePair<GameItem, Func<GameItem, List<ItemSpriteModifier>>> pair in new List<KeyValuePair<GameItem, Func<GameItem, List<ItemSpriteModifier>>>>(PreviousProviders))
        {
            if (pair.Key != null)
            {
                pair.Key.getItemSpriteModifierForItemFunc = pair.Value;
            }
        }

        PreviousProviders.Clear();
        foreach (Sprite sprite in Sprites.Values)
        {
            if (sprite != null)
            {
                UnityEngine.Object.Destroy(sprite);
            }
        }

        Sprites.Clear();
    }

    private static List<ItemSpriteModifier> GetModifiers(GameItem item, Func<GameItem, List<ItemSpriteModifier>> previous)
    {
        List<ItemSpriteModifier> modifiers = previous?.Invoke(item) ?? new List<ItemSpriteModifier>();
        int level = ModuleCrafterEnhanceState.GetLevel(item);
        if (level > 0)
        {
            if (level >= 9)
            {
                string blink = level == 12 && ((int)(Time.unscaledTimeAsDouble * 1000.0 / ModuleCrafterEnhanceRules.MaxLevelFrameMilliseconds) % 2 == 1) ? "_b" : "";
                bool specialized = ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind)
                    && ModuleCrafterExpansion.IsSpecialized(kind);
                Sprite overlay = specialized
                    ? LoadSprite(SpecializedOverlayKey(kind, level, blink))
                    : LoadSprite("module_enhance_overlay_plus_" + level.ToString("00") + "_" + ShapeSuffix(item) + blink);
                if (overlay != null)
                {
                    modifiers.Add(new ItemSpriteModifier(overlay, Vector2.zero, priority: 200));
                }
            }
            else
            {
                AddBadge(item, level, modifiers);
            }
        }

        ResultAnimation animation = FindAnimation(item);
        if (animation != null)
        {
            int frame = (int)((Time.unscaledTimeAsDouble - animation.StartedAt) * 1000.0 / ModuleCrafterEnhanceRules.ResultFrameMilliseconds);
            int count = ModuleCrafterEnhanceRules.ResultFrameCount(animation.Kind);
            if (frame >= 0 && frame < count)
            {
                Sprite fx = LoadSprite(FxPrefix(animation.Kind) + "_" + frame.ToString("00"));
                if (fx != null)
                {
                    Vector2 position = new Vector2((BaseWidth(item) - fx.rect.width) / 2f, (fx.rect.height - BaseHeight(item)) / 2f);
                    modifiers.Add(new ItemSpriteModifier(fx, position, priority: 300));
                }
            }
        }

        return modifiers;
    }

    private static ResultAnimation FindAnimation(GameItem item)
    {
        foreach (ResultAnimation animation in Animations.Values)
        {
            if (animation.Target == item)
            {
                double duration = ModuleCrafterEnhanceRules.ResultFrameCount(animation.Kind) * ModuleCrafterEnhanceRules.ResultFrameMilliseconds / 1000.0;
                return Time.unscaledTimeAsDouble - animation.StartedAt <= duration ? animation : null;
            }
        }

        return null;
    }

    private static void ScheduleTick()
    {
        if (!_running)
        {
            return;
        }

        if (ModHelper.Delay(ModuleCrafterMod.ManifestId, 0.05f, Tick) == null)
        {
            // OnEnable runs before LoadedMod.IsEnabled is set. Allow a later retry.
            _running = false;
        }
    }

    private static void Tick()
    {
        try
        {
            if (!_running)
            {
                return;
            }

            double now = Time.unscaledTimeAsDouble;
            List<GameItem> expired = new List<GameItem>();
            foreach (KeyValuePair<GameItem, ResultAnimation> pair in Animations)
            {
                double duration = ModuleCrafterEnhanceRules.ResultFrameCount(pair.Value.Kind) * ModuleCrafterEnhanceRules.ResultFrameMilliseconds / 1000.0;
                if (pair.Value.Target == null || now - pair.Value.StartedAt > duration)
                {
                    expired.Add(pair.Key);
                }
            }

            foreach (GameItem machine in expired)
            {
                ResultAnimation old = Animations[machine];
                RefreshModifiers(old.Target);
                Animations.Remove(machine);
            }

            foreach (GameItem item in new List<GameItem>(PreviousProviders.Keys))
            {
                if (item == null)
                {
                    continue;
                }

                if (ModuleCrafterEnhanceState.GetLevel(item) == 12 || FindAnimation(item) != null)
                {
                    RefreshModifiers(item);
                }
            }
        }
        catch (Exception ex)
        {
            ReportVisualError(ex);
        }
        finally
        {
            // A single stale UI object must not permanently stop +12 blinking or result frames.
            ScheduleTick();
        }
    }

    private static Sprite LoadSprite(string key)
    {
        if (Sprites.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        LoadedMod loaded = null;
        if (ModLoader.Mods != null)
        {
            foreach (LoadedMod mod in ModLoader.Mods)
            {
                if (mod?.Manifest != null && mod.Manifest.ID == ModuleCrafterMod.ManifestId)
                {
                    loaded = mod;
                    break;
                }
            }
        }

        if (loaded == null || !loaded.Textures.TryGetValue(key, out Texture2D texture) || texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = key;
        Sprites[key] = sprite;
        return sprite;
    }

    private static void AddBadge(GameItem item, int level, List<ItemSpriteModifier> modifiers)
    {
        Sprite badge = LoadSprite("module_enhance_badge_plus_" + level.ToString("00"));
        if (badge != null)
        {
            modifiers.Add(new ItemSpriteModifier(badge, new Vector2(BaseWidth(item) - badge.rect.width - 4f, -4f), priority: 210));
        }
    }

    private static string SpecializedOverlayKey(ModuleCrafterChipKind kind, int level, string blink)
    {
        const string chipPrefix = "module_crafter_chip_";
        string chipId = ModuleCrafterIds.Chip(kind);
        string kindKey = chipId.StartsWith(chipPrefix, StringComparison.Ordinal)
            ? chipId.Substring(chipPrefix.Length)
            : kind.ToString().ToLowerInvariant();
        return kindKey + "_plus_" + level.ToString("00") + blink;
    }

    private static int GridSize()
    {
        return UISettings.current != null ? Math.Max(1, UISettings.current.gridSize) : 16;
    }

    private static string FxPrefix(ModuleCrafterEnhanceResultKind kind)
    {
        return kind switch
        {
            ModuleCrafterEnhanceResultKind.Success => "module_enhance_fx_success",
            ModuleCrafterEnhanceResultKind.Stay => "module_enhance_fx_fail_stay",
            ModuleCrafterEnhanceResultKind.Drop => "module_enhance_fx_fail_drop",
            ModuleCrafterEnhanceResultKind.Ruined => "module_enhance_fx_fail_break",
            ModuleCrafterEnhanceResultKind.SafeguardReset => "module_enhance_fx_fail_clip",
            _ => ""
        };
    }

    private static string ShapeSuffix(GameItem item)
    {
        string path = item.spritePath ?? "";
        if (path.EndsWith("_c_red", StringComparison.Ordinal)) return "cut_red";
        if (path.EndsWith("_c", StringComparison.Ordinal)) return "cut";
        if (path.EndsWith("_l", StringComparison.Ordinal)) return "l";
        if (path.EndsWith("_t", StringComparison.Ordinal)) return "t";
        if (path.EndsWith("_s", StringComparison.Ordinal)) return "s";
        if (path.EndsWith("_h", StringComparison.Ordinal)) return "h";
        if (item.identifier == ModuleCrafterIds.Overclock || item.identifier == ModuleCrafterIds.Eco || item.identifier == ModuleCrafterIds.Fineness) return "square_2";
        if (item.identifier == ModuleCrafterIds.Streamlining || item.identifier == ModuleCrafterIds.NeuralUncapped) return "square_3";
        if (item.identifier == ModuleCrafterIds.NeuralCapped) return "square_black_4";
        return "square";
    }

    private static int BaseWidth(GameItem item)
    {
        if (ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind)
            && ModuleCrafterExpansion.IsSpecialized(kind))
        {
            return Math.Max(1, item.shape.width) * GridSize();
        }

        // Vanilla atlas: L is 48x32; T is 32x48.
        return ShapeSuffix(item) == "l" ? 48 : 32;
    }

    private static int BaseHeight(GameItem item)
    {
        if (ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind)
            && ModuleCrafterExpansion.IsSpecialized(kind))
        {
            return Math.Max(1, item.shape.height) * GridSize();
        }

        string shape = ShapeSuffix(item);
        return shape == "t" || shape == "s" || shape == "h" ? 48 : 32;
    }
}
