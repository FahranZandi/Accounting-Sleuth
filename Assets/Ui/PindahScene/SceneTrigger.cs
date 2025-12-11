using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Gameplay/Scene Trigger (Robust)")]
public class SceneTrigger : MonoBehaviour
{
    public enum DetectionMode
    {
        Trigger,    // rely on OnTriggerEnter
        Proximity   // check bounds.Contains(player.position) each frame
    }

    [Header("Detection")]
    public DetectionMode mode = DetectionMode.Trigger;
    [Tooltip("Tag untuk object yang akan memicu perpindahan scene. Default = Player")]
    public string triggerTag = "Player";

    [Header("Scene")]
    [Tooltip("Nama scene yang akan dimuat (akan terisi otomatis bila drag Scene Asset di editor)")]
    [SerializeField] private string sceneName;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    [Header("Proximity (fallback)")]
    [Tooltip("Jika menggunakan Proximity: cek posisi root transform player berada dalam bounds collider.")]
    public float proximityCheckInterval = 0.1f; // seconds

    private Collider _col;
    private float _proximityTimer = 0f;
    private bool _hasLoaded = false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col == null)
        {
            Debug.LogError("[SceneTrigger] Tidak ditemukan Collider pada GameObject ini. Tolong tambahkan BoxCollider atau Collider lain.");
        }

        // jika mode Trigger, warn jika collider tidak trigger
        if (mode == DetectionMode.Trigger && _col != null && !_col.isTrigger)
        {
            Debug.LogWarning("[SceneTrigger] Mode = Trigger tetapi Collider.isTrigger = false. Mengatur isTrigger = true otomatis.");
            _col.isTrigger = true;
        }
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneTrigger] sceneName kosong. Drag Scene .unity ke Scene Asset di inspector dan pastikan scene ada di Build Settings.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasLoaded) return;
        if (mode != DetectionMode.Trigger) return;

        if (!TagMatches(other)) return;

        // Ensure one of the colliders has rigidbody (Unity requires RigidBody for trigger callbacks)
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.LogWarning("[SceneTrigger] OnTriggerEnter terjadi tapi Rigidbody tidak terdeteksi pada object yang masuk. Jika tidak ada Rigidbody, trigger mungkin tidak dipanggil. Gunakan DetectionMode = Proximity sebagai alternatif.");
            // we still attempt load, but warn
        }

        TryLoadScene();
    }

    private bool TagMatches(Collider other)
    {
        if (string.IsNullOrEmpty(triggerTag)) return true;
        if (other.CompareTag(triggerTag)) return true;

        // sometimes collider is child: check root
        Transform root = other.transform.root;
        if (root != null && root.CompareTag(triggerTag)) return true;

        return false;
    }

    private void Update()
    {
        if (_hasLoaded) return;
        if (mode != DetectionMode.Proximity) return;

        _proximityTimer -= Time.deltaTime;
        if (_proximityTimer <= 0f)
        {
            _proximityTimer = proximityCheckInterval;
            CheckProximity();
        }
    }

    private void CheckProximity()
    {
        if (_col == null) return;

        // find all objects with the tag (should be only player ideally)
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(triggerTag);
        if (tagged == null || tagged.Length == 0)
        {
            Debug.LogWarning("[SceneTrigger] Tidak ditemukan GameObject dengan tag '" + triggerTag + "'. Pastikan tag sudah dibuat dan diterapkan pada player.");
            return;
        }

        Bounds b = _col.bounds;

        foreach (var go in tagged)
        {
            if (go == null) continue;
            Vector3 pos = go.transform.position;
            // cek apakah posisi berada dalam bounds collider
            if (b.Contains(pos))
            {
                Debug.Log("[SceneTrigger] Proximity detected for object: " + go.name);
                TryLoadScene();
                return;
            }
        }
    }

    private void TryLoadScene()
    {
        if (_hasLoaded) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneTrigger] Gagal memuat scene: sceneName kosong. Pastikan kamu drag SceneAsset di inspector dan tambahkan scene ke Build Settings.");
            return;
        }

        // check if scene is in build settings
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError("[SceneTrigger] Scene '" + sceneName + "' belum ada di Build Settings! Buka File -> Build Settings dan tambahkan scene tersebut.");
            return;
        }

        _hasLoaded = true;
        Debug.Log("[SceneTrigger] Memuat scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    private bool IsSceneInBuildSettings(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string n = System.IO.Path.GetFileNameWithoutExtension(path);
            if (n == name) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(name))
            {
                sceneName = name;
            }
        }

        _col = GetComponent<Collider>();
        // if collider present and using Trigger mode, set isTrigger true to help user
        if (_col != null && mode == DetectionMode.Trigger)
        {
            _col.isTrigger = true;
        }
    }
#endif
}
