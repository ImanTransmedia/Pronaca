using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using XRMultiplayer;

public class ProjectSceneManager : NetworkBehaviour
{
    /// <summary>
    /// Editor Fields
    /// </summary>
    [SerializeField] private List<string> m_SceneNames1;
    [SerializeField] private List<string> m_SceneNames2;
    [SerializeField] private List<string> m_SceneNames3;
    [SerializeField] private bool m_Connected = false;
    [SerializeField] private int m_BranchIndex = -1;
    [SerializeField] private int m_SceneIndex = -1;

    /// <summary>
    /// Private Fields
    /// </summary>
    private Scene m_LoadedScene;
    private Dictionary<string, Scene> m_LoadedScenes = new Dictionary<string, Scene>();


    IEnumerator Start()
    {
        yield return null;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
        NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = false;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"<color=cyan>[Connected]</color> Client with ID {clientId} connected.");
        m_Connected = true;
        // HandleSpawnRpc(clientId);
    }
    [Rpc(SendTo.Server)]
    void HandleSpawnRpc(ulong clientId)
    {
        // 1. Find the client in the NetworkManager's list
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient))
        {
            // 2. Get the player's NetworkObject from the client
            NetworkObject playerObject = networkClient.PlayerObject;

            if (playerObject != null)
            {
                // 3. (CRITICAL) Disable CharacterController if it exists
                // A CharacterController will fight the teleport and snap the player back.
                CharacterController controller = playerObject.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                // 4. Get the NetworkTransform and call Teleport
                // This instantly moves the object on all clients.
                Vector3 spawnpoint = new Vector3(UnityEngine.Random.Range(-5f, 5), 0, UnityEngine.Random.Range(-5f, 5));
                Debug.Log($"<color=cyan>[Spawned] {playerObject.name} </color> on: {spawnpoint.x},{spawnpoint.y},{spawnpoint.z}. For: {playerObject.NetworkTransforms.Count}");
                //NetworkTransform networkTransform = playerObject.GetComponent<NetworkTransform>();
                //networkTransform.Teleport(spawnpoint, Quaternion.identity, playerObject.transform.localScale);

                foreach(NetworkTransform ntransform in playerObject.NetworkTransforms)
                {
                    ntransform.Teleport(spawnpoint + ntransform.transform.localPosition, Quaternion.identity, playerObject.transform.localScale);
                }
                
                // 5. Re-enable the CharacterController after the teleport
                if (controller != null)
                {
                    controller.enabled = true;
                }
            }
            else
            {
                Debug.LogWarning($"Could not find PlayerObject for client: {clientId}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find client: {clientId}");
        }
    }
    
    IEnumerator TimeToPing(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        PingRpc(sceneName);
    }

    [Rpc(SendTo.Server)]
    public void PingRpc(string sceneName)
    {
        Debug.Log("<color=magenta>Loaded Scenes "+ m_LoadedScenes.Count + " </color>");
        PongRpc(sceneName, "PONG!");
        if(m_LoadedScenes.Count>=1)
        {
            UnloadScene(m_LoadedScenes.Last().Key);
        }
        else
        {
            LoadScene(sceneName);
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(string sceneName, string message)
    {
        Debug.Log($"<color=magenta>Received pong from server for ping {sceneName} and message {message}</color>");
    }

    public void SetScene(int branch, int sceneindex)
    {
        m_BranchIndex = branch;
        m_SceneIndex = sceneindex;
        Debug.Log($"<color=magenta>Menu {branch}, {sceneindex}!</color>");
        switch(branch)
        {
            case 1:
                StartCoroutine(TimeToPing(1, m_SceneNames1[m_SceneIndex]));
                break;
            case 2:
                StartCoroutine(TimeToPing(1, m_SceneNames2[m_SceneIndex]));
                break;
            case 3:
                StartCoroutine(TimeToPing(1, m_SceneNames3[m_SceneIndex]));
                break;
        }
        
    }
    void Update()
    {
        if (m_Connected && Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            if(m_BranchIndex != 1)
            {
                m_SceneIndex = -1;
                m_BranchIndex = 1;
            }
            ++m_SceneIndex;
            if(m_SceneIndex >= m_SceneNames1.Count)
            {
                m_SceneIndex = 0;
            }
            Debug.Log("<color=magenta>Key!</color>");
            StartCoroutine(TimeToPing(1, m_SceneNames1[m_SceneIndex]));
        }
        else if (m_Connected && Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            if (m_BranchIndex != 2)
            {
                m_SceneIndex = -1;
                m_BranchIndex = 2;
            }
            ++m_SceneIndex;
            if (m_SceneIndex >= m_SceneNames2.Count)
            {
                m_SceneIndex = 0;
            }
            Debug.Log("<color=magenta>Key!</color>");
            StartCoroutine(TimeToPing(1, m_SceneNames2[m_SceneIndex]));
        }
        else if (m_Connected && Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            if (m_BranchIndex != 3)
            {
                m_SceneIndex = -1;
                m_BranchIndex = 3;
            }
            ++m_SceneIndex;
            if (m_SceneIndex >= m_SceneNames3.Count)
            {
                m_SceneIndex = 0;
            }
            Debug.Log("<color=magenta>Key!</color>");
            StartCoroutine(TimeToPing(1, m_SceneNames3[m_SceneIndex]));
        }
    }
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (!m_LoadedScenes.ContainsKey(sceneEvent.SceneName))
            {
                m_LoadedScenes.Add(sceneEvent.SceneName, sceneEvent.Scene);
                Debug.Log($"Server: Successfully loaded and stored scene '{sceneEvent.SceneName}'");
            }
        }
        else if (sceneEvent.SceneEventType == SceneEventType.UnloadComplete)
        {
            if (m_LoadedScenes.ContainsKey(sceneEvent.SceneName))
            {
                m_LoadedScenes.Remove(sceneEvent.SceneName);
                Debug.Log($"Server: Successfully unloaded and removed scene '{sceneEvent.SceneName}'");
                string scenename;
                switch (m_BranchIndex)
                {
                    case 1:
                        scenename = m_SceneNames1[m_SceneIndex];
                        break;
                    case 2:
                        scenename = m_SceneNames2[m_SceneIndex];
                        break;
                    case 3:
                        scenename = m_SceneNames3[m_SceneIndex];
                        break;
                    default:
                        scenename = sceneEvent.SceneName;
                        break;
                }
                Debug.Log($"Server: Loading {m_BranchIndex}, {m_SceneIndex} '{scenename}'.");
                if (sceneEvent.SceneName != scenename)
                {
                    StartCoroutine(TimeToPing(0, scenename));
                }
                NetworkPhysicsInteractable[] interactables = GameObject.FindObjectsByType<NetworkPhysicsInteractable>(FindObjectsSortMode.None);
                foreach(NetworkPhysicsInteractable interactable in interactables)
                {
                    interactable.NetworkObject.Despawn();
                }
            }
        }
    }
    public void LoadScene(string sceneName)
    {
        if (m_LoadedScenes.ContainsKey(sceneName))
        {
            Debug.LogWarning($"Server: Scene '{sceneName}' is already loaded.");
            return;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadScene(string sceneName)
    {
        if (m_LoadedScenes.TryGetValue(sceneName, out Scene sceneToUnload))
        {
            NetworkManager.Singleton.SceneManager.UnloadScene(sceneToUnload);
        }
        else
        {
            Debug.LogError($"Server: Cannot unload scene '{sceneName}' because it was not found in the loaded scenes list.");
        }
    }

}
