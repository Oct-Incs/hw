using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace HorizonWalkerFreeCam
{
    /// <summary>
    /// ゲーム側のカメラ制御スクリプトの実装を知らなくても動くよう、
    /// 対象カメラに付いている他の Behaviour を一時的に無効化してから
    /// フリーフライ操作を乗せる方式のフリーカメラ。
    /// </summary>
    internal class FreeCamController : MonoBehaviour
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource(Plugin.PluginName);

        // IL2CPPで注入されるMonoBehaviourは通常のコンストラクタではなく
        // ネイティブポインタを受け取るこのコンストラクタ経由で生成される
        public FreeCamController(IntPtr ptr) : base(ptr) { }

        private bool _active;
        private Camera _camera;
        private Transform _camTransform;

        private Transform _originalParent;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;

        private readonly List<Behaviour> _disabledBehaviours = new List<Behaviour>();

        private float _yaw;
        private float _pitch;
        private bool _prevCursorVisible;
        private CursorLockMode _prevCursorLockMode;

        private bool _loggedFirstUpdate;
        private float _lastHeartbeatLogTime;

        private void Update()
        {
            // 動作確認用: Update() が本当に呼ばれているかを一度だけログに残す
            if (!_loggedFirstUpdate)
            {
                _loggedFirstUpdate = true;
                Log.LogInfo("[診断] FreeCamController.Update() の実行を確認しました。");
            }

            try
            {
                bool togglePressed = Input.GetKeyDown(Plugin.ToggleKey.Value);

                // 動作確認用: 5秒おきにキー監視が生きていることをログに残す
                if (Time.unscaledTime - _lastHeartbeatLogTime > 5f)
                {
                    _lastHeartbeatLogTime = Time.unscaledTime;
                    Log.LogInfo($"[診断] 監視中... ToggleKey={Plugin.ToggleKey.Value}, active={_active}");
                }

                if (togglePressed)
                {
                    Log.LogInfo("[診断] トグルキーの押下を検知しました。");
                    Toggle();
                }

                if (_active)
                {
                    if (_camera == null)
                    {
                        // シーン遷移等でカメラが破棄された場合は追従を諦めて解除する
                        Deactivate(restoreTransform: false);
                        return;
                    }

                    HandleLook();
                    HandleMove();
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[診断] Update中に例外が発生しました: {ex}");
            }
        }

        private void Toggle()
        {
            if (_active)
            {
                Deactivate(restoreTransform: true);
            }
            else
            {
                Activate();
            }
        }

        private void Activate()
        {
            _camera = ResolveTargetCamera();
            if (_camera == null)
            {
                Log.LogWarning("有効なカメラが見つからなかったため、フリーカメラを開始できません。");
                return;
            }

            _camTransform = _camera.transform;

            _originalParent = _camTransform.parent;
            _originalLocalPosition = _camTransform.localPosition;
            _originalLocalRotation = _camTransform.localRotation;

            var euler = _camTransform.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;

            _disabledBehaviours.Clear();
            foreach (var behaviour in _camera.GetComponents<Behaviour>())
            {
                if (behaviour == this || behaviour == _camera) continue;
                if (!behaviour.enabled) continue;

                behaviour.enabled = false;
                _disabledBehaviours.Add(behaviour);
            }

            // 親のリグごと動く実装(追従カメラ等)を切り離す
            _camTransform.SetParent(null, true);

            if (Plugin.LockCursorWhileActive.Value)
            {
                _prevCursorVisible = Cursor.visible;
                _prevCursorLockMode = Cursor.lockState;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _active = true;
            Log.LogInfo("フリーカメラを有効化しました。");
        }

        private void Deactivate(bool restoreTransform)
        {
            _active = false;

            if (_camTransform != null && restoreTransform)
            {
                _camTransform.SetParent(_originalParent, true);
                _camTransform.localPosition = _originalLocalPosition;
                _camTransform.localRotation = _originalLocalRotation;
            }

            foreach (var behaviour in _disabledBehaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = true;
                }
            }
            _disabledBehaviours.Clear();

            if (Plugin.LockCursorWhileActive.Value)
            {
                Cursor.visible = _prevCursorVisible;
                Cursor.lockState = _prevCursorLockMode;
            }

            _camera = null;
            _camTransform = null;
            Log.LogInfo("フリーカメラを解除しました。");
        }

        private void HandleLook()
        {
            float sensitivity = Plugin.MouseSensitivity.Value;
            float invert = Plugin.InvertY.Value ? 1f : -1f;

            _yaw += Input.GetAxis("Mouse X") * sensitivity;
            _pitch += Input.GetAxis("Mouse Y") * sensitivity * invert;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            _camTransform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void HandleMove()
        {
            float speed = Plugin.MoveSpeed.Value;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speed *= Plugin.SprintMultiplier.Value;
            }

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += _camTransform.forward;
            if (Input.GetKey(KeyCode.S)) move -= _camTransform.forward;
            if (Input.GetKey(KeyCode.A)) move -= _camTransform.right;
            if (Input.GetKey(KeyCode.D)) move += _camTransform.right;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) move -= Vector3.up;

            if (move.sqrMagnitude > 0f)
            {
                _camTransform.position += move.normalized * speed * Time.unscaledDeltaTime;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                Plugin.MoveSpeed.Value = Mathf.Max(0.5f, Plugin.MoveSpeed.Value + scroll * 10f);
            }
        }

        /// <summary>
        /// メインカメラを優先し、見つからない場合は有効な最初のカメラを使う。
        /// </summary>
        private static Camera ResolveTargetCamera()
        {
            if (Camera.main != null) return Camera.main;

            var cameras = Camera.allCameras;
            for (int i = 0; i < cameras.Length; i++)
            {
                var cam = cameras[i];
                if (cam != null && cam.isActiveAndEnabled) return cam;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (_active)
            {
                Deactivate(restoreTransform: false);
            }
        }
    }
}
