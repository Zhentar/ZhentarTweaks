using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ZhentarTweaks.Source
{
	class JobGiver_OptimizeApparel
	{
		private static Func<NeededWarmth> neededWarmthGetter;

		static JobGiver_OptimizeApparel()
		{
			neededWarmthGetter = Utils.GetStaticFieldAccessor<RimWorld.JobGiver_OptimizeApparel, NeededWarmth>("neededWarmth");
		}

		[DetourClassMethod(typeof(RimWorld.JobGiver_OptimizeApparel))]
		public static float ApparelScoreRaw(Apparel ap)
		{
			float num = 0.2f;
			float num2 = ap.GetStatValue(StatDefOf.ArmorRating_Sharp) + ap.GetStatValue(StatDefOf.ArmorRating_Blunt) * 0.75f;
			num2 += ap.GetStatValue(StatDefOf.PersonalShieldEnergyMax) * .005f;
			num += num2;
			if (ap.def.useHitPoints)
			{
				float x = ap.HitPoints / (float)ap.MaxHitPoints;
				num *= HitPointsPercentScoreFactorCurve.Evaluate(x); ;
			}
			float num3 = 1f;
			if (neededWarmthGetter() == NeededWarmth.Warm)
			{
				float statValueAbstract = ap.GetStatValue(StatDefOf.Insulation_Cold);
				num3 *= InsulationColdScoreFactorCurve_NeedWarm.Evaluate(statValueAbstract);
			}
			return num * num3;
		}

		private static readonly SimpleCurve InsulationColdScoreFactorCurve_NeedWarm = new SimpleCurve
		{
			new CurvePoint(-40f, 6f),
			new CurvePoint(0f, 1f)
		};

		private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(0.25f, 0.15f),
			new CurvePoint(0.5f, 0.7f),
			new CurvePoint(1f, 1f)
		};
	}
}
