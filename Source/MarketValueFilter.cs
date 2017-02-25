using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	
	class MarketValueFilter : ThingFilter
	{
		private static readonly Func<ThingFilter, Action> settingsChangedCallbackGet = Utils.GetFieldAccessor<ThingFilter, Action>("settingsChangedCallback");
		private static readonly FieldInfo settingsChangedCallbackInfo = typeof(ThingFilter).GetField("settingsChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
		private Action settingsChangedCallback { get { return settingsChangedCallbackGet(this); } set { settingsChangedCallbackInfo.SetValue(this, value); } }

		private FloatRange allowedMarketValue = new FloatRange(0, float.MaxValue);

		public FloatRange AllowedMarketValue
		{
			get { return allowedMarketValue; }
			set { allowedMarketValue = value; }
		} 

		public MarketValueFilter() { }

		public MarketValueFilter(Action callback) : base(callback) { }

		public MarketValueFilter(ThingFilter oldFilter)
		{
			this.settingsChangedCallback = settingsChangedCallbackGet(oldFilter);
		}

		[DetourMemberHarmonyPostfix] //It's not virtual so I can't just implement an override
		public static void ExposeData(MarketValueFilter __instance)
		{
			if (__instance.GetType() == typeof(MarketValueFilter))
			{
				Scribe_Values.LookValue(ref __instance.allowedMarketValue, "allowedMarketValue");
			}
		}

		[DetourMemberHarmonyPostfix] //Also not virtual
		public static void CopyAllowancesFrom(MarketValueFilter __instance, ThingFilter other)
		{
			//Gotta make sure we aren't reading or writing unallocated memory
			if (other.GetType() == __instance.GetType() && __instance.GetType() == typeof(MarketValueFilter))
			{
				__instance.allowedMarketValue = ((MarketValueFilter)other).allowedMarketValue;
			}
		}

		[DetourMemberHarmonyPostfix] //Yup, not virtual again
		public static void Allows(MarketValueFilter __instance, ref bool __result, Thing t)
		{
			if (__result && __instance.GetType() == typeof(MarketValueFilter))
			{
				if (!__instance.allowedMarketValue.IncludesEpsilon(t.GetInnerIfMinified().MarketValue))
				{
					__result = false;
				}
			}
		}
	}
}
