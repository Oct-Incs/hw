using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace HorizonWalkerFreeCam
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGuid = "oct-incs.horizonwalker.freecam";
        public const string PluginName = "Horizon Walker Free Camera";
        public const string PluginVersion = "1.1.0";

        internal static ConfigEntry<KeyCode> ToggleKey;
        internal static ConfigEntry<float> MoveSpeed;
        internal static ConfigEntry<float> SprintMultiplier;
        internal static ConfigEntry<float> MouseSensitivity;
        internal static ConfigEntry<bool> InvertY;
        internal static ConfigEntry<bool> LockCursorWhileActive;

        public override void Load()
        {
            ToggleKey = Config.Bind(
                "General", "ToggleKey", KeyCode.F9,
                "フリーカメラのON/OFFを切り替えるキー");

            MoveSpeed = Config.Bind(
                "Movement", "MoveSpeed", 10f,
                "フリーカメラの移動速度 (単位/秒)");

            SprintMultiplier = Config.Bind(
                "Movement", "SprintMultiplier", 3f,
                "Shift 押下時の移動速度倍率");

            MouseSensitivity = Config.Bind(
                "Look", "MouseSensitivity", 2.5f,
                "マウス視点操作の感度");

            InvertY = Config.Bind(
                "Look", "InvertY", false,
                "マウスの上下方向を反転する");

            LockCursorWhileActive = Config.Bind(
                "General", "LockCursorWhileActive", true,
                "フリーカメラ有効時にマウスカーソルをロックする");

            // IL2CPPでは独自MonoBehaviourを使う前にIl2Cpp型として登録する必要がある
            ClassInjector.RegisterTypeInIl2Cpp<FreeCamController>();

            var go = new GameObject(PluginName);
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent(Il2CppType.Of<FreeCamController>());

            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Toggle key: {ToggleKey.Value}");
        }
    }
}
