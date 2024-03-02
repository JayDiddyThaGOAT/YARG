using UnityEngine;

namespace YARG.MultiDisplay
{

    public class ConvertToWorldSpaceOnStart : MonoBehaviour
    {
        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
        }
    }
}
