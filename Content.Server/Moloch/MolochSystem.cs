using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.Station.Components;
using Content.Shared.Moloch;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

public sealed class MolochSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly HTNSystem _htnSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;


    private const float SearchRadius = 50f;
    private const float BaseWalkSpeed = 2.5f;
    private const float BaseSprintSpeed = 4.5f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MolochComponent, TransformComponent, MovementSpeedModifierComponent>();
        while (query.MoveNext(out var uid, out var moloch, out var xform, out var speedComp))
        {
            var targetGrid = FindTargetShuttle(uid, xform);
            HandleCharge(uid, moloch, speedComp, frameTime, targetGrid);
            UpdateAITarget(uid, targetGrid);

        }
    }

    private EntityUid? FindTargetShuttle(EntityUid uid, TransformComponent xform)
    {
        var mapCoords = _transform.GetMapCoordinates(uid, xform);
        var grids = _lookup.GetEntitiesInRange<MapGridComponent>(mapCoords, SearchRadius);

        EntityUid? bestTarget = null;
        var bestDistance = float.MaxValue;

        foreach (var gridUid in grids)
        {
            if (HasComp<StationDataComponent>(gridUid))
                continue;

            var gridXform = Transform(gridUid);
            var gridMapCoords = _transform.GetMapCoordinates(gridUid, gridXform);
            var distance = (mapCoords.Position - gridMapCoords.Position).Length();

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = gridUid;
            }
        }
        return bestTarget;
    }

    private void HandleCharge(
        EntityUid uid,
        MolochComponent moloch,
        MovementSpeedModifierComponent speedComp,
        float frameTime,
        EntityUid? target)
    {
        if (target == null)
        {
            if (moloch.IsCharging)
                EndCharge(uid, moloch, speedComp);
            else
                _movementSpeed.ChangeBaseSpeed(uid, speedComp, BaseWalkSpeed, BaseSprintSpeed);
            return;
        }

        moloch.ChargeTimer -= frameTime;

        if (moloch.ChargeTimer <= 0f && !moloch.IsCharging)
            StartCharge(uid, moloch, speedComp);

        if (moloch.IsCharging)
        {
            moloch.ChargeDuration -= frameTime;
            if (moloch.ChargeDuration <= 0f)
                EndCharge(uid, moloch, speedComp);
        }

    }
    private void StartCharge(EntityUid uid, MolochComponent moloch, MovementSpeedModifierComponent speedComp)
    {
        moloch.IsCharging = true;
        moloch.ChargeDuration = 1.5f;
        _movementSpeed.ChangeBaseSpeed(uid, speedComp, moloch.ChargeSpeed, moloch.ChargeSpeed);
        Dirty(uid, moloch);
    }

    private void EndCharge(EntityUid uid, MolochComponent moloch, MovementSpeedModifierComponent speedComp)
    {
        moloch.IsCharging = false;
        moloch.ChargeTimer = moloch.ChargeCooldown;
        _movementSpeed.ChangeBaseSpeed(uid, speedComp, BaseWalkSpeed, BaseSprintSpeed);
        Dirty(uid, moloch);
    }

    private void UpdateAITarget(EntityUid uid, EntityUid? targetGrid)
    {
        if (!TryComp<HTNComponent>(uid, out var htn))
            return;

        var blackboard = htn.Blackboard;

        if (targetGrid != null)
        {
            blackboard.SetValue(NPCBlackboardKeys.MoveTarget, targetGrid.Value);
        }
        else
        {
            blackboard.Remove(NPCBlackboardKeys.MoveTarget);
        }
    }
}

