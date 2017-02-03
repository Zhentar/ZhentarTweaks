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
			return anyThing.GetInnerIfMinified().MarketValue / anyThing.GetStatValue(StatDefOf.Mass);
		}
	}
}
