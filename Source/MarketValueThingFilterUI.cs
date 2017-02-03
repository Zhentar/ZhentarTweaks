using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ZhentarTweaks
{
	[StaticConstructorOnStartup]
	static class MarketValueThingFilterUI
	{
		private static float viewHeight;

		[DetourMember(typeof(ThingFilterUI))]
		public static void DoThingFilterConfigWindow(Rect rect, ref Vector2 scrollPosition, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null)
		{
			Widgets.DrawMenuSection(rect);
			Text.Font = GameFont.Tiny;
			float num = rect.width - 2f;
			Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
			if (Widgets.ButtonText(rect2, "ClearAll".Translate()))
			{
				filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
			}
			Rect rect3 = new Rect(rect2.xMax + 1f, rect2.y, num / 2f, 24f);
			if (Widgets.ButtonText(rect3, "AllowAll".Translate()))
			{
				filter.SetAllowAll(parentFilter);
			}
			Text.Font = GameFont.Small;
			rect.yMin = rect2.yMax;
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			float num2 = 2f;
			DrawHitPointsFilterConfig(ref num2, viewRect.width, filter);
			DrawQualityFilterConfig(ref num2, viewRect.width, filter);
			DrawMarketValueFilterConfig(ref num2, viewRect.width, filter);
			float num3 = num2;
			Rect rect4 = new Rect(0f, num2, viewRect.width, 9999f);
			Listing_TreeThingFilter listing_TreeThingFilter = new Listing_TreeThingFilter(rect4, filter, parentFilter, forceHiddenDefs, forceHiddenFilters);
			TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
			if (parentFilter != null)
			{
				if (parentFilter.DisplayRootCategory == null)
				{
					parentFilter.RecalculateDisplayRootCategory();
				}
				node = parentFilter.DisplayRootCategory;
			}
			listing_TreeThingFilter.DoCategoryChildren(node, 0, openMask, true);
			listing_TreeThingFilter.End();
			if (Event.current.type == EventType.Layout)
			{
				viewHeight = num3 + listing_TreeThingFilter.CurHeight + 90f;
			}
			Widgets.EndScrollView();
		}

		private static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
		{
			if (!filter.allowedHitPointsConfigurable)
			{
				return;
			}
			Rect rect = new Rect(20f, y, width - 20f, 26f);
			FloatRange allowedHitPointsPercents = filter.AllowedHitPointsPercents;
			Widgets.FloatRange(rect, 1, ref allowedHitPointsPercents, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
			filter.AllowedHitPointsPercents = allowedHitPointsPercents;
			y += 26f;
			y += 5f;
			Text.Font = GameFont.Small;
		}

		private static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
		{
			if (!filter.allowedQualitiesConfigurable)
			{
				return;
			}
			Rect rect = new Rect(20f, y, width - 20f, 26f);
			QualityRange allowedQualityLevels = filter.AllowedQualityLevels;
			Widgets.QualityRange(rect, 2, ref allowedQualityLevels);
			filter.AllowedQualityLevels = allowedQualityLevels;
			y += 26f;
			y += 5f;
			Text.Font = GameFont.Small;
		}

		private static void DrawMarketValueFilterConfig(ref float y, float width, ThingFilter filter)
		{
			if (!(filter is MarketValueFilter))
			{
				return;
			}
			var mvFilter = (MarketValueFilter)filter;
			Rect rect = new Rect(20f, y, width - 20f, 26f);
			var allowedMarketValues = mvFilter.AllowedMarketValue;
			NonlinearRangeSlider(rect, 3, ref allowedMarketValues, "market value", ToStringStyle.Integer);
			mvFilter.AllowedMarketValue = allowedMarketValues;
			y += 26f;
			y += 5f;
			Text.Font = GameFont.Small;
		}

		static MarketValueThingFilterUI()
		{
			C = 2 * Mathf.Log(9);
		}
		
		private static Func<int> draggingIdGet = Utils.GetStaticFieldAccessor<int>(typeof(Widgets), "draggingId");
		private static FieldInfo draggingIdInfo = typeof(Widgets).GetField("draggingId", Detours.UniversalBindingFlags);

		private enum RangeEnd : byte
		{
			None,
			Min,
			Max
		}

		private static RangeEnd curDragEnd;

		private static readonly Texture2D FloatRangeSliderTex = ContentFinder<Texture2D>.Get("UI/Widgets/RangeSlider");

		private static readonly Color RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);

		private static readonly string infinity = "∞";

		// thanks http://stackoverflow.com/questions/7246622/how-to-create-a-slider-with-a-non-linear-scale
		// calculated for center of 500, max of 5000
		private static readonly float B = 62.5f;
		private static readonly float C; //2 * ln(9)

		private static float SliderPosForValue(float value)
		{
			return Mathf.Log((value + B) / B) / C;
		}

		private static readonly float max = 5100f;

		private static float ValueForSliderPos(float pos)
		{
			return B * (Mathf.Exp(C * pos) - 1.0f);
		}

		static float RoundToSignificantDigits(this float d, int digits)
		{
			if (d == 0)
				return 0;

			double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
			return (float) (scale * Math.Round(d / scale, digits));
		}

		public static void NonlinearRangeSlider(Rect rect, int id, ref FloatRange range, string labelKey = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
		{
			Rect rect2 = rect;
			rect2.xMin += 8f;
			rect2.xMax -= 8f;
			GUI.color = RangeControlTextColor;
			string text = $"${range.min.ToStringByStyle(valueStyle)} - " + ( range.max > max ? infinity : $"${range.max.ToStringByStyle(valueStyle)}");
			if (labelKey != null)
			{
				text = text + " " + labelKey;
			}
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;

			float minSliderPos = rect2.x + (rect2.width * SliderPosForValue(range.min));
			float maxSliderPos = rect2.x + (rect2.width * Mathf.Clamp01(SliderPosForValue(range.max)));
			Rect position2 = new Rect(minSliderPos - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, FloatRangeSliderTex);
			Rect position3 = new Rect(maxSliderPos + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, FloatRangeSliderTex);
			if (curDragEnd != RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
			{
				draggingIdInfo.SetValue(null, 0);
				curDragEnd = RangeEnd.None;
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			bool flag = false;
			if (Mouse.IsOver(rect) || draggingIdGet() == id)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != draggingIdGet())
				{
					draggingIdInfo.SetValue(null, id);
					float x = Event.current.mousePosition.x;
					if (x < position2.xMax)
					{
						curDragEnd = RangeEnd.Min;
					}
					else if (x > position3.xMin)
					{
						curDragEnd = RangeEnd.Max;
					}
					else
					{
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						curDragEnd = ((num3 >= num4) ? RangeEnd.Max : RangeEnd.Min);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (curDragEnd != RangeEnd.None && Event.current.type == EventType.MouseDrag))
				{
					float sliderValue = ValueForSliderPos((Event.current.mousePosition.x - rect2.x) / rect2.width);
					sliderValue = RoundToSignificantDigits(sliderValue, 2);
					sliderValue = Mathf.Clamp(sliderValue, 0f, max);
					if (curDragEnd == RangeEnd.Min)
					{
						if (sliderValue != range.min)
						{
							range.min = sliderValue;
							if (range.max < range.min)
							{
								range.max = range.min;
							}
							CheckPlayDragSliderSound();
						}
					}
					else if (curDragEnd == RangeEnd.Max && sliderValue != range.max)
					{
						if (sliderValue == max) { sliderValue = float.MaxValue; }
						range.max = sliderValue;
						if (range.min > range.max)
						{
							range.min = range.max;
						}
						CheckPlayDragSliderSound();
					}
					Event.current.Use();
				}
			}
			Text.Font = font;
		}

		private static readonly Action CheckPlayDragSliderSound = Utils.GetStaticMethodInvoker(typeof(Widgets), "CheckPlayDragSliderSound");
	}
}
