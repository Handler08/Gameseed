using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform alienTransform;
    public Transform dombaTransform;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 5, -8);
    public float smoothSpeed = 5f;

    [Header("Editor")]
    [SerializeField] private Transform targetUntukSetOffset;

    private AlienController alienController;
    private Transform currentTarget;

    void Start()
    {
        if (alienTransform == null)
        {
            AlienController comp = FindFirstObjectByType<AlienController>();
            if (comp != null) alienTransform = comp.transform;
        }

        if (dombaTransform == null)
        {
            GameObject domba = GameObject.FindWithTag("Player");
            if (domba != null) dombaTransform = domba.transform;
        }

        if (alienTransform != null)
            alienController = alienTransform.GetComponent<AlienController>();

        // Target awal: Alien
        currentTarget = alienTransform;

        if (currentTarget != null)
        {
            transform.position = currentTarget.position + offset;
        }
    }

    void LateUpdate()
    {
        TentukanTarget();

        if (currentTarget == null) return;

        Vector3 desiredPosition = currentTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    void TentukanTarget()
    {
        if (alienController == null || dombaTransform == null || alienTransform == null) return;

        // Saat merasuki: Domba yang dikendalikan
        if (alienController.IsPossessing())
        {
            currentTarget = dombaTransform;
            return;
        }

        // Input Alien (pakai GetKeyDown — baru ditekan)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentTarget = alienTransform;
        }

        // Input Domba (pakai GetKeyDown — baru ditekan)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            currentTarget = dombaTransform;
        }
    }

    [ContextMenu("Set Offset dari Posisi Kamera Saat Ini")]
    void SetOffsetDariPosisiKamera()
    {
        if (targetUntukSetOffset == null)
        {
            Debug.LogWarning("Isi dulu 'Target Untuk Set Offset' di Inspector!");
            return;
        }
        offset = transform.position - targetUntukSetOffset.position;
    }
}
