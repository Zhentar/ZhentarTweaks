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
	[StaticConstructorOnStartup]
	class MarketValueFilterInjector
	{

		class ThingFilterDetour : ThingFilter
		{
			private static readonly Func<object, System.Collections.Generic.HashSet<Verse.ThingDef>> allowedDefsGet = Utils.GetFieldAccessorNoInherit<Verse.ThingFilter, System.Collections.Generic.HashSet<Verse.ThingDef>>("allowedDefs");
			private static readonly FieldInfo allowedDefsInfo = typeof(Verse.ThingFilter).GetField("allowedDefs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			private System.Collections.Generic.HashSet<Verse.ThingDef> allowedDefs { get { return allowedDefsGet(this); } set { allowedDefsInfo.SetValue(this, value); } }
			private static readonly Func<object, System.Collections.Generic.List<Verse.SpecialThingFilterDef>> disallowedSpecialFiltersGet = Utils.GetFieldAccessorNoInherit<Verse.ThingFilter, System.Collections.Generic.List<Verse.SpecialThingFilterDef>>("disallowedSpecialFilters");
			private static readonly FieldInfo disallowedSpecialFiltersInfo = typeof(Verse.ThingFilter).GetField("disallowedSpecialFilters", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			private System.Collections.Generic.List<Verse.SpecialThingFilterDef> disallowedSpecialFilters { get { return disallowedSpecialFiltersGet(this); } set { disallowedSpecialFiltersInfo.SetValue(this, value); } }
			private static readonly Func<object, Verse.FloatRange> allowedHitPointsPercentsGet = Utils.GetFieldAccessorNoInherit<Verse.ThingFilter, Verse.FloatRange>("allowedHitPointsPercents");
			private static readonly FieldInfo allowedHitPointsPercentsInfo = typeof(Verse.ThingFilter).GetField("allowedHitPointsPercents", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			private Verse.FloatRange allowedHitPointsPercents { get { return allowedHitPointsPercentsGet(this); } set { allowedHitPointsPercentsInfo.SetValue(this, value); } }
			private static readonly Func<object, RimWorld.QualityRange> allowedQualitiesGet = Utils.GetFieldAccessorNoInherit<Verse.ThingFilter, RimWorld.QualityRange>("allowedQualities");
			private static readonly FieldInfo allowedQualitiesInfo = typeof(Verse.ThingFilter).GetField("allowedQualities", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			private RimWorld.QualityRange allowedQualities { get { return allowedQualitiesGet(this); } set { allowedQualitiesInfo.SetValue(this, value); } }
			private static readonly Func<object, System.Action> settingsChangedCallbackGet = Utils.GetFieldAccessorNoInherit<Verse.ThingFilter, System.Action>("settingsChangedCallback");
			private static readonly FieldInfo settingsChangedCallbackInfo = typeof(Verse.ThingFilter).GetField("settingsChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			private System.Action settingsChangedCallback { get { return settingsChangedCallbackGet(this); } set { settingsChangedCallbackInfo.SetValue(this, value); } }
			[DetourConstructor(typeof(Verse.ThingFilter))]
			public void ThingFilter()
			{
				this.allowedDefs = new HashSet<ThingDef>();
				this.disallowedSpecialFilters = new List<SpecialThingFilterDef>();
				this.allowedHitPointsPercents = FloatRange.ZeroToOne;
				this.allowedHitPointsConfigurable = true;
				this.allowedQualities = QualityRange.All;
				this.allowedQualitiesConfigurable = true;
			}

			[DetourConstructor(typeof(Verse.ThingFilter))]
			public void ThingFilter(Action settingsChangedCallback)
			{
				this.allowedDefs = new HashSet<ThingDef>();
				this.disallowedSpecialFilters = new List<SpecialThingFilterDef>();
				this.allowedHitPointsPercents = FloatRange.ZeroToOne;
				this.allowedHitPointsConfigurable = true;
				this.allowedQualities = QualityRange.All;
				this.allowedQualitiesConfigurable = true;
				this.settingsChangedCallback = settingsChangedCallback;
			}
		}



		class Bill : Object
		{
			private static readonly Func<object, int> loadIDGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, int>("loadID");
			private static readonly FieldInfo loadIDInfo = typeof(RimWorld.Bill).GetField("loadID", Detours.UniversalBindingFlags);
			private int loadID { get { return loadIDGet(this); } set { loadIDInfo.SetValue(this, value); } }
			private static readonly Func<object, Single> ingredientSearchRadiusGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, Single>("ingredientSearchRadius");
			private static readonly FieldInfo ingredientSearchRadiusInfo = typeof(RimWorld.Bill).GetField("ingredientSearchRadius", Detours.UniversalBindingFlags);
			private Single ingredientSearchRadius { get { return ingredientSearchRadiusGet(this); } set { ingredientSearchRadiusInfo.SetValue(this, value); } }
			private static readonly Func<object, IntRange> allowedSkillRangeGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, IntRange>("allowedSkillRange");
			private static readonly FieldInfo allowedSkillRangeInfo = typeof(RimWorld.Bill).GetField("allowedSkillRange", Detours.UniversalBindingFlags);
			private IntRange allowedSkillRange { get { return allowedSkillRangeGet(this); } set { allowedSkillRangeInfo.SetValue(this, value); } }
			private static readonly Func<object, int> lastIngredientSearchFailTicksGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, int>("lastIngredientSearchFailTicks");
			private static readonly FieldInfo lastIngredientSearchFailTicksInfo = typeof(RimWorld.Bill).GetField("lastIngredientSearchFailTicks", Detours.UniversalBindingFlags);
			private int lastIngredientSearchFailTicks { get { return lastIngredientSearchFailTicksGet(this); } set { lastIngredientSearchFailTicksInfo.SetValue(this, value); } }
			private static readonly Func<object, Verse.RecipeDef> recipeGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, Verse.RecipeDef>("recipe");
			private static readonly FieldInfo recipeInfo = typeof(RimWorld.Bill).GetField("recipe", Detours.UniversalBindingFlags);
			private Verse.RecipeDef recipe { get { return recipeGet(this); } set { recipeInfo.SetValue(this, value); } }
			private static readonly Func<object, ThingFilter> ingredientFilterGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, ThingFilter>("ingredientFilter");
			private static readonly FieldInfo ingredientFilterInfo = typeof(RimWorld.Bill).GetField("ingredientFilter", Detours.UniversalBindingFlags);
			private ThingFilter ingredientFilter { get { return ingredientFilterGet(this); } set { ingredientFilterInfo.SetValue(this, value); } }
			private static readonly Func<object, bool> suspendedGet = Utils.GetFieldAccessorNoInherit<RimWorld.Bill, bool>("suspended");
			private static readonly FieldInfo suspendedInfo = typeof(RimWorld.Bill).GetField("suspended", Detours.UniversalBindingFlags);
			private bool suspended { get { return suspendedGet(this); } set { suspendedInfo.SetValue(this, value); } }


			[DetourConstructor(typeof(RimWorld.Bill))]
			public Bill(Verse.RecipeDef recipe)
			{
				this.loadID = -1;
				this.ingredientSearchRadius = 999f;
				this.allowedSkillRange = new IntRange(0, 20);
				this.lastIngredientSearchFailTicks = -99999;
				this.recipe = recipe;
				this.ingredientFilter = new MarketValueFilter();
				this.ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
				this.loadID = Find.World.uniqueIDsManager.GetNextBillID();
			}

			[DetourMember(typeof(RimWorld.Bill))]
			public virtual void ExposeData()
			{
				var loadIDtmp = loadID;
				Scribe_Values.LookValue(ref loadIDtmp, "loadID");
				loadID = loadIDtmp;
				var recipetmp = recipe;
				Scribe_Defs.LookDef(ref recipetmp, "recipe");
				recipe = recipetmp;
				var suspendedtmp = suspended;
				Scribe_Values.LookValue(ref suspendedtmp, "suspended");
				suspended = suspendedtmp;
				var ingredientSearchRadiustmp = ingredientSearchRadius;
				Scribe_Values.LookValue(ref ingredientSearchRadiustmp, "ingredientSearchRadius", 999f);
				ingredientSearchRadius = ingredientSearchRadiustmp;
				var allowedSkillRangetmp = allowedSkillRange;
				Scribe_Values.LookValue(ref allowedSkillRangetmp, "allowedSkillRange");
				allowedSkillRange = allowedSkillRangetmp;
				if (Scribe.mode == LoadSaveMode.Saving && recipe.fixedIngredientFilter != null)
				{
					foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
					{
						if (!recipe.fixedIngredientFilter.Allows(current))
						{
							ingredientFilter.SetAllow(current, false);
						}
					}
				}
				var ingredientFiltertmp = ingredientFilter;
				Scribe_Deep.LookDeep(ref ingredientFiltertmp, "ingredientFilter");
				ingredientFilter = ingredientFiltertmp;
				if (Scribe.mode == LoadSaveMode.LoadingVars && ingredientFiltertmp.GetType() != typeof(MarketValueFilter))
				{
					ingredientFilter = new MarketValueFilter();
					if (ingredientFiltertmp != null) { ingredientFilter.CopyAllowancesFrom(ingredientFiltertmp); }
				}
			}
		}

		class OutfitDetour : Outfit
		{
			[DetourConstructor(typeof(Outfit))]
			public void Outfit()
			{
				this.filter = new MarketValueFilter();
			}
			[DetourConstructor(typeof(Outfit))]
			public void Outfit(int uniqueId, string label)
			{
				this.filter = new MarketValueFilter();
				this.uniqueId = uniqueId;
				this.label = label;
			}

			[DetourMember]
			public void ExposeData()
			{
				Scribe_Values.LookValue(ref uniqueId, "uniqueId");
				Scribe_Values.LookValue(ref label, "label");
				Scribe_Deep.LookDeep(ref filter, "filter");
				if (Scribe.mode == LoadSaveMode.LoadingVars && filter.GetType() != typeof(MarketValueFilter))
				{
					var tempFilter = filter;
					filter = new MarketValueFilter();
					filter.CopyAllowancesFrom(tempFilter);
				}
			}
		}

		class StorageSettingsDetour : StorageSettings
		{
			private static readonly Func<object, StoragePriority> priorityIntGet = Utils.GetFieldAccessorNoInherit<StorageSettings, StoragePriority>("priorityInt");
			private static readonly FieldInfo priorityIntInfo = typeof(StorageSettings).GetField("priorityInt", Detours.UniversalBindingFlags);
			private StoragePriority priorityInt { get { return priorityIntGet(this); } set { priorityIntInfo.SetValue(this, value); } }

			private static readonly Action<StorageSettings> NotifyChanged = Utils.GetMethodInvoker<StorageSettings>("TryNotifyChanged");

			[DetourConstructor(typeof(StorageSettings))]
			public void StorageSettings()
			{
				this.priorityInt = StoragePriority.Normal;
				this.filter = new MarketValueFilter( () => NotifyChanged(this));
			}


			[DetourConstructor(typeof(RimWorld.StorageSettings))]
			public void StorageSettings(IStoreSettingsParent owner)
			{
				this.priorityInt = StoragePriority.Normal;
				this.owner = owner;
				if (owner != null)
				{
					StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
					if (parentStoreSettings != null)
					{
						this.priorityInt = priorityIntGet(parentStoreSettings);
					}
				}
				this.filter = new MarketValueFilter(() => NotifyChanged(this));
			}


			[DetourMember]
			public void ExposeData()
			{
				var priorityInttmp = priorityInt;
				Scribe_Values.LookValue(ref priorityInttmp, "priority");
				priorityInt = priorityInttmp;
				Scribe_Deep.LookDeep(ref filter, "filter");
				if (Scribe.mode == LoadSaveMode.LoadingVars && filter.GetType() != typeof(MarketValueFilter))
				{
					var tempFilter = filter;
					filter = new MarketValueFilter();
					if (tempFilter != null) { filter.CopyAllowancesFrom(tempFilter); }
				}
			}



		}

		class Dialog_ManageOutfitsDetour : Dialog_ManageOutfits
		{
			public Dialog_ManageOutfitsDetour(Outfit selectedOutfit) : base(selectedOutfit)
			{}

			[DetourConstructor(typeof(Dialog_ManageOutfits))]
			public void Dialog_ManageOutfits(Outfit selectedOutfit)
			{
				this.forcePause = true;
				this.doCloseX = true;
				this.closeOnEscapeKey = true;
				this.doCloseButton = true;
				this.closeOnClickedOutside = true;
				this.absorbInputAroundWindow = true;
				if (apparelGlobalFilterGet() == null)
				{
					var filter = new MarketValueFilter();
					apparelGlobalFilterInfo.SetValue(null, filter);
					filter.SetAllow(ThingCategoryDefOf.Apparel, true);
				}
				SelectedOutfitInfo.SetValue(this, selectedOutfit, null);

				//Window constructor, since this doesn't call base..ctor
				this.layer = WindowLayer.Dialog;
				this.closeOnEscapeKey = true;
				this.preventCameraMotion = true;
				this.doWindowBackground = true;
				this.onlyOneOfTypeAllowed = true;
				this.drawShadow = true;
				this.focusWhenOpened = true;
				this.shadowAlpha = 1f;
				CloseButSizeInfo.SetValue(this, new Vector2(120f, 40f)); //readonly field
				this.soundAppear = SoundDefOf.DialogBoxAppear;
				this.soundClose = SoundDefOf.Click;
			}

			private static readonly FieldInfo CloseButSizeInfo = typeof(Dialog_ManageOutfits).GetField("CloseButSize", Detours.UniversalBindingFlags);
			private static readonly PropertyInfo SelectedOutfitInfo = typeof(Dialog_ManageOutfits).GetProperty("SelectedOutfit", Detours.UniversalBindingFlags);
			private static readonly Func<ThingFilter> apparelGlobalFilterGet = Utils.GetStaticFieldAccessor<Dialog_ManageOutfits, ThingFilter>("apparelGlobalFilter");
			private static readonly FieldInfo apparelGlobalFilterInfo = typeof(Dialog_ManageOutfits).GetField("apparelGlobalFilter", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField);

			public override void DoWindowContents(Rect inRect)
			{
				throw new NotImplementedException();
			}
		}

		class IngredientCount : Object
		{
			private static readonly Func<object, ThingFilter> filterGet = Utils.GetFieldAccessorNoInherit<Verse.IngredientCount, ThingFilter>("filter");
			private static readonly FieldInfo filterInfo = typeof(Verse.IngredientCount).GetField("filter", Detours.UniversalBindingFlags);
			private ThingFilter filter { get { return filterGet(this); } set { filterInfo.SetValue(this, value); } }
			private static readonly Func<object, Single> countGet = Utils.GetFieldAccessorNoInherit<Verse.IngredientCount, Single>("count");
			private static readonly FieldInfo countInfo = typeof(Verse.IngredientCount).GetField("count", Detours.UniversalBindingFlags);
			private Single count { get { return countGet(this); } set { countInfo.SetValue(this, value); } }
			[DetourConstructor(typeof(Verse.IngredientCount))]
			public IngredientCount()
			{
				this.filter = new MarketValueFilter();
				this.count = 1f;
			}
		}

	}
}
