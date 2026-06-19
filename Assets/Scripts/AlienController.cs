using UnityEngine;

public class AlienController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 6f;
    public float interactRange = 2.5f; 
    
    [Header("Pengaturan Pipa Sedot")]
    public float pullRange = 10f;          // Batas maksimal jarak penarikan kue
    public float wallCheckDistance = 2.5f; // Jarak maksimal Alien dengan tembok besi
    public float pullSpeed = 7f;           // Kecepatan kue melayang ke arah Alien
    public float cakeStopOffset = 1.2f;    // Jarak berhentinya kue di samping (kiri/kanan) Alien
    
    public GameObject dombaObject; 
    private DombaController dombaScript;
    
    private bool isPossessing = false;
    private Rigidbody rb;
    private MeshRenderer mesh;
    private Collider col;

    // Variabel untuk animasi tarik
    private bool isPullingCake = false;
    private GameObject targetCake;
    private Vector3 pullDestination;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        
        if (dombaObject != null)
        {
            dombaScript = dombaObject.GetComponent<DombaController>();
        }
    }

    void Update()
    {
        if (isPossessing)
        {
            transform.position = dombaObject.transform.position;
            if (Input.GetKeyDown(KeyCode.E)) KeluarDariDomba();
            return; 
        }

        // Pergerakan Alien
        float moveX = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1;
        rb.linearVelocity = new Vector3(moveX * speed, rb.linearVelocity.y, 0);

        if (Input.GetKeyDown(KeyCode.UpArrow) && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) MenghancurkanBeton();

        if (Input.GetKeyDown(KeyCode.Alpha2)) MemulaiSedotCreamCake();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Vector3.Distance(transform.position, dombaObject.transform.position) <= interactRange)
            {
                MerasukiDomba();
            }
        }

        // --- PROSES ANIMASI MENARIK KUE ---
        if (isPullingCake && targetCake != null)
        {
            // Menggerakkan kue perlahan menuju titik tujuan di samping Alien
            targetCake.transform.position = Vector3.MoveTowards(targetCake.transform.position, pullDestination, pullSpeed * Time.deltaTime);

            // Jika kue sudah sampai di titik tujuan (jarak sangat dekat)
            if (Vector3.Distance(targetCake.transform.position, pullDestination) < 0.1f)
            {
                SelesaiSedot();
            }
        }
    }

    void MenghancurkanBeton()
    {
        Collider[] objekSekitar = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider objek in objekSekitar)
        {
            if (objek.CompareTag("Breakable"))
            {
                Destroy(objek.gameObject);
                Debug.Log("Beton hancur!");
            }
        }
    }

    void MemulaiSedotCreamCake()
    {
        // 1. LIMITASI DEKAT TEMBOK: Cek apakah ada objek ber-tag "Indestructible" di sekitar Alien
        bool dekatTembok = false;
        Collider[] objekSekitar = Physics.OverlapSphere(transform.position, wallCheckDistance);
        foreach (Collider obj in objekSekitar)
        {
            if (obj.CompareTag("Indestructible")) 
            {
                dekatTembok = true;
                break;
            }
        }

        if (!dekatTembok)
        {
            Debug.Log("Gagal: Alien harus berada di dekat tembok untuk menyedot!");
            return; // Batalkan tarikan jika tidak dekat tembok
        }

        // 2. Tentukan SISI TEMBOK: cari tembok (Indestructible) terdekat.
        //    Cake hanya boleh di sisi yang SAMA dengan tembok (agar tidak menarik tembus).
        float sisiTembok = 0f; // positif = tembok di kanan, negatif = tembok di kiri
        Collider[] semuaObjek = Physics.OverlapSphere(transform.position, wallCheckDistance);
        foreach (Collider obj in semuaObjek)
        {
            if (obj.CompareTag("Indestructible"))
            {
                float bedaX = obj.transform.position.x - transform.position.x;
                if (Mathf.Abs(bedaX) > 0.1f)
                {
                    sisiTembok = bedaX;
                    break;
                }
            }
        }

        // 3. LIMITASI JARAK & SISI KUE: Cari cream cake terdekat yang:
        //    - Dalam jangkauan pullRange
        //    - Berada di sisi yang SAMA dengan tembok
        GameObject[] semuaCake = GameObject.FindGameObjectsWithTag("CreamCake");
        GameObject cakeTerdekat = null;
        float jarakTerdekat = pullRange;

        foreach (GameObject c in semuaCake)
        {
            float jarak = Vector3.Distance(transform.position, c.transform.position);
            if (jarak > jarakTerdekat) continue;

            // Cek apakah kue berada di sisi yang SAMA dengan tembok
            float arahCake = c.transform.position.x - transform.position.x;
            if (sisiTembok != 0f && arahCake * sisiTembok <= 0f)
            {
                Debug.Log($"Cream Cake {c.name} di sisi berlawanan dari tembok, dilewati.");
                continue;
            }

            jarakTerdekat = jarak;
            cakeTerdekat = c;
        }

        if (cakeTerdekat != null && !isPullingCake)
        {
            targetCake = cakeTerdekat;
            isPullingCake = true;

            // Tentukan kue harus ditarik ke kiri atau ke kanan Alien
            float arahOffset = (cakeTerdekat.transform.position.x > transform.position.x) ? cakeStopOffset : -cakeStopOffset;
            
            // Set titik tujuan persis di samping Alien, setara dengan posisi Y Alien saat ini
            pullDestination = new Vector3(transform.position.x + arahOffset, transform.position.y, transform.position.z);

            // Matikan collider dan fisika kue sementara agar bisa "menembus" tembok dengan mulus
            Collider cakeCol = targetCake.GetComponent<Collider>();
            Rigidbody cakeRb = targetCake.GetComponent<Rigidbody>();
            if (cakeCol != null) cakeCol.enabled = false;
            if (cakeRb != null) cakeRb.isKinematic = true;

            Debug.Log($"Mulai menarik Cream Cake: {targetCake.name} (sisi: {(arahOffset > 0 ? "kanan" : "kiri")})");
        }
        else if (cakeTerdekat == null)
        {
            Debug.Log("Gagal: Tidak ada Cream Cake dalam jangkauan atau di sisi yang benar!");
        }
    }

    void SelesaiSedot()
    {
        isPullingCake = false;
        
        // Aktifkan kembali collider dan fisika kue agar bisa jatuh/tersentuh lagi
        Collider cakeCol = targetCake.GetComponent<Collider>();
        Rigidbody cakeRb = targetCake.GetComponent<Rigidbody>();
        if (cakeCol != null) cakeCol.enabled = true;
        if (cakeRb != null) cakeRb.isKinematic = false;
        
        targetCake = null;
    }

    void MerasukiDomba()
    {
        isPossessing = true;
        dombaScript.isPossessed = true;
        mesh.enabled = false;
        col.enabled = false;
        rb.isKinematic = true; 
    }

    void KeluarDariDomba()
    {
        isPossessing = false;
        dombaScript.isPossessed = false;
        mesh.enabled = true;
        col.enabled = true;
        rb.isKinematic = false;
        transform.position = dombaObject.transform.position + new Vector3(0, 1.5f, 0);
    }
}