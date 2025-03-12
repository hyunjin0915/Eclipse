using System;
using System.Collections;
using UnityEngine; //nameSpace : �Ҽ�

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f; //�÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f;//���콺 ����
    public Transform cameraTransform; //ī�޶� Ʈ������
    public CharacterController characterController; // ĳ���� ��Ʈ�ѷ� ������Ʈ
    public Transform playerHead;//�÷��̾� �Ӹ� ��ġ - 1��Ī ��带 ����
    public float thirdPersonDistance = 3.0f; //3��Ī��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0f);//3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; //�÷��̾� �þ� ��ġ

    public float zoomDistance = 1.0f;//ī�޶� Ȯ��� ���� �Ÿ� - 3��Ī ���
    public float zoomSpeed = 5.0f; //Ȯ�� ��Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; //�⺻ ī�޶� �þ� ��
    public float zoomFov = 30.0f;// Ȯ�� �� ī�޶� �þ߰� - 1��Ī ���

    private float currentDistance; //���� ī�޶���� �Ÿ� - 3��Ī ���
    private float targetDistance; //��ǥ ī�޶� �Ÿ�
    private float targetFov; //��ǥ FOv
    private bool isZoomed = false; //Ȯ�� ���� Ȯ��
    private Coroutine zoomCoroutine; // �ڷ�ƾ�� ����Ͽ� Ȯ����� ó��
    private Camera mainCamera; //ī�޶� ������Ʈ

    private float pitch = 0.0f; //���Ʒ� ȸ����
    private float yaw = 0.0f; // �¿� ȸ����
    private bool isFirstPerson = false; //1��Ī ��� ����
    private bool isRotateAroundPlayer = true; //ī�޶� �÷��̾� ������ ȸ���ϴ��� ���� 

    //�߷� ���� ����
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;

    private Animator animator;
    float horizontal;
    float vertical;
    private bool isRunning = false;
    private bool isCrouching = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float crouchSpeed = 3.0f;

    private bool isAim = false;
    private bool isFire = false;

    public AudioClip audioClipFire;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;
    public AudioClip audioClipItemPickUp;
    public GameObject shotGunObj;

    private int animationSpeed = 1;
    private string currentAnimation = "Idle";
    private bool isPickingUP = false;
    private AnimatorStateInfo animatorStateInfo;

    public Transform aimTarget;
    private float weaponMaxDistance = 100f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        shotGunObj.SetActive(false);

        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        animator.SetFloat("Horizontal", horizontal);
    }

    void Update()
    {
        MouseSet();
        CameraSet();
        WeaponChange();
        Attack();


        ItemPickUP();
        if (isPickingUP)
        {
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (animatorStateInfo.IsName("TakingItem"))
            {
                if (animatorStateInfo.normalizedTime >= 1.0f)
                {
                    animator.speed = 1;
                    isPickingUP=false;
                }
            }

        }
        AnimationSet();
        Crouch();
    }
    void AnimationSet()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("isRunning", isRunning);
    }

    void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if(!isCrouching)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", isCrouching);
                moveSpeed = crouchSpeed;
                /*animator.SetTrigger("Crouch");
                animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if(animatorStateInfo.IsName("ToCrouched") && animatorStateInfo.normalizedTime >= 1.0f)
                {
                    animator.SetBool("isCrouching", isCrouching);
                }*/
                
            }
            else
            {
                isCrouching = false;
                animator.SetBool("isCrouching", isCrouching);
                moveSpeed = walkSpeed;
                
            }

        }

    }

    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    private void ItemPickUP()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.speed = 2;
            animator.SetTrigger("PickUp");
            audioSource.PlayOneShot(audioClipItemPickUp);

            isPickingUP = true;
        }
    }
    void MouseSet()
    {
        //���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45); //�¿� �þ߰� ���� 

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0.0f) //Ȥ�� ���� ���� ����
        {
            velocity.y -= 2f;
        }


        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovemnet();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            moveSpeed = runSpeed;
        }
        else
        {
            isRunning = false;
            moveSpeed = walkSpeed;
        }

    }

    void CameraSet()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī ���" : "3��Ī ���");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾ �þ߸� ���� ȸ���մϴ�.");
        }
        ZoomCamera();
    }

    void ZoomCamera()
    {
        if (Input.GetMouseButtonDown(1)) //�����ؼ� ���ϱ�
        {
            isAim = true;
            animator.SetLayerWeight(1, 1); //ù��° ���̾� ���� 1�� �ٲ��(Ȱ��ȭ)
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine); //�������� �ڷ�ƾ ������ ����
            }
            if (isFirstPerson) //1��Ī�� ��
            {
                SetTargetFOV(zoomFov); //Ÿ�� �Ÿ��� 1��Ī �� �Ÿ��� ����
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //1��Ī�� �� ī�޶� �� �ڷ�ƾ ����
            }
            else //3��Ī �϶� 
            {
                SetTargetDistance(zoomDistance); //Ÿ�� �Ÿ��� �� �Ÿ��� ����
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //3��Ī�� �� ī�޶� �� �ڷ�ƾ ���� 
            }

        }
        if (Input.GetMouseButtonUp(1)) //���ƿ��� 
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine); //�������� �ڷ�ƾ ������ ����
            }
            if (isFirstPerson) //1��Ī�� ��
            {
                SetTargetFOV(defaultFov); //Ÿ�� �Ÿ��� 1��Ī �� �Ÿ��� ����
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //1��Ī�� �� ī�޶� �� �ڷ�ƾ ����
            }
            else //3��Ī �϶� 
            {
                SetTargetDistance(thirdPersonDistance); //Ÿ�� �Ÿ��� �� �Ÿ��� ����
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //3��Ī�� �� ī�޶� �� �ڷ�ƾ ���� 
            }
            isAim = false;
            animator.SetLayerWeight(1, 0);
        }
    }
    void WeaponChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //audioSource.PlayOneShot(audioClipWeaponChange);
            animator.SetTrigger("isWeaponChange");
            shotGunObj.SetActive(true);
        }
        //animator.SetBool("isAim", isAim);
    }
    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && isAim)
        {
            //�� ������ ���� �����Ÿ� ����
            weaponMaxDistance = 1000.0f;

            isFire = true;
            animator.SetTrigger("Fire");

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward); //ī�޶󿡼� ������ ���� 
            RaycastHit hit;

            GameObject hitObject;

            if(Physics.Raycast(ray, out hit, weaponMaxDistance))
            {
                hitObject = hit.collider.gameObject;
                if(hitObject.CompareTag("Enemy"))
                {
                    hitObject.SetActive(false);
                }
                
                Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                    
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
            }
            
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFire = false;
        }
    }


    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //ī�޶� �ٶ󺸴� ���� 
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0.0f; //y�� ������ 0
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime); // �� �������� movespeed��ŭ �ӷ����� �̵�


        //ī�޶� ��ġ�� �÷��̾� �Ӹ��� 
        cameraTransform.position = playerHead.position;
        //1��Ī���� ī�޶� �ٶ󺸴� ������ ���콺 ������ ���� ����
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        //�÷��̾ ī�޶� y������ ��ġ��Ŵ
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0.0f);
    }

    void ThirdPersonMovemnet()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);


        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            //ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����(�÷��̾ �ٶ󺸴� ���� �ƴ϶� ��¦ ���� ����)
            cameraTransform.LookAt(transform.position + new Vector3(thirdPersonOffset.x, thirdPersonOffset.y, 0));
        }
        else //�÷��̾ ���� ���� 
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);

            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
            
            UpdateAimTarget();

        }
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }
    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    IEnumerator ZoomCamera(float targetDistance) //3��Ī�� �� ī�޶� ��ġ�� ����
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) //���� �Ÿ����� ��ǥ�Ÿ��� �ε巴�� �̵�
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; //��ǥ�Ÿ� ���� �� �� ����
    }

    IEnumerator ZoomFieldOfView(float targetFov) //1��Ī�� ��(ī�޶� ��ü�� �����̴� �� �ƴ϶� �ܸ� �Ǹ� �Ǵϱ�)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }

    public void WeaponChangeSoundOn()
    {
        Debug.Log("����ٲٱ�������");
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    public void ShootingGunSoundOn()
    {
        audioSource.PlayOneShot(audioClipFire);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //transform.position = Vector3.zero;
            WeaponChangeSoundOn();
            animator.SetTrigger("Damage");

            transform.gameObject.GetComponent<CharacterController>().enabled = false;
            transform.position = Vector3.zero;
            transform.gameObject.GetComponent<CharacterController>().enabled = true;
        }
    }
}
