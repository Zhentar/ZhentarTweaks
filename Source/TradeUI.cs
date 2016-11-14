using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	class TradeUI
	{
		[DetourClassMethod(typeof(TradeUI))]
		private static void DrawPrice(Rect rect, Tradeable trad, TradeAction action)
		{
			if (trad.IsCurrency || !trad.TraderWillTrade)
			{
				return;
			}
			rect = rect.Rounded();
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			float num = trad.PriceFor(action);
			PriceType pType = PriceTypeUtlity.ClosestPriceType(num / trad.BaseMarketValue);
			switch (pType)
			{
				case PriceType.VeryCheap:
					GUI.color = new Color(0f, 1f, 0f);
					break;
				case PriceType.Cheap:
					GUI.color = new Color(0.5f, 1f, 0.5f);
					break;
				case PriceType.Normal:
					GUI.color = Color.white;
					break;
				case PriceType.Expensive:
					GUI.color = new Color(1f, 0.5f, 0.5f);
					break;
				case PriceType.Exorbitant:
					GUI.color = new Color(1f, 0f, 0f);
					break;
			}
			//Add trade session price factor to price display
			float factor = TradeUtility.RandomPriceFactorFor(TradeSession.trader, trad);
			factor = (factor - 1) * 100;
			string label = "$" + num.ToString("F2") + " (" + factor.ToString("F1") + "%)";
			Func<string> textGetter = delegate
			{
				if (!trad.HasAnyThing)
				{
					return string.Empty;
				}
				return ((action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate()) + "\n\n" +
					   "PriceTypeDesc".Translate(("PriceType" + pType).Translate());
			};
			TooltipHandler.TipRegion(rect, new TipSignal(textGetter, trad.GetHashCode() * 297));
			Rect rect2 = new Rect(rect);
			rect2.xMax -= 5f;
			rect2.xMin += 5f;
			if (Text.Anchor == TextAnchor.MiddleLeft)
			{
				rect2.xMax += 300f;
			}
			if (Text.Anchor == TextAnchor.MiddleRight)
			{
				rect2.xMin -= 300f;
			}
			Widgets.Label(rect2, label);
			GUI.color = Color.white;
		}


	}
}
