using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarTweaks
{
	public class CompTaintedIngredients : CompIngredients
	{

		public override void PostDraw()
		{
			if (ContainsHumanlikeMeat || ContainsInsectMeat)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayDrawerDetour.SentinalOverlayType);
			}
		}

		public bool ContainsHumanlikeMeat => ingredients.Any(FoodUtility.IsHumanlikeMeat);

		public bool ContainsInsectMeat => ingredients.Any(td => td.ingestible.specialThoughtAsIngredient == ThoughtDefOf.AteInsectMeatAsIngredient);
	}

	[StaticConstructorOnStartup]
	public class CompProperties_TaintedIngredients : CompProperties
	{
		static CompProperties_TaintedIngredients()
		{
			//Detour the constructor to get defs that haven't been loaded yet
			ConstructorInfo method1 = typeof(CompProperties_Ingredients).GetConstructor( new Type[] {});
			ConstructorInfo method2 = typeof(CompProperties_TaintedIngredients).GetConstructor(new Type[] {});
			if (!Detours.TryDetourFromToInt(method1, method2)) { Log.Error("YOU FAILED"); }

			//Modify the defs that have already been loaded
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
			{
				var compDef = def.CompDefFor<CompIngredients>();
				if (compDef != null) { compDef.compClass = typeof(CompTaintedIngredients); }
			}

		}

		public CompProperties_TaintedIngredients()
		{
			compClass = typeof(CompTaintedIngredients);
		}
	}

	[StaticConstructorOnStartup]
	public class OverlayDrawerDetour : OverlayDrawer
	{
		public const OverlayTypes SentinalOverlayType = OverlayTypes.ForbiddenBig;

		private static readonly float BaseAlt;

		private static Material ForbiddenMat;

		private static Material HumanlikeMat;

		private static Material InsectMat;

		private static Material BothMat;

		static OverlayDrawerDetour()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				ForbiddenMat = MaterialPool.MatFrom("Things/Special/ForbiddenOverlay", ShaderDatabase.MetaOverlay);
				HumanlikeMat = MaterialPool.MatFrom("humanlike", ShaderDatabase.MetaOverlay);
				InsectMat = MaterialPool.MatFrom("insect", ShaderDatabase.MetaOverlay);
				BothMat = MaterialPool.MatFrom("humanlikeinsect", ShaderDatabase.MetaOverlay);
			});
			BaseAlt = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
		}

		[DetourMember]
		private void RenderForbiddenBigOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = BaseAlt + 0.2f;

			var taintComp = t.TryGetComp<CompTaintedIngredients>();
			if ((taintComp?.ContainsHumanlikeMeat).GetValueOrDefault())
			{
				Graphics.DrawMesh(MeshPool.plane05, drawPos, Quaternion.identity,
					taintComp.ContainsInsectMeat ? BothMat : HumanlikeMat, 0);
				return;
			}
			if ((taintComp?.ContainsInsectMeat).GetValueOrDefault())
			{
				Graphics.DrawMesh(MeshPool.plane05, drawPos, Quaternion.identity, InsectMat, 0);
				return;
			}
			
			Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, ForbiddenMat, 0);
		}
	}
}
