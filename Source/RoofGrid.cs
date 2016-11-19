using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	static class RoofGrid
	{

		private static ThickRoofDrawer thickRoofDrawer;

		public static readonly Func<Verse.RoofGrid, ushort[]> roofGridGetter = Utils.GetFieldAccessor<Verse.RoofGrid, ushort[]>("roofGrid");

		private static readonly Func<Verse.RoofGrid, CellBoolDrawer> drawerGetter = Utils.GetFieldAccessor<Verse.RoofGrid, CellBoolDrawer>("drawerInt");

		[DetourClassMethod(typeof(Verse.RoofGrid))]
		public static bool GetCellBool(this Verse.RoofGrid @this, int index)
		{
			var roofGrid = roofGridGetter(@this);
			return roofGrid[index] != 0 && (!Find.FogGrid.IsFogged(index) || !DebugViewSettings.drawFog)
				&& roofGrid[index] != RoofDefOf.RoofRockThick.shortHash;
		}

		[DetourClassMethod(typeof(Verse.RoofGrid))]
		public static void RoofGridUpdate(this Verse.RoofGrid @this)
		{
			if (drawerGetter(@this) == null)
			{
				thickRoofDrawer = new ThickRoofDrawer(@this);
			}
			if (Find.PlaySettings.showRoofOverlay)
			{
				thickRoofDrawer.drawer.MarkForDraw();
				@this.Drawer.MarkForDraw();
			}
			thickRoofDrawer.drawer.CellBoolDrawerUpdate();
			@this.Drawer.CellBoolDrawerUpdate();
		}

		[DetourClassMethod(typeof(Verse.FogGrid))]
		public static void Unfog(this FogGrid @this, IntVec3 c)
		{
			int num = CellIndices.CellToIndex(c);
			if (!@this.fogGrid[num])
			{
				return;
			}
			@this.fogGrid[num] = false;
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.FogOfWar);
			}
			Designation designation = Find.DesignationManager.DesignationAt(c, DesignationDefOf.Mine);
			if (designation != null && MineUtility.MineableInCell(c) == null)
			{
				designation.Delete();
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.RoofGrid.Drawer.SetDirty();
				thickRoofDrawer.drawer.SetDirty();
			}
		}

		private class ThickRoofDrawer : ICellBoolGiver
		{
			private readonly Verse.RoofGrid parent;

			public CellBoolDrawer drawer;

			public ThickRoofDrawer(Verse.RoofGrid par)
			{
				parent = par;
				drawer = new CellBoolDrawer(this);
			}


			public Color Color => new Color(0.3f, 0.7f, 0.4f);

			public bool GetCellBool(int index)
			{
				return roofGridGetter(parent)[index] == RoofDefOf.RoofRockThick.shortHash && !Find.FogGrid.IsFogged(index);
			}
		}
	}
}
