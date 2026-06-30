using UnityEngine;

public class DombaController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;            // Drag Animator Controller kesini

    [Header("Movement (sama dengan Alien)")]
    public float speed = 5f;
    public float jumpForce = 6f;

    [Header("Terbang")]
    public float flyForce = 30f;         // Kekuatan dorong ke atas saat terbang
    public float fallMultiplier = 1.3f;  // Pengali gravitasi saat jatuh (1 = normal, makin besar makin cepet)
    public float maxFlyTime = 10f;       // Durasi terbang maksimal (detik)
    public float flyRegenRate = 3f;      // Kecepatan regen per detik
    public bool isPossessed = false;     // Status apakah sedang dirasuki Alien

    [HideInInspector]
    public float currentFlyTime;         // Sisa waktu terbang saat ini

    private Rigidbody rb;
    private bool isGrounded = false;
    private Texture2D barBgTexture;
    private Texture2D barFillTexture;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentFlyTime = maxFlyTime;

        // Buat texture untuk GUI bar
        barBgTexture = new Texture2D(1, 1);
        barBgTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        barBgTexture.Apply();

        barFillTexture = new Texture2D(1, 1);
        barFillTexture.SetPixel(0, 0, Color.cyan);
        barFillTexture.Apply();
    }

    void Update()
    {
        // Pergerakan Kiri/Kanan (A dan D)
        float moveX = 0;
        if (Input.GetKey(KeyCode.A)) moveX = -1;
        if (Input.GetKey(KeyCode.D)) moveX = 1;

        // Terapkan kecepatan pada sumbu X, pertahankan kecepatan Y bawaan gravitasi
        rb.linearVelocity = new Vector3(moveX * speed, rb.linearVelocity.y, 0);

        // Cek apakah di tanah (grounded)
        isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        // Lompat (W) - Hanya bisa lompat jika menyentuh tanah
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // --- MEKANIK TERBANG (hanya saat dirasuki) ---
        bool menekanSpasi = Input.GetKey(KeyCode.Space);
        bool isFlying = false;

        if (isPossessed && menekanSpasi && currentFlyTime > 0f)
        {
            // Terbang ke atas: konsumsi durasi
            rb.AddForce(Vector3.up * flyForce * Time.deltaTime, ForceMode.VelocityChange);
            currentFlyTime -= Time.deltaTime;
            if (currentFlyTime < 0f) currentFlyTime = 0f;
            isFlying = true;
        }
        else if (!menekanSpasi)
        {
            // Regenerasi durasi saat tidak menekan Spasi
            currentFlyTime += flyRegenRate * Time.deltaTime;
            if (currentFlyTime > maxFlyTime) currentFlyTime = maxFlyTime;
        }

        // --- FALL MULTIPLIER: biar jatoh lebih cepet (hanya saat dirasuki) ---
        if (isPossessed && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f) * rb.mass, ForceMode.Force);
        }

        // --- UPDATE ANIMASI ---
        if (animator != null)
        {
            bool isWalking = Mathf.Abs(moveX) > 0.01f;
            bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;

            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isFlying", isFlying);
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }

    void OnGUI()
    {
        if (!isPossessed) return;

        // Posisi & ukuran bar
        float barWidth = 200f;
        float barHeight = 20f;
        float barX = (Screen.width - barWidth) / 2f;
        float barY = Screen.height - 50f;

        // Latar belakang bar (hitam transparan)
        GUI.DrawTexture(new Rect(barX - 2, barY - 2, barWidth + 4, barHeight + 4), barBgTexture);

        // Isi bar (cyan) — proporsional dengan currentFlyTime / maxFlyTime
        float fillRatio = currentFlyTime / maxFlyTime;
        GUI.DrawTexture(new Rect(barX, barY, barWidth * fillRatio, barHeight), barFillTexture);

        // Teks "Fly" dan persentase
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), $"FLY  {currentFlyTime:F1}s / {maxFlyTime:F0}s", labelStyle);
    }
}