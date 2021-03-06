﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	class _Designate_SmoothFloor : Designator_SmoothFloor
	{
		[DetourMember]
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(Map))
			{
				return false;
			}
			if (c.Fogged(Map))
			{
				return false;
			}
			if (Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) != null)
			{
				return "TerrainBeingSmoothed".Translate();
			}
			Building edifice = c.GetEdifice(Map);
			if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain(Map);
			if (!terrain.affordances.Contains(TerrainAffordance.SmoothableStone))
			{
				return "MessageMustDesignateSmoothableFloor".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}
	}

	static class _GenConstruct
	{
		[DetourMember(typeof(GenConstruct))]
		public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!current.InBounds(map))
				{
					return new AcceptanceReport("OutOfBounds".Translate());
				}
				if (current.InNoBuildEdgeArea(map) && !DebugSettings.godMode && entDef != ThingDefOf.DeepDrill)
				{
					return "TooCloseToMapEdge".Translate();
				}
				iterator.MoveNext();
			}
			if (center.Fogged(map))
			{
				return "CannotPlaceInUndiscovered".Translate();
			}
			List<Thing> thingList = center.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing != thingToIgnore)
				{
					if (thing.Position == center && thing.Rotation == rot)
					{
						if (thing.def == entDef)
						{
							return new AcceptanceReport("IdenticalThingExists".Translate());
						}
						if (thing.def.entityDefToBuild == entDef)
						{
							if (thing is Blueprint)
							{
								return new AcceptanceReport("IdenticalBlueprintExists".Translate());
							}
							return new AcceptanceReport("IdenticalThingExists".Translate());
						}
					}
				}
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.hasInteractionCell)
			{
				IntVec3 c = Thing.InteractionCellWhenAt(thingDef, center, rot, map);
				if (!c.InBounds(map))
				{
					return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
				}
				List<Thing> list = map.thingGrid.ThingsListAtFast(c);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] != thingToIgnore)
					{
						if (list[j].def.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotBlocked".Translate(list[j].LabelNoCount).CapitalizeFirst());
						}
						Blueprint blueprint = list[j] as Blueprint;
						if (blueprint != null && blueprint.def.entityDefToBuild.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotWillBeBlocked".Translate(blueprint.LabelNoCount).CapitalizeFirst());
						}
					}
				}
			}
			if (entDef.passability != Traversability.Standable)
			{
				foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(center, rot, entDef.Size))
				{
					if (current2.InBounds(map))
					{
						thingList = current2.GetThingList(map);
						for (int k = 0; k < thingList.Count; k++)
						{
							Thing thing2 = thingList[k];
							if (thing2 != thingToIgnore)
							{
								Blueprint blueprint2 = thing2 as Blueprint;
								ThingDef thingDef3;
								if (blueprint2 != null)
								{
									ThingDef thingDef2 = blueprint2.def.entityDefToBuild as ThingDef;
									if (thingDef2 == null)
									{
										goto IL_37E;
									}
									thingDef3 = thingDef2;
								}
								else
								{
									thingDef3 = thing2.def;
								}
								if (thingDef3.hasInteractionCell && cellRect.Contains(Thing.InteractionCellWhenAt(thingDef3, thing2.Position, thing2.Rotation, thing2.Map)))
								{
									return new AcceptanceReport("WouldBlockInteractionSpot".Translate(entDef.label, thingDef3.label).CapitalizeFirst());
								}
							}
							IL_37E:;
						}
					}
				}
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				if (map.terrainGrid.TerrainAt(center) == terrainDef)
				{
					return new AcceptanceReport("TerrainIsAlready".Translate(terrainDef.label));
				}
				if (map.designationManager.DesignationAt(center, DesignationDefOf.SmoothFloor) != null)
				{
					return new AcceptanceReport("BeingSmoothed".Translate());
				}
			}
			if (!GenConstruct.CanBuildOnTerrain(entDef, center, map, rot, thingToIgnore))
			{
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			}
			if (!godMode)
			{
				CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
				while (!iterator2.Done())
				{
					thingList = iterator2.Current.GetThingList(map);
					for (int l = 0; l < thingList.Count; l++)
					{
						Thing thing3 = thingList[l];
						if (thing3 != thingToIgnore)
						{
							if (!GenConstruct.CanPlaceBlueprintOver(entDef, thing3.def))
							{
								return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
							}
						}
					}
					iterator2.MoveNext();
				}
			}
			if (entDef.PlaceWorkers != null)
			{
				for (int m = 0; m < entDef.PlaceWorkers.Count; m++)
				{
					AcceptanceReport result = entDef.PlaceWorkers[m].AllowsPlacing(entDef, center, rot, thingToIgnore);
					if (!result.Accepted)
					{
						return result;
					}
				}
			}
			return AcceptanceReport.WasAccepted;
		}

	}

	class _AreaManager : AreaManager
	{
		private _AreaManager() : base(null)
		{ }

		[DetourMember]
		public new bool CanMakeNewAllowed(AllowedAreaMode mode) => true;
	}

	public class SunLampPlanDesignatorAdd : Designator
	{
		
		public SunLampPlanDesignatorAdd()
		{
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.defaultLabel = "Sun Lamp Plan";
			this.defaultDesc = "Place planning designations in the shape of a sun lamp radius";
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn");
			this.soundSucceeded = SoundDefOf.DesignatePlanAdd;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(Map))
			{
				return false;
			}
			if (c.InNoBuildEdgeArea(Map))
			{
				return "TooCloseToMapEdge".Translate();
			}

			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			var desDef = DesignationDefOf.Plan;
			foreach (var cell in GenRadial.RadialCellsAround(c, 5.8f, true))
			{
				if (Map.designationManager.DesignationAt(cell, desDef) == null)
				{
					Map.designationManager.AddDesignation(new Designation(new LocalTargetInfo(cell), desDef));
				}
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			GenDraw.DrawNoBuildEdgeLines();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				GenDraw.DrawRadiusRing(UI.MouseCell(), 5.8f);
			}
		}

		public override void DrawMouseAttachments()
		{
			var intVec = UI.MouseCell();
			float totalFertility = 0;
			foreach (var cell in GenRadial.RadialCellsAround(intVec, 5.8f, false))
			{
				if (!cell.InBounds(Map)) continue;
				if (Map.fogGrid.IsFogged(cell)) continue;
					
				var fertility = CalculateFertilityAt(cell);
				if (fertility >= 0.4)
				{
					Vector3 v = GenMapUI.LabelDrawPosFor(cell);
					GenMapUI.DrawThingLabel(v, fertility.ToString(), FertilityColor(fertility));
				}
				totalFertility += fertility;
			}
			var avgFertility = totalFertility / (GenRadial.NumCellsInRadius(5.8f) - 1);
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(Event.current.mousePosition.x + 19f, Event.current.mousePosition.y + 19f, 100f, 100f);
			GUI.color = FertilityColor(avgFertility);
			Widgets.Label(rect, avgFertility.ToString("F3"));
			GUI.color = Color.white;
			GenUI.DrawMouseAttachment(null, string.Empty);
		}
			
		private float CalculateFertilityAt(IntVec3 loc)
		{
			Thing edifice = loc.GetEdifice(Map);
			if (edifice != null && edifice.def.fertility >= 0.0)
				return edifice.def.fertility;
			if (Map.terrainGrid.TerrainAt(loc).fertility > 0.0)
				return Map.terrainGrid.TerrainAt(loc).fertility;
			var underGrid = underGridGet(Map.terrainGrid);
			var underTerrain = underGrid[Map.cellIndices.CellToIndex(loc)];
			return (underTerrain?.fertility).GetValueOrDefault();
		}

		private static readonly Func<TerrainGrid, TerrainDef[]> underGridGet = Utils.GetFieldAccessor<TerrainGrid, TerrainDef[]>("underGrid");

		private static readonly Color ColorInfertile = Color.red;

		private static readonly Color ColorFertile = Color.green;

		private static Color FertilityColor(float fertility)
		{
			float num = Mathf.InverseLerp(0, 1.4f, fertility);
			num = Mathf.Clamp01(num);
			return Color.Lerp(ColorInfertile, ColorFertile, num);
		}
	}


	public static class Designator_PlaceDetour
	{
		[DetourMember]
		public static void SelectedUpdate(this Designator_Place @this)
		{
			GenDraw.DrawNoBuildEdgeLines();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				IntVec3 intVec = UI.MouseCell();
				if (@this.PlacingDef is TerrainDef)
				{
					GenUI.RenderMouseoverBracket();
					return;
				}
				Color ghostCol;
				if (@this.CanDesignateCell(intVec).Accepted)
				{
					ghostCol = new Color(0.5f, 1f, 0.6f, 0.4f);
				}
				else
				{
					ghostCol = new Color(1f, 0f, 0f, 0.4f);
				}
				DrawGhost(@this, ghostCol);
				if (@this.CanDesignateCell(intVec).Accepted && @this.PlacingDef.specialDisplayRadius > 0.01f)
				{
					GenDraw.DrawRadiusRing(UI.MouseCell(), @this.PlacingDef.specialDisplayRadius);
					if ((@this.PlacingDef as ThingDef)?.building?.turretGunDef?.Verbs?.FirstOrDefault()?.requireLineOfSight == true)
					{
						var map = Find.VisibleMap;
						float range = ((ThingDef) @this.PlacingDef).building.turretGunDef.Verbs[0].range;
						foreach (var cell in GenRadial.RadialCellsAround(intVec, range, false))
						{
							if (GenSight.LineOfSight(intVec, cell,  map, true) != GenSight.LineOfSight(cell, intVec, map, true)) { CellRenderer.RenderCell(cell, 0f); }
							if (GenSight.LineOfSight(intVec, cell, map, true))
							{
								CellRenderer.RenderCell(cell, 0.6f);
								continue;
							}
							
						}
					}
				}
				GenDraw.DrawInteractionCell((ThingDef)@this.PlacingDef, intVec, placingRotGet(@this));
			}
		}

		private static readonly Action<Designator_Place, Color> DrawGhost = Utils.GetMethodInvoker<Designator_Place, Color>("DrawGhost");

		private static readonly Func<Designator_Place, Rot4> placingRotGet = Utils.GetFieldAccessor<Designator_Place, Rot4>("placingRot");
	}
}
