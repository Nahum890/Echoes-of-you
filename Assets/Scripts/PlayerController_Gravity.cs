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
        {
            // Sólo usar la normal del suelo si apunta suficientemente hacia arriba.
            // Normales de paredes (>45° desde _currentUp) causan que el jugador
            // se oriente horizontalmente y "camine" sobre superficies verticales.
            float angle = Vector3.Angle(_currentUp, probe.hit.normal);
            if (angle <= 45f)
                return probe.hit.normal;
        }

        return _currentUp;
    }

    GroundProbe ProbeGround(Vector3 probeUp)
    {
        Vector3 normalizedUp = probeUp.sqrMagnitude > 0.001f ? probeUp.normalized : transform.up;
        Vector3 origin = groundCheck != null
            ? groundCheck.position + normalizedUp * groundProbeRadius
            : transform.position + normalizedUp * groundProbeRadius;

        // Usar DefaultRaycastLayers (~0 excluyendo la capa Ignore Raycast)
        // para detectar el suelo sin importar en qué capa esté configurado (e.g. Default, Ground, etc.)
        // ya que el CharacterController.isGrounded (que quitamos por el softblock) detectaba cualquier capa.
        int mask = Physics.DefaultRaycastLayers;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            groundProbeRadius,
            -normalizedUp,
            groundProbeDistance + groundProbeRadius,
            mask,
            QueryTriggerInteraction.Ignore);

        bool grounded = false;
        RaycastHit bestHit = default;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hitInfo = hits[i];
            if (hitInfo.collider == null)
                continue;

            // Ignorar colisiones con el propio jugador (ya que empezamos el cast dentro de su propia jerarquía)
            if (hitInfo.collider.transform.IsChildOf(transform))
                continue;

            // Ignorar triggers
            if (hitInfo.collider.isTrigger)
                continue;

            // Quedarse con el impacto más cercano por debajo del jugador
            if (hitInfo.distance < bestDistance)
            {
                bestDistance = hitInfo.distance;
                bestHit = hitInfo;
                grounded = true;
            }
        }

        if (grounded)
        {
            // Validar que la distancia de colisión esté dentro del rango de pisada
            grounded = bestHit.distance <= groundProbeDistance + 0.02f;

            // Excluir superficies cuya normal no sea suficientemente vertical (paredes).
            // Un ángulo >45° desde normalizedUp indica una pared, no suelo.
            if (grounded)
            {
                float slopeAngle = Vector3.Angle(normalizedUp, bestHit.normal);
                if (slopeAngle > 45f)
                    grounded = false;
            }
        }

        return new GroundProbe
        {
            isGrounded = grounded,
            hit = bestHit
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

        // Place groundCheck at the center of the capsule's bottom sphere.
        // Works correctly regardless of whether the pivot is at the feet or the center.
        float localYOffset = _controller.center.y - (_controller.height * 0.5f) + _controller.radius;
        groundCheck.localPosition = new Vector3(0f, localYOffset, 0f);
        groundCheck.localRotation = Quaternion.identity;
    }

    static Vector3 SafeGravity(Vector3 direction, float strength)
    {
        Vector3 fallback = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.down;
        return fallback * Mathf.Max(0.01f, strength);
    }
}
