using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	static class _RoofGrid
	{
		public static readonly Func<RoofGrid, ushort[]> roofGridGetter = Utils.GetFieldAccessor<RoofGrid, ushort[]>("roofGrid");

		private static readonly Func<RoofGrid, Map> mapGet = Utils.GetFieldAccessor<RoofGrid, Map>("map");

		private static readonly Func<FogGrid, Map> fogMapGet = Utils.GetFieldAccessor<FogGrid, Map>("map");

		[DetourMember]
		public static bool GetCellBool(this RoofGrid @this, int index)
		{
			var roofGrid = roofGridGetter(@this);
			return roofGrid[index] != 0 && (!mapGet(@this).fogGrid.IsFogged(index) || !DebugViewSettings.drawFog)
				&& roofGrid[index] != RoofDefOf.RoofRockThick.shortHash;
		}

		[DetourMember]
		public static void Unfog(this FogGrid @this, IntVec3 c)
		{
			var map = fogMapGet(@this);
			int num = map.cellIndices.CellToIndex(c);
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
				map.GetComponent<ThickRoofDrawer>().Drawer.SetDirty();
			}
		}

		private class ThickRoofDrawer : MapComponent, ICellBoolGiver
		{
			private CellBoolDrawer drawerInt;

			public CellBoolDrawer Drawer => drawerInt ?? (drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z));

			public ThickRoofDrawer(Map map) : base(map)
			{
				LongEventHandler.ExecuteWhenFinished(() => { if (map.GetComponent<ThickRoofDrawer>() == null) { map.components.Add(this); } });
			}

			public Color Color => new Color(0.3f, 0.7f, 0.4f);

			public bool GetCellBool(int index)
			{
				return roofGridGetter(map.roofGrid)[index] == RoofDefOf.RoofRockThick.shortHash && !map.fogGrid.IsFogged(index);
			}

			public override void MapComponentUpdate()
			{
				if (Find.PlaySettings.showRoofOverlay)
				{
					Drawer.MarkForDraw();
				}
				Drawer.CellBoolDrawerUpdate();
			}
		}
	}
}
