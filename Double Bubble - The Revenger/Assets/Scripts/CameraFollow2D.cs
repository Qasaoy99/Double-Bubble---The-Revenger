using UnityEngine;

// كاميرا 2D تتابع اللاعب بحركة سلسة (SmoothDamp).
// تضاف على Main Camera، ويُسحب Player إلى خانة Target في الـ Inspector.
public class CameraFollow2D : MonoBehaviour
{
    [Header("هدف المتابعة")]
    public Transform target;

    [Header("إعدادات السلاسة")]
    [Tooltip("أقل = أسرع في اللحاق باللاعب. تجربة: 0.1 - 0.3")]
    public float smoothTime = 0.15f;

    [Header("إزاحة الكاميرا عن اللاعب")]
    // Z = -10 معيار للكاميرا 2D — يضمن إنها قدام المشهد وترى الـ sprites
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity = Vector3.zero;

    // LateUpdate لأن اللاعب يتحرك في Update — نريد الكاميرا تتحدث بعد حركة اللاعب
    // عشان ما يصير jitter (اهتزاز)
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;

        // نحافظ على Z ثابت — تغيير Z يفسد الإضاءة والـ culling في 2D
        targetPos.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
