using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExpandedRoofing;

public class JobDriver_PerformRoofMaintenance : JobDriver
{
    private const float MaintenanceTicks = 80f;

    private float ticksToNextMaintenance;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        var maintenance = new Toil
        {
            initAction = delegate { ticksToNextMaintenance = MaintenanceTicks; }
        };
        maintenance.tickAction = delegate
        {
            var actor = maintenance.actor;
            actor.skills.Learn(SkillDefOf.Construction, 0.2f);
            var statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed);
            ticksToNextMaintenance -= statValue;
            Log.Message($"{ticksToNextMaintenance}");
            if (!(ticksToNextMaintenance <= 0f))
            {
                return;
            }

            pawn.Map.GetComponent<RoofMaintenance_MapComponenent>().DoMaintenance(TargetA.Cell);
            actor.records.Increment(RecordDefOf.ThingsRepaired);
            actor.jobs.EndCurrentJob(JobCondition.Succeeded);
        };
        maintenance.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        maintenance.WithEffect(EffecterDefOf.RoofWork, TargetIndex.A);
        maintenance.defaultCompleteMode = ToilCompleteMode.Never;
        yield return maintenance;
    }
}