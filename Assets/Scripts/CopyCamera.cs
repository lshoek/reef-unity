using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Camera))]
    public class CopyCamera : MonoBehaviour
    {
        private Camera targetCamera;
        public Camera TargetCamera { get { return targetCamera; } set { targetCamera = value; } }

        private Camera thisCamera;

        private bool initialized = false;
        private float initialDepth;

        [SerializeField] bool AdjustAspect = false;

        public CopyCamera(Camera target)
        {
            targetCamera = target;
        }

        void Awake()
        {
            thisCamera = GetComponent<Camera>();
        }

        public void Initialize(Camera target)
        {
            if (!initialized)
            {
                targetCamera = target;
                thisCamera.CopyFrom(targetCamera);
                thisCamera.clearFlags = CameraClearFlags.Depth;
                thisCamera.backgroundColor = targetCamera.backgroundColor;
                thisCamera.depth = targetCamera.depth+1;
                thisCamera.enabled = true;
                initialized = true;
            }
        } 

        void LateUpdate()
        {
            if (initialized)
            {
                transform.position = targetCamera.transform.position;
                transform.rotation = targetCamera.transform.rotation;

                if (AdjustAspect)
                {
                    thisCamera.aspect = (float)thisCamera.targetTexture.width / thisCamera.targetTexture.height;

                    // this line will for now only work in a perfect world where any camera aspect ratio matches that of the display
                    thisCamera.fieldOfView = targetCamera.fieldOfView;
                }
                else
                {
                    thisCamera.projectionMatrix = targetCamera.projectionMatrix;
                }
            }
        }
    }
}