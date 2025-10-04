using HarmonyLib;
using Il2Cpp;
using Il2CppKeepsake;
using Il2CppKeepsake.HyperSpace.GameplayFeatures.FoodProcessor;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(NoPineappleOnPizzaMod.Core), "NoPineappleOnPizza", "1.0.0", "Scribble", null)]
[assembly: MelonGame("Keepsake Games", "Jump Space")]

namespace NoPineappleOnPizzaMod
{
    public class Core : MelonMod
    {

        private static float reloadMessageStart = -1f;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Mod initialized.");
        }

        public override void OnLateUpdate()
        {

            //if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
            //{
            //    MelonLogger.Msg("Manual reload triggered.");
            //    MelonCoroutines.Start(MeshSwap.DelayedActivate(3f));
            //    reloadMessageStart = Time.time;
            //    MelonEvents.OnGUI.Subscribe(DrawReloadText, 100);
            //}
        }

        public static void DrawReloadText()
        {
            if (reloadMessageStart > 0 && Time.time - reloadMessageStart < 3f)
            {
                GUI.Label(new Rect(20, 20, 500, 50),
                    "<b><color=black><size=30>Reloading...</size></color></b>");
            }
            else
            {
                MelonEvents.OnGUI.Unsubscribe(DrawReloadText);
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            //debugging - MelonLogger.Msg($"Scene initialized: {sceneName} ({buildIndex})");
            MelonCoroutines.Start(MeshSwap.DelayedActivate(3f));
        }
    }

    [HarmonyPatch]
    class PizzaStartPatchClass
    {
        private static float activate = .01f;

        [HarmonyPatch(typeof(FoodProcessorInteraction), nameof(FoodProcessorInteraction.OnSliceSpawned))]
        [HarmonyPostfix]
        public static void OnPizzaSpawned(PickupableItem_Dropped itemDropped)
        {
            if (itemDropped.name.Contains("Pizza"))
            {
                MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
            }
        }

        [HarmonyPatch(typeof(PlayerPickupableItemHandler), nameof(PlayerPickupableItemHandler.DropItemInternal))]
        [HarmonyPostfix]
        public static void OnPizzaDropped(PersistentPickupable persistentPickupable)
        {
            if (persistentPickupable.name.Contains("Pizza"))
            {
                MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
            }
        }

        [HarmonyPatch(typeof(PlayerPickupableItemHandler), nameof(PlayerPickupableItemHandler.HoldItem))]
        [HarmonyPostfix]
        public static void OnItemSwitch(PersistentPickupable persistentPickupable)
        {
            if (persistentPickupable.name.Contains("Pizza"))
            {
                MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
            }
        }

        [HarmonyPatch(typeof(PlayerPickupableItemHandler), nameof(PlayerPickupableItemHandler.PickupAndAddToInventory))]
        [HarmonyPostfix]
        public static void OnItemPickup(PersistentPickupable persistentPickupable)
        {
            if (persistentPickupable.name.Contains("Pizza"))
            {
                MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
            }
        }

        [HarmonyPatch(typeof(PlayerPickupableItemHandler), nameof(PlayerPickupableItemHandler.ReplaceItem))]
        [HarmonyPostfix]
        public static void OnItemReplace(PersistentPickupable newItem, PersistentPickupable heldItem)
        {
            if (newItem.name.Contains("Pizza") || heldItem.name.Contains("Pizza"))
            {
                MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
            }
        }

        [HarmonyPatch(typeof(PlayerPickupableItemHandler), nameof(PlayerPickupableItemHandler.RefreshHeldItem))]
        [HarmonyPostfix]
        public static void OnItemRefresh()
        {
            MelonCoroutines.Start(MeshSwap.DelayedActivate(activate));
        }
    }

    public static class MeshSwap
    {
        public static void SwapMesh()
        {
            string bundlePath = Path.Combine(
                MelonEnvironment.ModsDirectory,
                "pizza.bundle"
            );

            if (!File.Exists(bundlePath))
            {
                MelonLogger.Msg("Bundle not found: " + bundlePath);
                return;
            }

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                //debugging - MelonLogger.Error("Failed to load AssetBundle.");
                return;
            }

            Mesh customMesh = bundle.LoadAsset<Mesh>("Assets/Pizza.fbx");
            customMesh = MoveMesh(customMesh, 0.23f, -0.01f, 0.23f);
            customMesh = RotateMesh(customMesh, 0f, 45f, 0f);
            Texture2D mainTex = null;
            //Texture2D normalTex = bundle.LoadAsset<Texture2D>("Assets/NormalMap.png");

            bundle.Unload(false);

            if (customMesh == null)
            {
                //debugging - MelonLogger.Error("Custom mesh not found in bundle.");
                return;
            }

            var targets = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name == "FoodProcessor_PizzaSlice")
                .ToList();

            //MelonLogger.Msg($"Found {targets.Count} target object(s).");

