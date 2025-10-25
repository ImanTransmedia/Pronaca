using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Android;

namespace XRMultiplayer
{
    [DefaultExecutionOrder(100)]
    public class PlayerMic : MonoBehaviour
    {
        [SerializeField] AudioMixer m_Mixer;

        [Header("Voice Chat")]
        [SerializeField] Button m_MicPermsButton;
        [SerializeField] Image m_LocalPlayerAudioVolume;
        [SerializeField] Image m_MutedIcon;
        [SerializeField] Image m_MicOnIcon;

        VoiceChatManager m_VoiceChatManager;
        PermissionCallbacks permCallbacks;

        private void Awake()
        {
            m_VoiceChatManager = FindFirstObjectByType<VoiceChatManager>();
            m_VoiceChatManager.selfMuted.Subscribe(MutedChanged);
            permCallbacks = new PermissionCallbacks();
            permCallbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            permCallbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
            m_MicPermsButton.gameObject.SetActive(false);
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        }

        void OnEnable()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                m_MicPermsButton.gameObject.SetActive(true);
            }
            else
            {
                m_MicPermsButton.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            m_VoiceChatManager.selfMuted.Unsubscribe(MutedChanged);
        }

        private void Update()
        {
                m_LocalPlayerAudioVolume.fillAmount = OfflinePlayerAvatar.voiceAmp.Value;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetVolumeLevel(float sliderValue)
        {
            m_Mixer.SetFloat("MainVolume", Mathf.Log10(sliderValue) * 20);
        }
        public void SetInputVolume(float volume)
        {
            float perc = Mathf.Lerp(-10, 10, volume);
            m_VoiceChatManager.SetInputVolume(perc);
        }

        public void SetOutputVolume(float volume)
        {
            float perc = Mathf.Lerp(-10, 10, volume);
            m_VoiceChatManager.SetOutputVolume(perc);
        }

        public void ToggleMute()
        {
            m_VoiceChatManager.ToggleSelfMute();
        }
        void MutedChanged(bool muted)
        {
            m_MutedIcon.enabled = muted;
            m_MicOnIcon.enabled = !muted;
            m_LocalPlayerAudioVolume.enabled = !muted;
            PlayerHudNotification.Instance.ShowText($"<b>Microphone: {(muted ? "OFF" : "ON")}</b>");
        }

    }
}
