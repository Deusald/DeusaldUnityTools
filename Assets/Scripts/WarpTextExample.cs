using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace DeusaldUnityTools
{
    public class WarpTextExample : MonoBehaviour
    {
        [SerializeField] private TMP_Text       _Text;
        [SerializeField] public  AnimationCurve _VertexCurve;
        [SerializeField] public  float          _YCurveScaling = 100f;
        
        private void Start()
        {
            Warp();
        }

        [Button]
        private void Warp()
        {
            _Text.WarpText(_VertexCurve, _YCurveScaling);
        }
    }
}