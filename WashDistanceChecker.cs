
using UnityEngine;


namespace WashUrHelmet
{
    internal class WashDistanceChecker : MonoBehaviour
    {
        Transform helmet;
        Transform camera;

        bool isShowerParticle = false;
        float smoothDistance = 0.35f;
        
        const float mainDistance = 0.5f;

        void Awake()
        {
            helmet = WashUrHelmet.helmetItem?.transform;
            camera = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("POV").Value.transform;

            isShowerParticle = gameObject.name == "ShowerParticle";
            smoothDistance = isShowerParticle ? mainDistance : 0.35f;

            if (helmet == null)
                Destroy(this);
        }

        void Update()
        {
            if (!WashUrHelmet.IsStainVisible()) return;
            if (WashUrHelmet.playerHelmet.Value)
            {
                if (Vector3.Distance(transform.position, camera.position) > smoothDistance) return;

                WashUrHelmet.SmoothStainWash();
                return;
            }

            if (Vector3.Distance(transform.position, helmet.position) > mainDistance) return;
            WashUrHelmet.StainWash();
        }
    }
}
