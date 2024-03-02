using UnityEngine;

namespace YARG.MultiDisplay
{
    public class MultiDisplayRenderTexture : MonoBehaviour
    {
        public static RenderTexture TargetTexture { get; private set; }

        private Camera _renderCamera;

        private void OnEnable()
        {
            var descriptor = new RenderTextureDescriptor(
                Screen.width, Screen.height,
                RenderTextureFormat.ARGBHalf
            )
            {
                mipCount = 0,
            };

            // Create render texture
            TargetTexture = new RenderTexture(descriptor);

            _renderCamera = GetComponent<Camera>();
            _renderCamera.targetTexture = TargetTexture;
        }

        private void OnDisable()
        {
            if (TargetTexture == null)
            {
                return;
            }

            TargetTexture.Release();
            TargetTexture = null;
        }
    }
}
