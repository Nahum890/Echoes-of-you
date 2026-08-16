using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class VisualStateChange
    {
        [Header("Material")]
        [SerializeField] bool _changeEmission = false;
        [SerializeField] float _emissionIntensity = 1.0f;
        [SerializeField] bool _changeColor = false;
        [SerializeField] Color _targetColor = Color.white;

        [Header("Visibility")]
        [SerializeField] bool _disableRenderer = false;
        [SerializeField] bool _disableObject = false;

        [Header("Scale")]
        [SerializeField] bool _changeScale = false;
        [SerializeField] Vector3 _targetScale = Vector3.one;

        public bool ChangeEmission => _changeEmission;
        public float EmissionIntensity => _emissionIntensity;
        public bool ChangeColor => _changeColor;
        public Color TargetColor => _targetColor;
        public bool DisableRenderer => _disableRenderer;
        public bool DisableObject => _disableObject;
        public bool ChangeScale => _changeScale;
        public Vector3 TargetScale => _targetScale;
    }
}
