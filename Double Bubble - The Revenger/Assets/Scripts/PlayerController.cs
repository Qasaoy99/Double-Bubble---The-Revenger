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

    // مكونات اللاعب
    private Rigidbody2D rb;
    private Animator anim;

    // حالات اللاعب (States)
    private float moveInput;
    private bool isGrounded;
    private bool isCrouching;

    // Hashes لتحسين أداء الأنيميشن
    private static readonly int WalkHash = Animator.StringToHash("Walk"); // تم التغيير من Speed إلى Walk
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        CheckGrounded();
        HandleCrouch();
        HandleMovement();
        HandleJumping();
        UpdateAnimatorState();
    }

    void LateUpdate()
    {
        ApplyFacing();
    }

    // التحقق من ملامسة الأرض
    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // منطق النزول (القرفصاء)
    void HandleCrouch()
    {
        // النزول يحدث فقط إذا كانت على الأرض وتم الضغط على حرف S
        isCrouching = isGrounded && Input.GetKey(KeyCode.S);
    }

    // منطق المشي يميناً ويساراً
    void HandleMovement()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        // تبطيء السرعة في حالة القرفصاء
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;

        // استخدام linearVelocity الخاص بـ Unity 6
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    // منطق القفز
    void HandleJumping()
    {
        // منع القفز أثناء النزول
        if (isCrouching) return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // تدوير الشخصية لليمين واليسار (معدل ليتناسب مع كاونتس)
    void ApplyFacing()
    {
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // تحديث بيانات الـ Animator
    void UpdateAnimatorState()
    {
        if (anim == null) return;

        // إرسال أمر المشي (True أو False) بدلاً من حساب السرعة بالأرقام
        bool isWalking = Mathf.Abs(moveInput) > 0;
        anim.SetBool(WalkHash, isWalking);

        anim.SetBool(IsGroundedHash, isGrounded);
        anim.SetBool(IsCrouchingHash, isCrouching);
        anim.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
    }
}