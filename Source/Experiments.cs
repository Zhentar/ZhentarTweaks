using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using UnityEngine.SceneManagement;
using Verse;

namespace ZhentarTweaks
{
	public static class Experiments
	{
		private const int blah = 0 ;

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
			float factor = TradeUtility.RandomPriceFactorFor(TradeSession.trader, trad);
			factor = (factor - 1)*100;
			string label = "$" + num.ToString("F2") + " (" + factor.ToString("F1") + "%)";
			Func<string> textGetter = delegate
			{
				if (!trad.HasAnyThing)
				{
					return string.Empty;
				}
				return ((action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate()) + "\n\n" + "PriceTypeDesc".Translate(("PriceType" + pType).Translate());
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

		private static bool quickStarted;

		public static bool CheckQuickStart()
		{
			Log.Message("Checking Quick Start!!!");
			if (GenCommandLine.CommandLineArgPassed("quicktest") && !quickStarted && GenScene.InEntryScene)
			{
				string savefile;
				if (GenCommandLine.TryGetCommandLineArg("quicktest", out savefile))
				{
					Current.Game = new Game();
					Current.Game.InitData = new GameInitData();
					Current.Game.InitData.mapToLoad = savefile;
				}
				quickStarted = true;
				SceneManager.LoadScene("Map");
				return true;
			}
			return false;
		}
	}


	public class CoolerTest : Building_TempControl
	{
		public override void TickRare()
		{
			if (this.compPowerTrader.PowerOn)
			{
				IntVec3 intVec = Position + IntVec3.South.RotatedBy(Rotation);
				IntVec3 intVec2 = Position + IntVec3.North.RotatedBy(Rotation);
				bool flag = false;
				if (!intVec2.Impassable() && !intVec.Impassable())
				{
					float temperature = intVec2.GetTemperature();
					float temperature2 = intVec.GetTemperature();
					float num = temperature - temperature2;
					//if (temperature - 40f > num)
					//{
					//	num = temperature - 40f;
					//}
					float num2 = 1f - num*0.0076923077f;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					float num3 = this.compTempControl.Props.energyPerSecond*num2*4.16666651f;
					float num4 = GenTemperature.ControlTemperatureTempChange(intVec, num3, this.compTempControl.targetTemperature);
					flag = !Mathf.Approximately(num4, 0f);
					if (flag)
					{
						intVec.GetRoom().Temperature += num4;
						GenTemperature.PushHeat(intVec2, -num3*1.25f);
					}
				}
				CompProperties_Power props = this.compPowerTrader.Props;
				if (flag)
				{
					this.compPowerTrader.PowerOutput = -props.basePowerConsumption;
				}
				else
				{
					this.compPowerTrader.PowerOutput = -props.basePowerConsumption*this.compTempControl.Props.lowPowerConsumptionFactor;
				}
				this.compTempControl.operatingAtHighPower = flag;
			}
		}
	}
}
