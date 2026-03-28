using UnityEngine;

public class Bullet : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // تجاهل اللاعب
        if (hitInfo.CompareTag("Player")) return;

        // الإضافة الجديدة: إذا لمست الرصاصة رصاصة أخرى، تجاهل الأمر!
        if (hitInfo.GetComponent<Bullet>() != null) return;

        // تدمير الرصاصة عند ملامسة أي شيء آخر
        Destroy(gameObject);
    }
}