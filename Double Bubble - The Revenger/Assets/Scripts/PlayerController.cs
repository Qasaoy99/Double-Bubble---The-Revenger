using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("إعدادات الحركة والقفز")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("إعدادات ملامسة الأرض")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount = 0;
    private float moveInput;

    [Header("إعدادات التصويب وإطلاق النار")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("إعدادات القرفصاء")]
    public float crouchSpeedMultiplier = 0.4f; // تبطيء الحركة أثناء القرفصاء — شعور لعب مألوف
    private bool isCrouching = false;

    [Header("حالة الموت")]
    private bool isDead = false;

    // مرجع للـ Animator — ميشتغل بدونه بس السكربت يضل حي
    private Animator anim;

    // تحويل أسماء الـ Parameters لـ int hashes — أسرع من الـ strings بكل نداء داخل Update
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // توقف كل التحكم لما اللاعب ميت — بس الفيزياء تكمل حتى يطيح بشكل طبيعي
        if (isDead) return;

        CheckGrounded();
        HandleCrouch();
        HandleMovement();
        HandleJumping();
        HandleShooting();
        UpdateAnimatorState();
    }

    // LateUpdate يشتغل بعد ما Animator يخلص — هذا مهم لأن الأنميشن أحياناً يحرك Transform.localScale
    // إذا flippنا بـ Update، الأنميتور بيكتب فوقنا وميصير شي. هنا نحن آخر واحد يلمس الـ scale.
    void LateUpdate()
    {
        if (isDead) return;
        ApplyFacing();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpCount = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void HandleCrouch()
    {
        // القرفصاء بس لما اللاعب على الأرض — قرفصاء بالهوا ما تنفع منطقياً
        isCrouching = isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
    }

    void HandleMovement()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        // تبطيء أثناء القرفصاء
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    // نقلب الـ scale بدل flipX علمود الـ Children (firePoint, groundCheck) ينقلبون تلقائياً
    // تنادى من LateUpdate علمود تضمن أنها بعد الـ Animator
    void ApplyFacing()
    {
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x), // Abs يحافظ على الحجم الأصلي أي كان
                transform.localScale.y,
                transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);
        }
    }

    void HandleJumping()
    {
        // منع القفز أثناء القرفصاء — قرار تصميم لعب
        if (isCrouching) return;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (jumpCount < 2)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount++;
    }

    void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.right);
                TriggerShootAnim();
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.left);
                TriggerShootAnim();
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.up);
                TriggerShootAnim();
            }
            else if (Input.GetKey(KeyCode.DownArrow) && !isGrounded)
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.down);
                TriggerShootAnim();
            }
        }
    }

    void Shoot(Vector2 direction)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null) bulletRb.linearVelocity = direction * bulletSpeed;
        }
    }

    // SetTrigger بدل SetBool — الإطلاق نبضة قصيرة، نريدها تنطلق مرة واحدة كل ضغطة
    void TriggerShootAnim()
    {
        if (anim != null) anim.SetTrigger(ShootHash);
    }

    void UpdateAnimatorState()
    {
        if (anim == null) return;

        anim.SetFloat(SpeedHash, Mathf.Abs(moveInput));
        anim.SetBool(IsGroundedHash, isGrounded);
        // VerticalVelocity تفرّق بين Jump (+) و Fall (-) داخل الـ Animator blend tree
        anim.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
        anim.SetBool(IsCrouchingHash, isCrouching);
    }

    // يُستدعى من نظام Health — نداء public علمود سكربت خارجي يوصله
    public void PlayHitAnim()
    {
        if (anim != null) anim.SetTrigger(HitHash);
    }

    public void Die()
    {
        if (isDead) return; // حماية من الاستدعاء مرتين
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsDeadHash, true);
    }
}
