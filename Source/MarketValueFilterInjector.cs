using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Object = System.Object;

namespace ZhentarTweaks
{
	static class MarketValueFilterInjector
	{
		abstract class BillDetour : Bill
		{
			[DetourConstructorHarmonyPostfix(typeof(RimWorld.Bill))]
			public static void Bill(Bill __instance, Verse.RecipeDef recipe)
			{
				__instance.ingredientFilter = new MarketValueFilter();
				__instance.ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
			}

			[DetourMemberHarmonyPostfix]
			public static void ExposeData(Bill __instance)
			{
				var ingredientFiltertmp = __instance.ingredientFilter;
				if (Scribe.mode == LoadSaveMode.LoadingVars && ingredientFiltertmp.GetType() != typeof(MarketValueFilter))
				{
					__instance.ingredientFilter = new MarketValueFilter();
					if (ingredientFiltertmp != null) { __instance.ingredientFilter.CopyAllowancesFrom(ingredientFiltertmp); }
				}
			}
		}

		class OutfitDetour : Outfit
		{
			[DetourConstructorHarmonyPostfix(typeof(Outfit))]
			public static void Outfit(Outfit __instance)
			{
				__instance.filter = new MarketValueFilter();
			}

			[DetourMemberHarmonyPostfix]
			public static void ExposeData(Outfit __instance)
			{
				if (Scribe.mode == LoadSaveMode.LoadingVars && __instance.filter.GetType() != typeof(MarketValueFilter))
				{
					var tempFilter = __instance.filter;
					__instance.filter = new MarketValueFilter();
					__instance.filter.CopyAllowancesFrom(tempFilter);
				}
			}
		}

		class StorageSettingsDetour : StorageSettings
		{
			private static readonly Action<StorageSettings> NotifyChanged = Utils.GetMethodInvoker<StorageSettings>("TryNotifyChanged");


			[DetourConstructorHarmonyPostfix(typeof(StorageSettings))]
			public static void StorageSettings(StorageSettings __instance)
			{
				__instance.filter = new MarketValueFilter(__instance.filter);
			}

			[DetourConstructorHarmonyPostfix(typeof(StorageSettings))]
			public static void StorageSettings(StorageSettings __instance, IStoreSettingsParent owner)
			{
				__instance.filter = new MarketValueFilter(__instance.filter);
			}

			[DetourMemberHarmonyPostfix]
			public static void ExposeData(StorageSettings __instance)
			{
				if (Scribe.mode == LoadSaveMode.LoadingVars && __instance.filter.GetType() != typeof(MarketValueFilter))
				{
					var tempFilter = __instance.filter;
					__instance.filter = new MarketValueFilter();
					if (tempFilter != null) { __instance.filter.CopyAllowancesFrom(tempFilter); }
				}
			}
		}

		class Dialog_ManageOutfitsDetour
		{
			[DetourConstructorHarmonyPostfix(typeof(Dialog_ManageOutfits))]
			public static void Dialog_ManageOutfits(Dialog_ManageOutfits __instance, Outfit selectedOutfit)
			{
				if (apparelGlobalFilterGet() == null)
				{
					var filter = new MarketValueFilter();
					apparelGlobalFilterInfo.SetValue(null, filter);
					filter.SetAllow(ThingCategoryDefOf.Apparel, true);
				}
			}

			private static readonly Func<ThingFilter> apparelGlobalFilterGet = Utils.GetStaticFieldAccessor<Dialog_ManageOutfits, ThingFilter>("apparelGlobalFilter");
			private static readonly FieldInfo apparelGlobalFilterInfo = typeof(Dialog_ManageOutfits).GetField("apparelGlobalFilter", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField);
		}

		class IngredientCountDetour
		{
			[DetourConstructorHarmonyPostfix(typeof(Verse.IngredientCount))]
			public static void IngredientCount(IngredientCount __instance)
			{
				__instance.filter = new MarketValueFilter();
			}
		}

	}
}
