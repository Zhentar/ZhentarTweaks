using System;
using RimWorld;
using Verse;

namespace ZhentarTweaks
{
	
	class MarketValueFilter : ThingFilter
	{
		private static readonly Func<ThingFilter, Action> settingsChangedCallbackGet = Utils.GetFieldAccessor<ThingFilter, Action>("settingsChangedCallback");

		private FloatRange allowedMarketValue = new FloatRange(0, float.MaxValue);

		public FloatRange AllowedMarketValue
		{
			get { return allowedMarketValue; }
			set { allowedMarketValue = value; }
		} 

		public MarketValueFilter() { }
		
		public MarketValueFilter(ThingFilter oldFilter) : base(settingsChangedCallbackGet(oldFilter)) { }

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref allowedMarketValue, "allowedMarketValue");
		}

		public override void CopyAllowancesFrom(ThingFilter other)
		{
			base.CopyAllowancesFrom(other);
			if (other is MarketValueFilter)
			{
				allowedMarketValue = ((MarketValueFilter) other).allowedMarketValue;
			}
		}
		
		public override bool Allows(Thing t)
		{
			return allowedMarketValue.IncludesEpsilon(t.GetInnerIfMinified().MarketValue) && base.Allows(t);
		}
	}
}
