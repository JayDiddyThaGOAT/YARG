using UnityEngine;
using UnityEngine.UI;

namespace YARG.MultiDisplay
{
    public class MirrorMultiDisplayRenderTexture : MonoBehaviour
    {
        private RawImage _image;

        void Start()
        {
            _image = GetComponent<RawImage>();
            _image.texture = MultiDisplayRenderTexture.TargetTexture;
        }
    }
}
