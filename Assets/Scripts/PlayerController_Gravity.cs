using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerController partial — Gravity, ground detection, and gravity zone management.
/// </summary>
public partial class PlayerController
{
    public void RegisterGravityZone(GravityZone zone)
    {
        if (zone == null || _gravityZones.Contains(zone))
            return;

        _gravityZones.Add(zone);
    }

    public void UnregisterGravityZone(GravityZone zone)
    {
        if (zone == null)
            return;

        _gravityZones.Remove(zone);
    }

    public void ForceGravity(Vector3 worldDirection, float strength = -1f, bool playFeedback = true)
    {
        Vector3 nextGravity = SafeGravity(worldDirection, strength > 0f ? strength : gravityStrength);
        if (playFeedback && Vector3.Angle(_targetGravity, nextGravity) > 8f)
            GameFeelController.Instance?.PlayGravityShift(transform.position, -nextGravity.normalized);

        _targetGravity = nextGravity;
    }

    void UpdateTargetGravity()
    {
        GravityZone strongestZone = null;
        int highestPriority = int.MinValue;

        for (int i = _gravityZones.Count - 1; i >= 0; i--)
        {
            GravityZone zone = _gravityZones[i];
            if (zone == null)
            {
                _gravityZones.RemoveAt(i);
                continue;
            }

            if (zone.Priority >= highestPriority)
            {
                highestPriority = zone.Priority;
                strongestZone = zone;
            }
        }

        Vector3 nextGravity = strongestZone != null
            ? strongestZone.GetGravityVector()
            : SafeGravity(defaultGravityDirection, gravityStrength);

        if (Vector3.Angle(_targetGravity, nextGravity) > 8f || Mathf.Abs(_targetGravity.magnitude - nextGravity.magnitude) > 0.5f)
            GameFeelController.Instance?.PlayGravityShift(transform.position, -nextGravity.normalized);

        _targetGravity = nextGravity;
    }

    void BlendGravity(float deltaTime)
    {
        Vector3 currentDirection = _currentGravity.sqrMagnitude > 0.0001f ? _currentGravity.normalized : Vector3.down;
        Vector3 targetDirection = _targetGravity.normalized;
        float blend = DampingFactor(gravityBlendSpeed, deltaTime);
        Vector3 blendedDirection = Vector3.Slerp(currentDirection, targetDirection, blend).normalized;
        float blendedStrength = Mathf.Lerp(_currentGravity.magnitude, _targetGravity.magnitude, blend);
        _currentGravity = blendedDirection * blendedStrength;
    }

    Vector3 ResolveMovementUp(GroundProbe probe)
    {
        if (alignToGroundNormal && probe.hit.collider != null)
            return probe.hit.normal;

        return _currentUp;
    }

    GroundProbe ProbeGround(Vector3 probeUp)
    {
        Vector3 normalizedUp = probeUp.sqrMagnitude > 0.001f ? probeUp.normalized : transform.up;
        Vector3 origin = groundCheck != null
            ? groundCheck.position + normalizedUp * groundProbeRadius
            : transform.position + normalizedUp * groundProbeRadius;

        RaycastHit hit;
        bool grounded = Physics.SphereCast(
            origin,
            groundProbeRadius,
            -normalizedUp,
            out hit,
            groundProbeDistance + groundProbeRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (grounded)
            grounded = hit.distance <= groundProbeDistance + 0.02f;

        // Excluir colisiones con el propio jugador (failsafe si groundMask incluye Default)
        if (grounded && hit.collider != null && hit.collider.transform.IsChildOf(transform))
            grounded = false;

        return new GroundProbe
        {
            isGrounded = grounded,
            hit = hit
        };
    }

    void EnsureGroundCheck()
    {
        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform, false);
            groundCheck = gc.transform;
        }

        float localYOffset = -((_controller.height * 0.5f) - _controller.radius + 0.02f);
        groundCheck.localPosition = new Vector3(0f, localYOffset, 0f);
        groundCheck.localRotation = Quaternion.identity;
    }

    static Vector3 SafeGravity(Vector3 direction, float strength)
    {
        Vector3 fallback = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.down;
        return fallback * Mathf.Max(0.01f, strength);
    }
}