            foreach (var targetObj in targets)
            {
                var smr = targetObj.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    smr.sharedMesh = customMesh;
                    var mats = smr.materials;

                    if (mainTex != null && mats[0].HasProperty("_T1"))
                        mats[0].SetTexture("_T1", mainTex);

                    /*
                    if (normalTex != null && mats[1].HasProperty("_T2"))
                        mats[0].SetTexture("_T2", normalTex);
                    */


                    /*foreach (var mat in mats)
                    {
                        if (mat.HasProperty("_T2")) mat.SetTexture("_T2", null); //Normal map
                        if (mat.HasProperty("_T3")) mat.SetTexture("_T3", null); //Emission map
                        if (mat.HasProperty("_M")) mat.SetTexture("_M", null); //Metallic map

                        if (mat.HasProperty("_ColorMainLookup")) mat.SetTexture("_ColorMainLookup", null);
                        if (mat.HasProperty("_ColorSecondaryLookup")) mat.SetTexture("_ColorSecondaryLookup", null);
                        if (mat.HasProperty("_ColorDetailLookup")) mat.SetTexture("_ColorDetailLookup", null);
                    }

                    //This is a temporary measure, certain models may be ultra reflective with their default shader:
                    Shader unlitShader = Shader.Find("Unlit/Texture");
                    if (unlitShader != null)
                    {
                        foreach (var mat in mats)
                        {
                            mat.shader = unlitShader;
                            if (bodyTex != null)
                                mat.SetTexture("_MainTex", bodyTex);
                        }
                    }*/

                    smr.materials = mats;
                }
            }
        }
        public static IEnumerator DelayedActivate(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            SwapMesh();
        }

        /// <summary>
        /// Moves the mesh without moving the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="x">Move along the X axis (Left/Right)</param>
        /// <param name="z">Move along the Z axis (Up/Down)</param>
        /// <param name="y">Move along the Y axis (Forward/Backward)</param>
        /// <returns>The moved mesh</returns>
        public static Mesh MoveMesh(Mesh mesh, float x, float z, float y)
        {
            return MoveMesh(mesh, new Vector3(x, y, z));
        }

        /// <summary>
        /// Moves the mesh without moving the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="pos">The relative position to move to</param>
        /// <returns>The moved mesh</returns>
        public static Mesh MoveMesh(Mesh mesh, Vector3 pos)
        {
            Vector3[] originalVerts = mesh.vertices;
            Vector3[] transformedVerts = new Vector3[mesh.vertices.Length];

            for (int vert = 0; vert < originalVerts.Length; vert++)
            {
                transformedVerts[vert] = pos + originalVerts[vert];
            }

            mesh.vertices = transformedVerts;
            return mesh;
        }

        /// <summary>
        /// Rotates the mesh without rotating the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="x">Rotate around the X axis (Left/Right)</param>
        /// <param name="z">Rotate around the Z axis (Up/Down)</param>
        /// <param name="y">Rotate around the Y axis (Forward/Backward)</param>
        /// <returns>The rotated mesh</returns>
        public static Mesh RotateMesh(Mesh mesh, float x, float z, float y)
        {
            Quaternion qAngle = Quaternion.Euler(x, y, z);
            return RotateMesh(mesh, qAngle);
        }

        /// <summary>
        /// Rotates the mesh without rotating the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="qAngle">The quaternion to use for rotation</param>
        /// <returns>The rotated mesh</returns>
        public static Mesh RotateMesh(Mesh mesh, Quaternion qAngle)
        {
            Vector3[] originalVerts = mesh.vertices;
            Vector3[] transformedVerts = new Vector3[mesh.vertices.Length];

            for (int vert = 0; vert < originalVerts.Length; vert++)
            {
                transformedVerts[vert] = qAngle * originalVerts[vert];
            }

            mesh.vertices = transformedVerts;
            return mesh;
        }

        /// <summary>
        /// Scales the mesh without rotating the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="x">Scale along the X axis </param>
        /// <param name="x">Scale along the X axis (Left/Right)</param>
        /// <param name="z">Scale along the Z axis (Up/Down)</param>
        /// <param name="y">Scale along the Y axis (Forward/Backward)</param>
        /// <returns>The scaled mesh</returns>
        public static Mesh ScaleMesh(Mesh mesh, float x, float z, float y)
        {
            return ScaleMesh(mesh, new Vector3(x, y, z));
        }

        /// <summary>
        /// Scales the mesh without rotating the game object
        /// </summary>
        /// <param name="mesh">The mesh to transform</param>
        /// <param name="scale">The vector used for scaling</param>
        /// <returns>The scaled mesh</returns>
        public static Mesh ScaleMesh(Mesh mesh, Vector3 scale)
        {
            Vector3[] originalVerts = mesh.vertices;
            Vector3[] transformedVerts = new Vector3[mesh.vertices.Length];

            for (int vert = 0; vert < originalVerts.Length; vert++)
            {
                Vector3 originalVertex = originalVerts[vert];
                transformedVerts[vert] = new Vector3(
                    originalVertex.x * scale.x,
                    originalVertex.y * scale.y,
                    originalVertex.z * scale.z
                    );
            }

            mesh.vertices = transformedVerts;
            return mesh;
        }
    }
}