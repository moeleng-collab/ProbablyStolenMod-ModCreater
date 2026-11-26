#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterExpansionStock
{
    public const int LegalChipJacksonChance = 35;
    public const int ComponentMaterialSellerChance = 50;
    public const int NightmareThiefChance = 8;

    private static readonly HashSet<Dialogue> WrappedDialogues = new HashSet<Dialogue>();

    public static void Reset()
    {
        WrappedDialogues.Clear();
    }

    private static readonly ModuleCrafterChipKind[] LegalSpecialKinds =
    {
        ModuleCrafterChipKind.FarmNutrifruit,
        ModuleCrafterChipKind.FarmDreamCap,
        ModuleCrafterChipKind.FarmKottonLoom,
        ModuleCrafterChipKind.FarmFillerweed,
        ModuleCrafterChipKind.FarmHydroreed,
        ModuleCrafterChipKind.EvaporatorSolventRecovery,
        ModuleCrafterChipKind.ChemEquipment,
        ModuleCrafterChipKind.FurnaceJunk,
        ModuleCrafterChipKind.PrinterMetal,
        ModuleCrafterChipKind.PrinterMeleeBatch,
        ModuleCrafterChipKind.DnaStabilizer,
        ModuleCrafterChipKind.DnaPositiveFilter,
        ModuleCrafterChipKind.DnaNegativeFilter,
        ModuleCrafterChipKind.FurnaceBlast,
        ModuleCrafterChipKind.FarmSmuggler,
        ModuleCrafterChipKind.PrinterCnc,
        ModuleCrafterChipKind.DnaPrecision,
        ModuleCrafterChipKind.AlarmTransmitter
    };

    public static int PlaceJacksonStock()
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        int added = 0;
        if (ModuleCrafterRules.RollStock(LegalChipJacksonChance, NextExclusive))
        {
            ModuleCrafterChipKind kind = LegalSpecialKinds[NextExclusive(LegalSpecialKinds.Length)];
            added += AddCopies(ModuleCrafterIds.Chip(kind), 1, stolen: false);
        }

        added += AddCopies(ModuleCrafterExpansion.UniversalComponent, 2, stolen: false);
        return added;
    }

    public static int PlaceRetiredStock()
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        List<ModuleCrafterChipKind> pool = new List<ModuleCrafterChipKind>(LegalSpecialKinds);
        int added = 0;
        for (int i = 0; i < 2 && pool.Count > 0; i++)
        {
            int index = NextExclusive(pool.Count);
            ModuleCrafterChipKind kind = pool[index];
            pool.RemoveAt(index);
            added += AddCopies(ModuleCrafterIds.Chip(kind), 1, stolen: false);
        }

        added += AddCopies(ModuleCrafterExpansion.UniversalComponent, 4, stolen: false);
        added += ModuleCrafterNetworkUpgrades.PlaceLegalOrder();
        return added;
    }

    public static void OnGenerateCustomerLate(StoreClientManager manager)
    {
        if (manager?.clientStack == null)
        {
            return;
        }

        foreach (StoreClient client in manager.clientStack)
        {
            if (client == null)
            {
                continue;
            }

            try
            {
                switch (client.identifier)
                {
                    case "material_surplus_client":
                        WrapFinalDialogue(client, () =>
                        {
                            if (ModuleCrafterRules.RollStock(ComponentMaterialSellerChance, NextExclusive))
                            {
                                AddCopies(ModuleCrafterExpansion.UniversalComponent, 2, stolen: false);
                            }
                        });
                        break;
                    case "thief":
                        WrapFinalDialogue(client, () =>
                        {
                            if (ModuleCrafterRules.RollStock(ModuleCrafterRules.NeuralCappedThiefChance, NextExclusive))
                            {
                                AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralCapped), 1, stolen: true);
                            }

                            if (ModuleCrafterRules.RollStock(NightmareThiefChance, NextExclusive))
                            {
                                AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.FarmNightmareCap), 1, stolen: true);
                            }

                            ModuleCrafterNetworkUpgrades.PlaceBlackMarketOrder();
                        });
                        break;
                    case "hypertech_smuggler":
                        WrapFinalDialogue(client, () =>
                        {
                            if (ModuleCrafterRules.RollStock(ModuleCrafterRules.NeuralUncappedSmugglerChance, NextExclusive))
                            {
                                AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralUncapped), 1, stolen: false);
                            }

                            ModuleCrafterNetworkUpgrades.PlaceBlackMarketOrder();
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                ModuleCrafterMod.LogMsg("Expansion stock seller hook failed for " + client.identifier + ": " + ex.Message);
            }
        }
    }

    private static void WrapFinalDialogue(StoreClient client, Action append)
    {
        Dialogue dialogue = client.mainDialogue;
        if (dialogue == null)
        {
            return;
        }

        HashSet<Dialogue> visited = new HashSet<Dialogue>();
        while (dialogue.nextDialogue != null && visited.Add(dialogue))
        {
            dialogue = dialogue.nextDialogue;
        }

        if (!WrappedDialogues.Add(dialogue))
        {
            return;
        }

        Action original = dialogue.endAction;
        dialogue.endAction = delegate
        {
            original?.Invoke();
            if (PlayerStore.Instance != null)
            {
                append();
            }
        };
    }

    public static ModuleCrafterChipKind RandomLegalSpecialKind()
    {
        return LegalSpecialKinds[NextExclusive(LegalSpecialKinds.Length)];
    }

    private static int AddCopies(string itemId, int count, bool stolen)
    {
        int added = 0;
        for (int i = 0; i < count; i++)
        {
            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(itemId, isOwend: false, isStolen: stolen))
            {
                added++;
            }
        }

        return added;
    }

    private static int NextExclusive(int exclusive)
    {
        return RNG.GetRandomIntExculsive(0, exclusive);
    }
}
