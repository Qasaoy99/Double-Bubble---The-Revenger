using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    public Transform player;
    public float screenWidth = 29.83f;
    public float smoothSpeed = 0.125f;

    // هذا هو مركز الغرفة الأولى بالضبط
    public float firstRoomX = 14.99f;
    public float centerOffset = 14.91f;

    void LateUpdate()
    {
        if (player != null)
        {
            // حساب رقم الغرفة بناءً على موقع اللاعب بالنسبة لنقطة البداية
            // طرحنا 0.08 لأنها نقطة بداية الرسم الفعلية لديك
            float relativePlayerPos = player.position.x - 0.08f;
            int roomIndex = Mathf.FloorToInt(relativePlayerPos / screenWidth);

            // منع الكاميرا من الذهاب للغرف السلبية
            if (roomIndex < 0) roomIndex = 0;

            // تحديد موقع الكاميرا (رقم الغرفة * العرض + مركز الغرفة الأولى)
            float targetX = (roomIndex * screenWidth) + firstRoomX;

            Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        }
    }
}