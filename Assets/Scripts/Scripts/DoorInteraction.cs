using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // gunakan TextMeshPro langsung

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // Drag Player di sini
    public GameObject interactUI;        // Drag Canvas (World Space) panel root
    public TextMeshProUGUI option1Text;  // Drag Text TMP untuk "Masuk"
    public TextMeshProUGUI option2Text;  // Drag Text TMP untuk "Batal"

    [Header("Interaction")]
    public float interactionDistance = 3f;

    [Header("Scene (assign in Editor)")]
    public string targetSceneName = "";

#if UNITY_EDITOR
    // Memudahkan assign scene lewat drag & drop (editor only)
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset != null)
            targetSceneName = sceneAsset.name;
    }
#endif

    private int selectedIndex = 0; // 0 = Masuk, 1 = Batal
    private bool isNearby = false;

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactionDistance)
        {
            if (!isNearby)
            {
                isNearby = true;
                if (interactUI != null)
                    interactUI.SetActive(true);
                UpdateUI();
            }

            HandleInput();

            // opsional: agar UI selalu menghadap kamera
            if (interactUI != null)
                FaceUIToCamera();
        }
        else
        {
            if (isNearby)
            {
                isNearby = false;
                if (interactUI != null)
                    interactUI.SetActive(false);
            }
        }
    }

    void HandleInput()
    {
        // Scroll atau panah atas/bawah untuk ganti pilihan
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + 2) % 2;
            UpdateUI();
        }
        else if (scroll < 0f || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % 2;
            UpdateUI();
        }

        // Enter untuk konfirmasi
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex == 0) // Masuk
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    SceneManager.LoadScene(targetSceneName);
                }
                else
                {
                    Debug.LogWarning("Target scene name kosong. Set targetSceneName di Inspector ya, Onii-chan!");
                }
            }
            else // Batal
            {
                if (interactUI != null)
                    interactUI.SetActive(false);
                isNearby = false;
            }
        }
    }

    void UpdateUI()
    {
        if (option1Text != null)
            option1Text.text = (selectedIndex == 0 ? "> Masuk" : "  Masuk");
        if (option2Text != null)
            option2Text.text = (selectedIndex == 1 ? "> Batal" : "  Batal");
    }

    void FaceUIToCamera()
    {
        Transform uiT = interactUI.transform;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = uiT.position - cam.transform.position;
        uiT.rotation = Quaternion.LookRotation(dir);
    }
}