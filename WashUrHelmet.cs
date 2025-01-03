
using System.Linq;

using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;


namespace WashUrHelmet
{
    public sealed class WashUrHelmet : Mod
    {
        public override string ID => "WashUrHelmet";
        public override string Name => "WashUrHelmet";
        public override string Author => "Andrinall";
        public override string Version => "1.0";
        public override string Description => "This mod allows you to wash the smudge off your helmet visor.";
        public override byte[] Icon => Properties.Resources.Icon;


        public static float SmoothWashingSpeed = 0.4f;


        internal static GameObject helmetItem;
        internal static GameObject helmetStain;
        internal static FsmBool helmetStainRandomizeDone;
        internal static FsmBool playerHelmet;
        FsmBool rainRoofCheck;

        static Transform player;
        static GameObject swimming;
        static GameObject rainParticle;
        static Material stainParticle;
        static Color originalColor;


        public static bool IsStainVisible()
            => helmetStainRandomizeDone?.Value == true;

        public static void StainWash()
        {
            if (helmetStainRandomizeDone == null) return;
            if (!IsStainVisible()) return;

            helmetStainRandomizeDone.Value = false;
            helmetStain.SetActive(false);
        }

        public static void SmoothStainWash()
        {
            if (!IsStainVisible()) return;

            stainParticle.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                Mathf.Clamp(stainParticle.color.a - SmoothWashingSpeed * Time.deltaTime, 0, 1)
            );

            if (stainParticle.color.a < 0.15f)
            {
                StainWash();
                stainParticle.color = originalColor;
            }
        }

        bool IsPlayerInNoRainArea()
            => rainRoofCheck?.Value == true;

        public override void ModSetup()
        {
            SetupFunction(Setup.PostLoad, Mod_PostLoad);
            SetupFunction(Setup.OnMenuLoad, Mod_OnMenuLoad);
            SetupFunction(Setup.Update, Mod_Update);
        }

        void Mod_PostLoad()
        {
            helmetItem = (
                GameObject.Find("helmet(itemx)")
                ?? GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Helmet/HelmetPivot/helmet(itemx)")
                ?? GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/ItemPivot/helmet(itemx)")
            );

            helmetStain = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Helmet/Stains/Stain");
            helmetStainRandomizeDone = helmetStain.GetComponent<PlayMakerFSM>().GetVariable<FsmBool>("Done");

            player = GameObject.Find("PLAYER").transform;
            playerHelmet = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerHelmet");
            swimming = player.GetChild(6).gameObject;

            rainParticle = GameObject.Find("PLAYER/Rain/Particle");
            stainParticle = helmetStain.GetComponent<MeshRenderer>().material;
            originalColor = stainParticle.color;

            GameObject.Find("YARD/Building/BATHROOM/Shower/TapDrink")?.AddComponent<WashDistanceChecker>();
            GameObject.Find("YARD/Building/BATHROOM/Shower/ShowerParticle")?.AddComponent<WashDistanceChecker>();
            GameObject.Find("YARD/Building/KITCHEN/KitchenWaterTap/ParticleDrink")?.AddComponent<WashDistanceChecker>();

            Transform[] _waterWells = Resources.FindObjectsOfTypeAll<Transform>().Where(v => v != null && v.name == "WaterWell").ToArray();
            foreach (Transform _well in _waterWells)
                _well?.Find("Functions/ParticleDrink")?.gameObject?.AddComponent<WashDistanceChecker>();

            rainRoofCheck = GameObject.Find("PLAYER/Rain").GetPlayMaker("RoofCheck").GetVariable<FsmBool>("RoofCheck");
        }

        void Mod_OnMenuLoad()
        {
            helmetItem = null;
            helmetStain = null;
            helmetStainRandomizeDone = null;
            playerHelmet = null;
            swimming = null;
            rainParticle = null;
            stainParticle = null;
            originalColor = Color.white;
        }

        void Mod_Update()
        {
            if (!playerHelmet.Value) return;
            if (rainParticle.activeSelf && !IsPlayerInNoRainArea())
            {
                SmoothStainWash();
                return;
            }

            if (swimming?.activeSelf == true)
                SmoothStainWash();
        }
    }
}
