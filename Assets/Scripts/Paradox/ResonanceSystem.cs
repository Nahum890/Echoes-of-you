using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de Resonancia: Activa una señal de puzzle cuando el jugador y/o sus ecos 
/// ocupan múltiples zonas de resonancia simultáneamente.
/// </summary>
public class ResonanceSystem : MonoBehaviour, IResettableLevelObject
{
    [System.Serializable]
    public class ResonanceZone
    {
        public Collider triggerCollider;
        public Renderer zoneRenderer;
        public Color inactiveColor = new Color(0.1f, 0.2f, 0.3f, 0.5f);
        public Color activeColor = new Color(0.2f, 0.8f, 1f, 0.9f);
        
        [HideInInspector] public HashSet<Collider> occupants = new HashSet<Collider>();
        [HideInInspector] public Material material;

        public bool IsOccupied => occupants.Count > 0;
    }

    [SerializeField] List<ResonanceZone> zones = new List<ResonanceZone>();
    [SerializeField] PuzzleSignal targetSignal;
    [SerializeField] int requiredActiveZones = 2;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        foreach (var zone in zones)
        {
            if (zone.zoneRenderer != null)
            {
                zone.material = zone.zoneRenderer.material;
                zone.material.EnableKeyword("_EMISSION");
            }
            UpdateZoneVisuals(zone);
        }
    }

    void Start()
    {
        EvaluateResonance();
    }

    public void RegisterOccupant(Collider zoneCollider, Collider occupant)
    {
        var zone = FindZone(zoneCollider);
        if (zone != null)
        {
            if (occupant.CompareTag("Player") || occupant.CompareTag("Echo"))
            {
                zone.occupants.Add(occupant);
                UpdateZoneVisuals(zone);
                EvaluateResonance();
            }
        }
    }

    public void UnregisterOccupant(Collider zoneCollider, Collider occupant)
    {
        var zone = FindZone(zoneCollider);
        if (zone != null)
        {
            if (zone.occupants.Remove(occupant))
            {
                UpdateZoneVisuals(zone);
                EvaluateResonance();
            }
        }
    }

    private ResonanceZone FindZone(Collider col)
    {
        return zones.Find(z => z.triggerCollider == col);
    }

    private void EvaluateResonance()
    {
        int activeCount = 0;
        foreach (var zone in zones)
        {
            if (zone.IsOccupied) activeCount++;
        }

        bool satisfied = activeCount >= requiredActiveZones;
        if (targetSignal != null)
        {
            if (targetSignal.IsSatisfied != satisfied)
            {
                targetSignal.SetSatisfied(satisfied);
                if (satisfied)
                {
                    GameFeelController.Instance?.PlayMechanicTick(transform.position, 1.2f);
                }
            }
        }
    }

    private void UpdateZoneVisuals(ResonanceZone zone)
    {
        if (zone.material == null) return;

        bool active = zone.IsOccupied;
        zone.material.SetColor(ColorId, active ? zone.activeColor : zone.inactiveColor);
        zone.material.SetColor(EmissionColorId, active ? zone.activeColor * 1.5f : zone.inactiveColor * 0.1f);
    }

    public void ResetLevelState()
    {
        foreach (var zone in zones)
        {
            zone.occupants.Clear();
            UpdateZoneVisuals(zone);
        }
        EvaluateResonance();
    }
}
