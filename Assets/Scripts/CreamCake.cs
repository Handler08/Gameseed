using UnityEngine;

public class CreamCake : MonoBehaviour
{
    // Fungsi bawaan Unity yang otomatis aktif jika ada objek lain menabrak objek ini
    private void OnCollisionEnter(Collision collision)
    {
        // Cek apakah objek yang menabrak memiliki Tag "Player" (yaitu si Domba)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Hubungi GameManager untuk menambah hitungan kue
            GameManager.instance.TambahKue();

            // Hancurkan/hilangkan objek kue ini dari game
            Destroy(gameObject);
        }
    }
}