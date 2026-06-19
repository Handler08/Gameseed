using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pattern agar script lain bisa memanggil GameManager dengan mudah
    public static GameManager instance;

    [Header("Pengaturan Objective")]
    public int totalCakesRequired = 3; // Jumlah kue yang harus dikumpulkan untuk menang
    private int currentCakes = 0;      // Jumlah kue yang saat ini sudah diambil

    void Awake()
    {
        // Menyambungkan instance Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fungsi yang akan dipanggil oleh kue saat diambil oleh Domba
    public void TambahKue()
    {
        currentCakes++;
        
        // Memunculkan status objective di jendela Console Unity
        Debug.Log("Kue Berhasil Diambil! Progress: " + currentCakes + " / " + totalCakesRequired);

        // Cek apakah semua kue sudah terkumpul
        if (currentCakes >= totalCakesRequired)
        {
            Debug.Log("OBJECTIVE SELESAI! Semua kue telah dikumpulkan. Kamu Menang!");
            // Nanti di sini bisa kamu tambahkan logika untuk pindah ke Next Level
        }
    }
}