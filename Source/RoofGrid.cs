using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	static class _RoofGrid
	{

		private static ThickRoofDrawer thickRoofDrawer;

		public static readonly Func<RoofGrid, ushort[]> roofGridGetter = Utils.GetFieldAccessor<RoofGrid, ushort[]>("roofGrid");

		private static readonly Func<RoofGrid, CellBoolDrawer> drawerGetter = Utils.GetFieldAccessor<RoofGrid, CellBoolDrawer>("drawerInt");

		private static readonly Func<RoofGrid, Map> mapGet = Utils.GetFieldAccessor<RoofGrid, Map>("map");

		private static readonly Func<FogGrid, Map> fogMapGet = Utils.GetFieldAccessor<FogGrid, Map>("map");


		[DetourClassMethod(typeof(RoofGrid))]
		public static bool GetCellBool(this RoofGrid @this, int index)
		{
			var roofGrid = roofGridGetter(@this);
			return roofGrid[index] != 0 && (!mapGet(@this).fogGrid.IsFogged(index) || !DebugViewSettings.drawFog)
				&& roofGrid[index] != RoofDefOf.RoofRockThick.shortHash;
		}

		[DetourClassMethod(typeof(RoofGrid))]
		public static void RoofGridUpdate(this RoofGrid @this)
		{
			if (drawerGetter(@this) == null)
			{
				thickRoofDrawer = new ThickRoofDrawer(@this,mapGet(@this));
			}
			if (Find.PlaySettings.showRoofOverlay)
			{
				thickRoofDrawer.drawer.MarkForDraw();
				@this.Drawer.MarkForDraw();
			}
			thickRoofDrawer.drawer.CellBoolDrawerUpdate();
			@this.Drawer.CellBoolDrawerUpdate();
		}

		[DetourClassMethod(typeof(FogGrid))]
		public static void Unfog(this FogGrid @this, IntVec3 c)
		{
			var map = fogMapGet(@this);
			int num = fogMapGet(@this).cellIndices.CellToIndex(c);
			if (!@this.fogGrid[num])
			{
				return;
			}
			@this.fogGrid[num] = false;
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.mapDrawer.MapMeshDirty(c, MapMeshFlag.FogOfWar);
			}
			Designation designation = map.designationManager.DesignationAt(c, DesignationDefOf.Mine);
			if (designation != null && MineUtility.MineableInCell(c, map) == null)
			{
				designation.Delete();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.roofGrid.Drawer.SetDirty();
				thickRoofDrawer.drawer.SetDirty();
			}
		}

		private class ThickRoofDrawer : ICellBoolGiver
		{
			private readonly RoofGrid parent;

			private readonly Map map;

			public CellBoolDrawer drawer;

			public ThickRoofDrawer(RoofGrid par, Map map)
			{
				parent = par;
				this.map = map;
				drawer = new CellBoolDrawer(this, this.map.Size.x, this.map.Size.z);
			}

			public Color Color => new Color(0.3f, 0.7f, 0.4f);

			public bool GetCellBool(int index)
			{
				return roofGridGetter(parent)[index] == RoofDefOf.RoofRockThick.shortHash && !map.fogGrid.IsFogged(index);
			}
		}
	}
}
