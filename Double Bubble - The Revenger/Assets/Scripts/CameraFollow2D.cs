using UnityEngine;

// كاميرا 2D تتابع اللاعب بحركة سلسة (SmoothDamp) مع نظام حدود لمنع الخروج عن الخريطة.
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

    [Header("حدود الكاميرا (لمنع رؤية الفراغ)")]
    public bool enableBounds = true; // تفعيل الحدود
    [Tooltip("الحد الأدنى: أقصى نقطة لليسار وللأسفل")]
    public Vector2 minCameraPos;
    [Tooltip("الحد الأقصى: أقصى نقطة لليمين وللأعلى")]
    public Vector2 maxCameraPos;

    private Vector3 velocity = Vector3.zero;

    // LateUpdate لأن اللاعب يتحرك في Update — نريد الكاميرا تتحدث بعد حركة اللاعب
    // عشان ما يصير jitter (اهتزاز)
    void LateUpdate()
    {
        if (target == null) return;

        // 1. حساب المكان الذي يجب أن تذهب إليه الكاميرا بناءً على موقع اللاعب
        Vector3 targetPos = target.position + offset;

        // نحافظ على Z ثابت — تغيير Z يفسد الإضاءة والـ culling في 2D
        targetPos.z = offset.z;

        // 2. إذا كانت الحدود مفعلة، نقوم بحبس الهدف داخل هذه الحدود
        if (enableBounds)
        {
            // نمنع X (اليمين واليسار) من تجاوز الحدود
            targetPos.x = Mathf.Clamp(targetPos.x, minCameraPos.x, maxCameraPos.x);
            // نمنع Y (الأعلى والأسفل) من تجاوز الحدود
            targetPos.y = Mathf.Clamp(targetPos.y, minCameraPos.y, maxCameraPos.y);
        }

        // 3. التحرك بنعومة نحو المكان المطلوب (المقيد)
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}