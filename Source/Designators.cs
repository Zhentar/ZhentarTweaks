using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	class Designators
	{
		[DetourClassMethod(typeof(Designator_SmoothFloor))]
		public AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds())
			{
				return false;
			}
			if (c.Fogged())
			{
				return false;
			}
			if (Find.DesignationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) != null)
			{
				return "TerrainBeingSmoothed".Translate();
			}
			Building edifice = c.GetEdifice();
			if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain();
			if (!terrain.affordances.Contains(TerrainAffordance.SmoothableStone))
			{
				return "MessageMustDesignateSmoothableFloor".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		[DetourClassMethod(typeof(GenConstruct))]
		public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, bool godMode = false, Thing thingToIgnore = null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!current.InBounds())
				{
					return new AcceptanceReport("OutOfBounds".Translate());
				}
				if (current.InNoBuildEdgeArea() && entDef != ThingDefOf.DeepDrill)
				{
					return "TooCloseToMapEdge".Translate();
				}
				iterator.MoveNext();
			}
			if (center.Fogged())
			{
				return "CannotPlaceInUndiscovered".Translate();
			}
			List<Thing> thingList = center.GetThingList();
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
				IntVec3 c = Thing.InteractionCellWhenAt(thingDef, center, rot);
				if (!c.InBounds())
				{
					return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
				}
				List<Thing> list = Find.ThingGrid.ThingsListAtFast(c);
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
					if (current2.InBounds())
					{
						thingList = current2.GetThingList();
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
										goto IL_364;
									}
									thingDef3 = thingDef2;
								}
								else
								{
									thingDef3 = thing2.def;
								}
								if (thingDef3.hasInteractionCell && cellRect.Contains(Thing.InteractionCellWhenAt(thingDef3, thing2.Position, thing2.Rotation)))
								{
									return new AcceptanceReport("WouldBlockInteractionSpot".Translate(entDef.label, thingDef3.label).CapitalizeFirst());
								}
							}
							IL_364:;
						}
					}
				}
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				if (Find.TerrainGrid.TerrainAt(center) == terrainDef)
				{
					return new AcceptanceReport("TerrainIsAlready".Translate(terrainDef.label));
				}
				if (Find.DesignationManager.DesignationAt(center, DesignationDefOf.SmoothFloor) != null)
				{
					return new AcceptanceReport("BeingSmoothed".Translate());
				}
			}
			if (!CanBuildOnTerrain(entDef, center, rot, thingToIgnore))
			{
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			}
			if (!godMode)
			{
				CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
				while (!iterator2.Done())
				{
					thingList = iterator2.Current.GetThingList();
					for (int l = 0; l < thingList.Count; l++)
					{
						Thing thing3 = thingList[l];
						if (thing3 != thingToIgnore)
						{
							if (!CanPlaceBlueprintOver(entDef, thing3.def))
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
					AcceptanceReport result = entDef.PlaceWorkers[m].AllowsPlacing(entDef, center, rot);
					if (!result.Accepted)
					{
						return result;
					}
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		public static bool CanBuildOnTerrain(BuildableDef entDef, IntVec3 c, Rot4 rot, Thing thingToIgnore = null)
		{
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null && !c.GetTerrain().changeable)
			{
				return false;
			}
			CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
			cellRect.ClipInsideMap();
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				TerrainDef terrainDef2 = Find.TerrainGrid.TerrainAt(iterator.Current);
				if (!terrainDef2.affordances.Contains(entDef.terrainAffordanceNeeded))
				{
					return false;
				}
				List<Thing> thingList = iterator.Current.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] != thingToIgnore)
					{
						TerrainDef terrainDef3 = thingList[i].def.entityDefToBuild as TerrainDef;
						if (terrainDef3 != null && !terrainDef3.affordances.Contains(entDef.terrainAffordanceNeeded))
						{
							return false;
						}
					}
				}
				iterator.MoveNext();
			}
			return true;
		}

		public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef)
		{
			if (oldDef.EverHaulable)
			{
				return true;
			}
			TerrainDef terrainDef = newDef as TerrainDef;
			if (terrainDef != null)
			{
				if (oldDef.category == ThingCategory.Building && !terrainDef.affordances.Contains(oldDef.terrainAffordanceNeeded))
				{
					return false;
				}
				if ((oldDef.IsBlueprint || oldDef.IsFrame) && !terrainDef.affordances.Contains(oldDef.entityDefToBuild.terrainAffordanceNeeded))
				{
					return false;
				}
			}
			ThingDef thingDef = newDef as ThingDef;
			BuildableDef buildableDef = BuiltDefOf(oldDef);
			ThingDef thingDef2 = buildableDef as ThingDef;
			if (oldDef == ThingDefOf.SteamGeyser && !newDef.ForceAllowPlaceOver(oldDef))
			{
				return false;
			}
			if (oldDef.category == ThingCategory.Plant && oldDef.passability == Traversability.Impassable && thingDef != null && thingDef.category == ThingCategory.Building && !thingDef.building.canPlaceOverImpassablePlant)
			{
				return false;
			}
			if (oldDef.category == ThingCategory.Building || oldDef.IsBlueprint || oldDef.IsFrame)
			{
				if (thingDef != null)
				{
					if (!thingDef.IsEdifice())
					{
						return (oldDef.building == null || oldDef.building.canBuildNonEdificesUnder) && (!thingDef.EverTransmitsPower || !oldDef.EverTransmitsPower);
					}
					if (thingDef.IsEdifice() && oldDef != null && oldDef.category == ThingCategory.Building && !oldDef.IsEdifice())
					{
						return thingDef.building == null || thingDef.building.canBuildNonEdificesUnder;
					}
					if (thingDef2 != null && thingDef2 == ThingDefOf.Wall && thingDef.building != null && thingDef.building.canPlaceOverWall)
					{
						return true;
					}
					if (newDef != ThingDefOf.PowerConduit && buildableDef == ThingDefOf.PowerConduit)
					{
						return true;
					}
				}
				return (newDef is TerrainDef && buildableDef is ThingDef && ((ThingDef)buildableDef).CoexistsWithFloors) || (buildableDef is TerrainDef && !(newDef is TerrainDef));
			}
			return true;
		}

		public static BuildableDef BuiltDefOf(ThingDef def)
		{
			return (def.entityDefToBuild == null) ? def : def.entityDefToBuild;
		}

		[StaticConstructorOnStartup]
		public class SunLampPlanDesignatorAdd : Designator
		{
			static SunLampPlanDesignatorAdd()
			{
				var resolvedDesignatorGetter = Utils.GetFieldAccessor<DesignationCategoryDef, List<Designator>>("resolvedDesignators");
				var orders = DefDatabase<DesignationCategoryDef>.AllDefs.FirstOrDefault(def => def.defName == "Orders");
				resolvedDesignatorGetter(orders).Add(new SunLampPlanDesignatorAdd());
			}

			private readonly DesignationDef desDef = DesignationDefOf.Plan;

			public SunLampPlanDesignatorAdd()
			{
				this.soundDragSustain = SoundDefOf.DesignateDragStandard;
				this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
				this.useMouseIcon = true;
				this.desDef = DesignationDefOf.Plan;
				this.defaultLabel = "Sun Lamp Plan";
				this.defaultDesc = "Place planning designations in the shape of a sun lamp radius";
				this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn");
				this.soundSucceeded = SoundDefOf.DesignatePlanAdd;
			}

			public override AcceptanceReport CanDesignateCell(IntVec3 c)
			{
				if (!c.InBounds())
				{
					return false;
				}
				if (c.InNoBuildEdgeArea())
				{
					return "TooCloseToMapEdge".Translate();
				}
				
				return true;
			}

			public override void DesignateSingleCell(IntVec3 c)
			{
				//Find.DesignationManager.AddDesignation(new Designation(c, this.desDef));
				foreach (var cell in GenRadial.RadialCellsAround(c, 5.8f, true))
				{
					if (Find.DesignationManager.DesignationAt(cell, this.desDef) == null)
					{
						Find.DesignationManager.AddDesignation(new Designation(cell, this.desDef));
					}
				}

			}

			public override void SelectedUpdate()
			{
				GenUI.RenderMouseoverBracket();
				GenDraw.DrawNoBuildEdgeLines();
				if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
				{
					IntVec3 intVec = Gen.MouseCell();
					Color ghostCol;
					if (this.CanDesignateCell(intVec).Accepted)
					{
						ghostCol = new Color(0.5f, 1f, 0.6f, 0.4f);
					}
					else
					{
						ghostCol = new Color(1f, 0f, 0f, 0.4f);
					}
					GenDraw.DrawRadiusRing(Gen.MouseCell(), 5.8f);
				}
			}

			public override void DrawMouseAttachments()
			{
				var intVec = Gen.MouseCell();
				float totalFertility = 0;
				foreach (var cell in GenRadial.RadialCellsAround(intVec, 5.8f, true))
				{
					var fertility = Find.FertilityGrid.FertilityAt(cell);
					if (fertility >= 0.4)
					{
						Vector3 v = GenWorldUI.LabelDrawPosFor(cell);
						GenWorldUI.DrawThingLabel(v, fertility.ToString(), FertilityColor(fertility));
					}
					totalFertility += fertility;
				}
				var avgFertility = totalFertility / GenRadial.NumCellsInRadius(5.8f);
				Text.Font = GameFont.Medium;
				Rect rect = new Rect(Event.current.mousePosition.x + 19f, Event.current.mousePosition.y + 19f, 100f, 100f);
				GUI.color = FertilityColor(avgFertility);
				Widgets.Label(rect, avgFertility.ToString("F2"));
				GUI.color = Color.white;
				GenUI.DrawMouseAttachment(null, string.Empty);
			}

			private static readonly Color ColorInfertile = Color.red;

			private static readonly Color ColorFertile = Color.green;

			private static Color FertilityColor(float fertility)
			{
				float num = Mathf.InverseLerp(0, 1.4f, fertility);
				num = Mathf.Clamp01(num);
				return Color.Lerp(ColorInfertile, ColorFertile, num);
			}
		}
	}
}
