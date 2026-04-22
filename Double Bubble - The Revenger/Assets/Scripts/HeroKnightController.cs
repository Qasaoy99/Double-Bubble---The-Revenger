using UnityEngine;

// نسخة مبسطة من HeroKnight demo script — نفس الحركات الأصلية (attack combo، block، roll، jump)
// بدون الحاجة لأبناء Sensor_HeroKnight. يستخدم GroundCheck + OverlapCircle للتحقق من الأرض.
// مناسب لـ Player اللي ما تبي تعدّل بنيته (أبناء GroundCheck و FirePoint يبقون زي ما هم).
public class HeroKnightController : MonoBehaviour
{
    [Header("إعدادات الحركة")]
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;

    [Header("إعدادات ملامسة الأرض")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("إعدادات الهجوم")]
    [SerializeField] Transform attackPoint;                         // empty child قدام اللاعب
    [SerializeField] Vector2 attackHitboxSize = new Vector2(1.2f, 1f);
    [SerializeField] int attackDamage = 10;
    [SerializeField] LayerMask enemyLayers;                         // طبقة Enemy من Layer dropdown

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;

    // Roll duration مشتقة من طول animation (8 frames @ 14 fps) — يضمن ان الدحرجة تخلص مع الأنميشن
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // مؤقت combo الضربات — بعد ثانية بدون ضرب، ترجع الضربة الأولى
        m_timeSinceAttack += Time.deltaTime;

        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // ground check بـ OverlapCircle بدل GroundSensor — يحافظ على بنية Player الحالية
        bool wasGrounded = m_grounded;
        m_grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // نحدّث الـ Animator بس لما الحالة تتغير — يوفر نداءات زيادة
        if (m_grounded != wasGrounded)
            m_animator.SetBool("Grounded", m_grounded);

        // نستخدم KeyCode مباشرة بدل Input.GetAxis("Horizontal") لأن Unity 6 أحياناً
        // يختلط مع Input System الجديد وما يرد قيم — KeyCode مضمون يشتغل مع Input Manager القديم
        float inputX = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX = 1f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;

        // flipX بدل قلب الـ scale — لأن HeroKnight عنده أبناء قد تنقلب معاه بشكل غلط
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // أثناء الدحرجة ما نسمح بتغيير السرعة — العجلة تأخذ التحكم
        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        // AirSpeedY يوجّه الـ Animator blend بين jump animation (+) و fall animation (-)
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        // الترتيب كـ if/else if متعمد — نسمح بـ action واحد كل فريم (مثل الأصلي)
        // Death (E)
        if (Input.GetKeyDown(KeyCode.E) && !m_rolling)
        {
            m_animator.SetTrigger("Death");
        }
        // Hurt (Q)
        else if (Input.GetKeyDown(KeyCode.Q) && !m_rolling)
        {
            m_animator.SetTrigger("Hurt");
        }
        // Attack combo (Left Click) — 3 ضربات بالدوران
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // reset combo لو مرّ وقت طويل بين الضربات
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            PerformAttackHit(); // damage check يترافق مع تشغيل الأنميشن
            m_timeSinceAttack = 0.0f;
        }
        // Block (Right Click) — أثناء الضغط يصير IdleBlock true
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
        }
        // Roll (Left Shift) — دحرجة في اتجاه الوجه الحالي
        else if (Input.GetKeyDown(KeyCode.LeftShift) && !m_rolling)
        {
            m_rolling = true;
            m_rollCurrentTime = 0f;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
        // Jump (Space)
        else if (Input.GetKeyDown(KeyCode.Space) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
        }
        // Run — لو فيه حركة أفقية
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // delay صغير قبل الرجوع لـ Idle — يمنع flicker عند إيقاف الحركة للحظة
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        // Idle
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    // يعمل OverlapBox قدام اللاعب ويودّي ضرر لأي EnemyController أو Health يصاب بالـ hitbox.
    // public عشان تقدر تستدعيها من Animation Event لو بغيت timing أدق مع frames الضربة.
    public void PerformAttackHit()
    {
        Vector2 origin = GetAttackCenter();
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, attackHitboxSize, 0f, enemyLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            // نفحص EnemyController أولاً لأن هذا نظام الصحة الموجود للأعداء الحاليين
            var enemy = hits[i].GetComponentInParent<EnemyController>();
            if (enemy != null) { enemy.TakeDamage(attackDamage); continue; }

            // fallback لأي كيان مستقبلي يستخدم Health component
            var hp = hits[i].GetComponentInParent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);
        }
    }

    // نعكس offset الـ attackPoint لما الـ sprite يكون flipX عشان الـ hitbox يضل قدام الوجه
    private Vector2 GetAttackCenter()
    {
        if (!attackPoint) return transform.position;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.flipX)
        {
            Vector3 local = attackPoint.localPosition;
            local.x = -local.x;
            return transform.TransformPoint(local);
        }
        return attackPoint.position;
    }

    // يرسم دائرة حمراء مكان groundCheck و مربع للـ attack hitbox في الـ Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(attackPoint.position, attackHitboxSize);
        }
    }
}
