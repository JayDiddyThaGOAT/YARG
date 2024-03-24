using System.Linq;
using UnityEngine;

namespace YARG
{
    public class RenderCanvasToTexture : MonoBehaviour
    {
        private void Awake()
        {
            var canvases = GetComponentsInChildren<Canvas>(true).ToList();
            canvases.ForEach(canvas => canvas.worldCamera = GlobalVariables.Instance.RenderCamera);
        }
    }
}
