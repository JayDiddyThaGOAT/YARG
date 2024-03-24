using System.Linq;
using UnityEngine;

namespace YARG
{
    public class RenderCanvasToTexture : MonoBehaviour
    {
        private void Awake()
        {
            var canvases = GetComponentsInChildren<Canvas>(true).ToList();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = GlobalVariables.Instance.RenderCamera;
                canvas.planeDistance = 1f;
            }
        }
    }
}
