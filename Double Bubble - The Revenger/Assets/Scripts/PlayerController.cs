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
    private int jumpCount = 0; // لتعقب عدد القفزات (حد أقصى 2)
    private float moveInput;

    [Header("إعدادات التصويب وإطلاق النار")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("إعدادات الأنيميشن (جديد)")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // تعريف مكونات الأنيميشن والشكل
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJumping();
        HandleShooting();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // التعديل السري هنا: لا تصفر القفزات إلا إذا كان اللاعب ثابتاً أو يسقط (وليس أثناء انطلاقه للأعلى)
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpCount = 0;
        }
    }

    // هذه الدالة ترسم دائرة حمراء في شاشة المشهد (Scene) لتساعدك كمطور على رؤية مكان حساس الأرض
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void HandleMovement()
    {
        // الحركة بحرفي A و D
        moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // ==========================================
        // إضافة الأنيميشن والالتفات هنا
        // ==========================================
        if (anim != null)
        {
            // تشغيل وإيقاف حركة الركض بناءً على قيمة الإدخال
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
        }

        if (spriteRenderer != null)
        {
            // قلب الشخصية لتنظر لليمين أو اليسار
            if (moveInput > 0)
            {
                spriteRenderer.flipX = false; // يمين
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = true; // يسار
            }
        }
    }

    void HandleJumping()
    {
        // القفز بزر المسافة (Space)
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (jumpCount < 2) // يسمح بالقفزة الثانية فقط
            {
                Jump();
                // تم إزالة الإطلاق التلقائي من هنا بناءً على طلبك
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
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.left);
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.up);
            }
            // التعديل الجديد: الإطلاق للأسفل بالسهم السفلي فقط إذا كان في الهواء
            else if (Input.GetKey(KeyCode.DownArrow) && !isGrounded)
            {
                nextFireTime = Time.time + fireRate;
                Shoot(Vector2.down);
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
}