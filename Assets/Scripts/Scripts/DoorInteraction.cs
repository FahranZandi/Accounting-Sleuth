using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject interactUI;        // Drag Canvas (child dari door)
    public TextMeshProUGUI option1Text;  // Drag TMP text "Masuk"
    public TextMeshProUGUI option2Text;  // Drag TMP text "Batal"

    [Header("Scene Settings")]
    public string targetSceneName = "";

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
    private void OnValidate()
    {
        if (sceneAsset != null)
            targetSceneName = sceneAsset.name;
    }
#endif

    private Transform player;
    private int selectedIndex = 0;
    private bool isNearby = false;
    private Collider triggerZone;

    private void Start()
    {
        triggerZone = GetComponent<Collider>();

        if (triggerZone == null)
        {
            Debug.LogError($"[DoorInteraction] Tidak ada Collider di {name}! Tambahkan collider dan centang IsTrigger!");
            return;
        }

        if (!triggerZone.isTrigger)
        {
            triggerZone.isTrigger = true;
            Debug.LogWarning($"[DoorInteraction] Collider di {name} belum di-set sebagai Trigger, sekarang diaktifkan otomatis!");
        }

        if (interactUI != null)
            interactUI.SetActive(false);

        UpdateUI();

        // Cek player yang mungkin sudah berada di dalam trigger sejak awal
        Collider[] hits = Physics.OverlapBox(triggerZone.bounds.center, triggerZone.bounds.extents);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("[DoorInteraction] Player sudah di dalam area saat Start()");
                OnTriggerEnter(hit);
                break;
            }
        }
    }

    private void Update()
    {
        if (!isNearby || player == null) return;

        HandleInput();
        FaceUIToCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger Enter: " + other.name);
            player = other.transform;
            isNearby = true;

            if (interactUI != null)
            {
                interactUI.SetActive(true);
                Debug.Log("UI AKTIF!");
            }

            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;
        Debug.Log("Trigger Exit: " + other.name);

        if (other.CompareTag("Player"))
        {
            isNearby = false;
            player = null;

            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }

    private void HandleInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + 2) % 2;
            UpdateUI();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % 2;
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex == 0)
            {
                if (Application.CanStreamedLevelBeLoaded(targetSceneName))
                    SceneManager.LoadScene(targetSceneName);
                else
                    Debug.LogError($"Scene '{targetSceneName}' belum ditambahkan ke Build Settings!");
            }
            else
            {
                interactUI.SetActive(false);
                isNearby = false;
            }
        }
    }

    private void UpdateUI()
    {
        if (option1Text != null)
            option1Text.text = (selectedIndex == 0 ? "> Masuk" : "  Masuk");
        if (option2Text != null)
            option2Text.text = (selectedIndex == 1 ? "> Batal" : "  Batal");
    }

    private void FaceUIToCamera()
    {
        if (interactUI == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = interactUI.transform.position - cam.transform.position;
        interactUI.transform.rotation = Quaternion.LookRotation(dir);
    }
}
