using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public string lastDoorID = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(lastDoorID))
            return;

        Debug.Log($"[GameManager] Scene loaded: {scene.name}. Mencari DoorSpawn dengan ID '{lastDoorID}'...");

        // --- Perbaikan: API baru ---
        DoorSpawn[] spawns = Object.FindObjectsByType<DoorSpawn>(FindObjectsSortMode.None);

        foreach (var ds in spawns)
        {
            if (ds.doorID == lastDoorID)
            {
                TeleportPlayerTo(ds);
                lastDoorID = "";
                return;
            }
        }

        Debug.LogWarning($"[GameManager] Tidak menemukan DoorSpawn dengan doorID '{lastDoorID}' di scene {scene.name}.");
    }

    public void TeleportPlayerTo(DoorSpawn ds)
    {
        if (ds == null || ds.spawnPoint == null)
        {
            Debug.LogWarning("[GameManager] DoorSpawn atau spawnPoint null.");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GameManager] Tidak menemukan Player.");
            return;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = ds.spawnPoint.position;
        player.transform.rotation = ds.spawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log($"[GameManager] Player dipindah ke doorID '{ds.doorID}'");
    }
}
