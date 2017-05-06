using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ZhentarTweaks
{
	class TransferableComparer_StuffUsed : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
		}

		private string GetValueFor(Transferable t)
		{
			Thing anyThing = t.AnyThing;

			return anyThing.GetInnerIfMinified().Stuff?.LabelAsStuff ?? "";
		}
	}
}
