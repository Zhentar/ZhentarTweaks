using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	class MarketValueFilter : Verse.ThingFilter
	{
		private FloatRange allowedMarketValue = new FloatRange(0, float.MaxValue);

		public FloatRange AllowedMarketValue
		{
			get { return allowedMarketValue; }
			set { allowedMarketValue = value; }
		} 

		public MarketValueFilter() : base()
		{ }

		public MarketValueFilter(Action callback) : base(callback)
		{ }

		[DetourMember] //It's not virtual so I can't just implement an override
		public void ExposeData()
		{
			var localSpecFilters = disallowedSpecialFilters;
			Scribe_Collections.LookList(ref localSpecFilters, "disallowedSpecialFilters", LookMode.Def);
			disallowedSpecialFilters = localSpecFilters;
			var aDefs = allowedDefs;
			Scribe_Collections.LookHashSet(ref aDefs, "allowedDefs");
			allowedDefs = aDefs;
			var allowedHP = AllowedHitPointsPercents;
			Scribe_Values.LookValue(ref allowedHP, "allowedHitPointsPercents");
			AllowedHitPointsPercents = allowedHP;
			var allowedQualities = AllowedQualityLevels;
			Scribe_Values.LookValue(ref allowedQualities, "allowedQualityLevels");
			AllowedQualityLevels = allowedQualities;
			if (this.GetType() == typeof(MarketValueFilter))
			{
				Scribe_Values.LookValue(ref allowedMarketValue, "allowedMarketValue");
			}
			else if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Warning("Failed to replace a ThingFilter");
			}
		}

		//TODO: use manual detour to bypass type check and get magic cast
		[DetourMember] //Also not virtual
		public void CopyAllowancesFrom(MarketValueFilter other)
		{
			allowedDefs.Clear();
			foreach (ThingDef current in AllStorableThingDefs)
			{
				SetAllow(current, other.Allows(current));
			}
			disallowedSpecialFilters = other.disallowedSpecialFilters.ListFullCopyOrNull();
			allowedHitPointsPercents = other.allowedHitPointsPercents;
			allowedHitPointsConfigurable = other.allowedHitPointsConfigurable;
			allowedQualities = other.allowedQualities;
			allowedQualitiesConfigurable = other.allowedQualitiesConfigurable;
			thingDefs = other.thingDefs.ListFullCopyOrNull();
			categories = other.categories.ListFullCopyOrNull();
			exceptedThingDefs = other.exceptedThingDefs.ListFullCopyOrNull();
			exceptedCategories = other.exceptedCategories.ListFullCopyOrNull();
			specialFiltersToAllow = other.specialFiltersToAllow.ListFullCopyOrNull();
			specialFiltersToDisallow = other.specialFiltersToDisallow.ListFullCopyOrNull();
			stuffCategoriesToAllow = other.stuffCategoriesToAllow.ListFullCopyOrNull();
			allowAllWhoCanMake = other.allowAllWhoCanMake.ListFullCopyOrNull();
			settingsChangedCallback?.Invoke();
			
			//Gotta make sure we aren't reading or writing unallocated memory
			if (other.GetType() == this.GetType() && this.GetType() == typeof(MarketValueFilter))
			{
				allowedMarketValue = other.allowedMarketValue;
			}
		}

		[DetourMember] //Yup, not virtual again
		public bool Allows(Thing t)
		{
			if (!Allows(t.def))
			{
				return false;
			}
			if (t.def.useHitPoints)
			{
				float num = t.HitPoints / (float)t.MaxHitPoints;
				num = Mathf.Round(num * 100f) / 100f;
				if (!allowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(num)))
				{
					return false;
				}
			}
			if (allowedQualities != QualityRange.All && t.def.FollowQualityThingFilter())
			{
				QualityCategory p;
				if (!t.TryGetQuality(out p))
				{
					p = QualityCategory.Normal;
				}
				if (!allowedQualities.Includes(p))
				{
					return false;
				}
			}
			for (int i = 0; i < disallowedSpecialFilters.Count; i++)
			{
				if (disallowedSpecialFilters[i].Worker.Matches(t))
				{
					return false;
				}
			}

			if (this.GetType() == typeof(MarketValueFilter))
			{
				if (!allowedMarketValue.IncludesEpsilon(t.GetInnerIfMinified().MarketValue))
				{
					return false;
				}
			}

			return true;
		}

		private static IEnumerable<ThingDef> AllStorableThingDefs
		{
			get
			{
				return from def in DefDatabase<ThingDef>.AllDefs
					   where def.EverStoreable
					   select def;
			}
		}


		private static readonly Func<ThingFilter, List<SpecialThingFilterDef>> disallowedSpecialFiltersGet = Utils.GetFieldAccessor<ThingFilter, List<SpecialThingFilterDef>>("disallowedSpecialFilters");
		private static readonly FieldInfo disallowedSpecialFiltersInfo = typeof(ThingFilter).GetField("disallowedSpecialFilters", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<SpecialThingFilterDef> disallowedSpecialFilters { get { return disallowedSpecialFiltersGet(this); } set { disallowedSpecialFiltersInfo.SetValue(this, value); } }


		private static readonly Func<ThingFilter, HashSet<ThingDef>> allowedDefsGet = Utils.GetFieldAccessor<ThingFilter, HashSet<ThingDef>>("allowedDefs");
		private static readonly FieldInfo allowedDefsInfo = typeof(ThingFilter).GetField("allowedDefs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private HashSet<ThingDef> allowedDefs { get { return allowedDefsGet(this); } set { allowedDefsInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, Action> settingsChangedCallbackGet = Utils.GetFieldAccessor<ThingFilter, Action>("settingsChangedCallback");
		private static readonly FieldInfo settingsChangedCallbackInfo = typeof(ThingFilter).GetField("settingsChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private Action settingsChangedCallback { get { return settingsChangedCallbackGet(this); } set { settingsChangedCallbackInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, TreeNode_ThingCategory> displayRootCategoryIntGet = Utils.GetFieldAccessor<ThingFilter, TreeNode_ThingCategory>("displayRootCategoryInt");
		private static readonly FieldInfo displayRootCategoryIntInfo = typeof(ThingFilter).GetField("displayRootCategoryInt", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private TreeNode_ThingCategory displayRootCategoryInt { get { return displayRootCategoryIntGet(this); } set { displayRootCategoryIntInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, FloatRange> allowedHitPointsPercentsGet = Utils.GetFieldAccessor<ThingFilter, FloatRange>("allowedHitPointsPercents");
		private static readonly FieldInfo allowedHitPointsPercentsInfo = typeof(ThingFilter).GetField("allowedHitPointsPercents", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private FloatRange allowedHitPointsPercents { get { return allowedHitPointsPercentsGet(this); } set { allowedHitPointsPercentsInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, QualityRange> allowedQualitiesGet = Utils.GetFieldAccessor<ThingFilter, QualityRange>("allowedQualities");
		private static readonly FieldInfo allowedQualitiesInfo = typeof(ThingFilter).GetField("allowedQualities", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private QualityRange allowedQualities { get { return allowedQualitiesGet(this); } set { allowedQualitiesInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<ThingDef>> thingDefsGet = Utils.GetFieldAccessor<ThingFilter, List<ThingDef>>("thingDefs");
		private static readonly FieldInfo thingDefsInfo = typeof(ThingFilter).GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<ThingDef> thingDefs { get { return thingDefsGet(this); } set { thingDefsInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<String>> categoriesGet = Utils.GetFieldAccessor<ThingFilter, List<String>>("categories");
		private static readonly FieldInfo categoriesInfo = typeof(ThingFilter).GetField("categories", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<String> categories { get { return categoriesGet(this); } set { categoriesInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<ThingDef>> exceptedThingDefsGet = Utils.GetFieldAccessor<ThingFilter, List<ThingDef>>("exceptedThingDefs");
		private static readonly FieldInfo exceptedThingDefsInfo = typeof(ThingFilter).GetField("exceptedThingDefs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<ThingDef> exceptedThingDefs { get { return exceptedThingDefsGet(this); } set { exceptedThingDefsInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<String>> exceptedCategoriesGet = Utils.GetFieldAccessor<ThingFilter, List<String>>("exceptedCategories");
		private static readonly FieldInfo exceptedCategoriesInfo = typeof(ThingFilter).GetField("exceptedCategories", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<String> exceptedCategories { get { return exceptedCategoriesGet(this); } set { exceptedCategoriesInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<String>> specialFiltersToAllowGet = Utils.GetFieldAccessor<ThingFilter, List<String>>("specialFiltersToAllow");
		private static readonly FieldInfo specialFiltersToAllowInfo = typeof(ThingFilter).GetField("specialFiltersToAllow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<String> specialFiltersToAllow { get { return specialFiltersToAllowGet(this); } set { specialFiltersToAllowInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<String>> specialFiltersToDisallowGet = Utils.GetFieldAccessor<ThingFilter, List<String>>("specialFiltersToDisallow");
		private static readonly FieldInfo specialFiltersToDisallowInfo = typeof(ThingFilter).GetField("specialFiltersToDisallow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<String> specialFiltersToDisallow { get { return specialFiltersToDisallowGet(this); } set { specialFiltersToDisallowInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<StuffCategoryDef>> stuffCategoriesToAllowGet = Utils.GetFieldAccessor<ThingFilter, List<StuffCategoryDef>>("stuffCategoriesToAllow");
		private static readonly FieldInfo stuffCategoriesToAllowInfo = typeof(ThingFilter).GetField("stuffCategoriesToAllow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<StuffCategoryDef> stuffCategoriesToAllow { get { return stuffCategoriesToAllowGet(this); } set { stuffCategoriesToAllowInfo.SetValue(this, value); } }

		private static readonly Func<ThingFilter, List<ThingDef>> allowAllWhoCanMakeGet = Utils.GetFieldAccessor<ThingFilter, List<ThingDef>>("allowAllWhoCanMake");
		private static readonly FieldInfo allowAllWhoCanMakeInfo = typeof(ThingFilter).GetField("allowAllWhoCanMake", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private List<ThingDef> allowAllWhoCanMake { get { return allowAllWhoCanMakeGet(this); } set { allowAllWhoCanMakeInfo.SetValue(this, value); } }
	}
}
