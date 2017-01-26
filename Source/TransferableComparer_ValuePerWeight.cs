using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ZhentarTweaks
{
	class TransferableComparer_ValuePerWeight : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
		}

		private float GetValueFor(ITransferable t)
		{
			Thing anyThing = t.AnyThing;
			if (!anyThing.def.useHitPoints)
			{
				return 1f;
			}
			return anyThing.MarketValue / anyThing.GetStatValue(StatDefOf.Mass);
		}
	}
}
