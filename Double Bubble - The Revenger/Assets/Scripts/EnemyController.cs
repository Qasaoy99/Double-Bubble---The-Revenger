using System;
using UnityEngine;

// سكربت عدو شامل: دورية + مطاردة + هجوم + نظام صحة.
// يعتمد على Animator بالباراميترات: Speed (float), Attack (trigger), TakeHit (trigger), IsDead (bool).
// لاحقاً ضربة اللاعب تنادي TakeDamage(int) علمود تأذي العدو.
public class EnemyController : MonoBehaviour
{
    [Header("إعدادات الحركة")]
    public float moveSpeed = 1.5f;

    [Header("نقاط الدورية")]
    // Empty GameObjects نحطّهم في المشهد، العدو يمشي بينهم ذهاباً وإياباً
    public Transform patrolPointA;
    public Transform patrolPointB;

    [Header("كشف اللاعب")]
    public Transform player;
    public float detectionRange = 5f;   // نصف قطر بداية المطاردة
    public float attackRange = 0.9f;    // المسافة اللي فيها يضرب بدل ما يلاحق
    public float attackCooldown = 1.2f; // فاصل بين الضربات علمود ما يسبام المهاجمة

    [Header("الصحة")]
    public int maxHealth = 3;

    [Header("الـ Hitbox (child GameObject يتفعل أثناء frames الضربة)")]
    [SerializeField] private EnemyMeleeHitbox attackHitbox;

    // events عشان شريط الصحة يسمع بدون polling — أي UI ممكن يشترك بدون تغيير هذا السكربت
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // الحالة الداخلية
    private int currentHealth;
    private Transform currentTarget;   // النقطة الحالية المتجه لها في الدورية
    private float lastAttackTime = -999f; // قيمة سالبة كبيرة علمود أول ضربة تصير فوراً
    private bool isDead = false;

    // مراجع الكومبوننتس
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // تحويل أسماء الباراميترات لـ int hashes — أسرع من الـ strings داخل Update
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int TakeHitHash = Animator.StringToHash("TakeHit");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // نخبر UI بالقيمة المبدئية
        // نبدأ بالتوجه للنقطة A — إذا ما في نقاط، العدو يوقف فقط (يُعالج في Patrol)
        currentTarget = patrolPointA;
    }

    void Update()
    {
        // لما يموت نوقف كل السلوك — الجسد يطيح طبيعياً بالفيزياء
        if (isDead) return;

        // حماية من NullReference لو نسيت تحط اللاعب في Inspector
        if (player == null)
        {
            Patrol();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // ترتيب الأولوية متعمّد: الهجوم أهم من المطاردة علمود ما يدخل في اللاعب
        if (distToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distToPlayer <= detectionRange)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // إذا ما حدّدت نقاط دورية، العدو يوقف ساكن بدل ما يتحرك عشوائياً
        if (patrolPointA == null || patrolPointB == null)
        {
            StopMoving();
            return;
        }

        MoveToward(currentTarget.position);

        // بدّل الهدف لما نصل قرب النقطة — 0.2 مسافة معقولة (ميصير اهتزاز حول النقطة)
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
        {
            currentTarget = (currentTarget == patrolPointA) ? patrolPointB : patrolPointA;
        }
    }

    void Chase()
    {
        MoveToward(player.position);
    }

    void AttackPlayer()
    {
        // نوقف الحركة أثناء الهجوم — واقف في مكانه يضرب
        StopMoving();

        // نواجه اللاعب حتى لو وقفنا — يضمن إن الضربة تطلع للاتجاه الصحيح
        FaceTarget(player.position.x);

        // كولداون يمنع السبام — الأنميشن يحتاج وقت يخلص بأي حال
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger(AttackHash);
            lastAttackTime = Time.time;
        }
    }

    // تحريك العدو أفقياً نحو نقطة معينة (x فقط — Y تتركه الفيزياء)
    void MoveToward(Vector3 targetPos)
    {
        float dir = Mathf.Sign(targetPos.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat(SpeedHash, moveSpeed); // نمرّر moveSpeed كقيمة موجبة علمود الـ Transition يشتغل
        FaceTarget(targetPos.x);
    }

    void StopMoving()
    {
        // نصفّر X فقط — Y تبقى علمود الجاذبية تشتغل طبيعي
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetFloat(SpeedHash, 0f);
    }

    // flipX بدل قلب الـ scale — أبسط وما يأثر على الأبناء
    // Goblin sprite يطلع وجهه لليمين افتراضياً، فـ flipX=true لما نتجه يسار
    void FaceTarget(float targetX)
    {
        if (targetX < transform.position.x)
            sr.flipX = true;
        else if (targetX > transform.position.x)
            sr.flipX = false;
    }

    // Animation Event proxies — لازم تكون على GameObject اللي عليه الأنميتور (القوبلن root).
    // نحوّل النداء للـ child hitbox لأن Animation Events ما تشوف الأبناء.
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.EnableHitbox();
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.DisableHitbox();
    }

    // يستدعى من سكربت اللاعب لما الضربة تلمس العدو
    public void TakeDamage(int damage)
    {
        if (isDead) return; // ما نسمح بضرر بعد الموت

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // يحدّث شريط الصحة
        anim.SetTrigger(TakeHitHash);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetBool(IsDeadHash, true);
        rb.linearVelocity = Vector2.zero;

        // Static يوقف الفيزياء بالكامل — الجسد يضل مكانه على الأرض
        rb.bodyType = RigidbodyType2D.Static;

        // نعطّل الكولايدر علمود اللاعب يقدر يمر فوق الجثة بدون ما يصدم فيها
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        OnDied?.Invoke(); // يخفي شريط الصحة
    }

    // رسم دوائر المدى في Scene view — تساعد في ضبط detectionRange و attackRange بصرياً
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // خط بين نقطتي الدورية — يوضّح المسار
        if (patrolPointA != null && patrolPointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
        }
    }
}
