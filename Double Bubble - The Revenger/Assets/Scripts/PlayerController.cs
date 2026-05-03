using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("إعدادات الحركة والقفز")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float crouchSpeedMultiplier = 0.4f;

    [Header("إعدادات ملامسة الأرض")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("إعدادات القتال (الكومبو البسيط)")]
    public float comboResetTime = 0.5f; // وقت نسيان الضربة والعودة للضربة الأولى
    private int hitCount = 0; // عداد الضربات (1 أو 2)
    private float lastAttackTime;

    // مكونات اللاعب
    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private bool isGrounded;
    private bool isCrouching;
    private bool canDoubleJump;

    // Hashes
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        CheckGrounded();
        HandleCrouch();
        HandleAttack();
        HandleMovement();
        HandleJumping();
        UpdateAnimatorState();
    }

    void LateUpdate()
    {
        // نمنع الشخصية من الالتفاف إذا كانت تضرب
        if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            ApplyFacing();
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded) canDoubleJump = true;
    }

    void HandleCrouch() => isCrouching = isGrounded && Input.GetKey(KeyCode.S);

    // منطق الهجوم 
    void HandleAttack()
    {
        // === الشرط الجديد لمنع تكرار الضربة القاضية ===
        // نتحقق هل الأنيميتر يعرض حالياً ملف "3x atk_Clip"
        bool isPlayingUltimate = anim.GetCurrentAnimatorStateInfo(0).IsName("3x atk_Clip");

        // إذا كانت القاضية تعمل، اخرج من الدالة تماماً ولا تسمح بأي هجوم جديد حتى تنتهي
        if (isPlayingUltimate) return;
        // ===============================================

        // 1. إذا توقفت عن الضرب، انسَ الكومبو لتبدأ من الضربة الأولى
        if (Time.time - lastAttackTime > comboResetTime)
        {
            hitCount = 0;
        }

        // 2. الهجوم العادي (سهم لأسفل) - ضربتين فقط
        if (Input.GetKeyDown(KeyCode.DownArrow) && isGrounded)
        {
            lastAttackTime = Time.time;
            hitCount++;

            if (hitCount == 1)
            {
                anim.Play("1x atk_c_Clip", -1, 0f);
            }
            else if (hitCount >= 2)
            {
                anim.Play("2x atk_Clip", -1, 0f);
                hitCount = 0;
            }

            // تجميد الحركة فوراً
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // 3. الضربة القاضية (سهم لأعلى) 
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            // تشغيل القاضية فوراً
            anim.Play("3x atk_Clip", -1, 0f);

            // تصفير كومبو السهم لأسفل
            hitCount = 0;

            // تجميد الحركة فوراً
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void HandleMovement()
    {
        // التحقق مما إذا كان الأنيميتر يعرض أنيميشن يحمل كلمة "Attack"
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        // إذا كان يضرب، لا تسمح له بالمشي
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            moveInput = 0f;
            return;
        }

        moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJumping()
    {
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        if (isCrouching || isAttacking) return;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (canDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = false;
                anim.SetTrigger(DoubleJumpHash);
            }
        }
    }

    void ApplyFacing()
    {
        if (moveInput > 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void UpdateAnimatorState()
    {
        if (anim == null) return;
        anim.SetBool(WalkHash, Mathf.Abs(moveInput) > 0);
        anim.SetBool(IsGroundedHash, isGrounded);
        anim.SetBool(IsCrouchingHash, isCrouching);
        anim.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
    }
}