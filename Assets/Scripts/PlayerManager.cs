using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging; //nameSpace : �Ҽ�
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>
{
    public HPScriptableObject hpManager;

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

    public GameObject shotGunObj;

    private int animationSpeed = 1;
    private string currentAnimation = "Idle";
    //private bool isPickingUP = false;
    private AnimatorStateInfo animatorStateInfo;

    public Transform aimTarget;
    private float weaponMaxDistance = 100f;

    public LayerMask TargetLayerMask;

    public MultiAimConstraint multiAimConstraint; //ó�������� �� Ʋ�����ְ� �ż� �ڵ�� ����

    public Vector3 boxSize = new Vector3(2.0f, 2.0f, 2.0f);
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos; //raycast �� ��� ��Ÿ���� �����ִ�

    public GameObject crosshairObj;
    private bool isWeaponGet = false;
    public GameObject weaponIcon;
    private bool isWeponHold = false;

    public ParticleSystem shotGunEffect;

    private float shotGunFireDelay = 0.5f;

    public float attackPower = 2.0f;

    public Text bulletText;
    private int fireBulletCount;
    private int saveBulletCount;

    public ParticleSystem DamageParticleSystem;

    public GameObject flashLightObj;
    private bool isFlashLightOn = false;
    public AudioClip audioClipFlash;

    public int playerHP;

    public GameObject PauseObj;
    private bool isPause = false;
    public AudioClip audioClipPause;

    public GameObject handPos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        shotGunObj.SetActive(false);

        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        animator.SetFloat("Horizontal", horizontal);

        crosshairObj.SetActive(false);
        weaponIcon.SetActive(false);

        bulletText.gameObject.SetActive(false);
        fireBulletCount = 5;
        saveBulletCount = 2;
        bulletText.text = fireBulletCount + "/" + saveBulletCount;

        flashLightObj.SetActive(false);

        playerHP = 100;
    }

    void Update()
    {
        MouseSet();
        CameraSet();
        WeaponChange();
        Attack();


        ItemPickUP();

        AnimationSet();
        Crouch();
        Reload();
        ActionFlashLight();

        PauseMenu();
    }

    void PauseMenu()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SoundManager.Instance.PlaySFX("UIButton");
            isPause = !isPause;
            if (isPause)
            {
                Pause();
            }
            else
            {
                ReGame();
            }
        }
    }

    public void ReGame()
    {
        PauseObj.SetActive(false);
        Time.timeScale = 1; //���� �ð� �簳 

    }
    void Pause()
    {
        PauseObj.SetActive(true);
        Time.timeScale = 0; //���� �ð� ����
    }

    public void Exit()
    {
        PauseObj.SetActive(false);
        Time.timeScale = 1; //���� �ð� �簳 
        Application.Quit();
    }
    void ActionFlashLight()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            isFlashLightOn = !isFlashLightOn;
            flashLightObj.SetActive(isFlashLightOn);

            SoundManager.Instance.PlaySFX("FlashOn", flashLightObj.transform.position);

        }
    }
    void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        { 
            int cnt = 5 - fireBulletCount; //�����ؾ��ϴ°��� 
            if (cnt < saveBulletCount) 
            {
                fireBulletCount += cnt;
                saveBulletCount -= cnt;
            }
            else
            {
                fireBulletCount += saveBulletCount;
                saveBulletCount = 0;
            }

            bulletText.text = fireBulletCount + "/" + saveBulletCount;
            SoundManager.Instance.PlaySFX("ShotGunLoad", transform.position);
            animator.SetTrigger("Reload");
        }
            
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
            if (!isCrouching)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", isCrouching);
                moveSpeed = crouchSpeed;
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

    void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green);
    }
    private void ItemPickUP()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.speed = 1.5f;
            animator.SetTrigger("PickUp");
            SoundManager.Instance.PlaySFX("ItemPickUP", transform.position);
        }
    }

    private void ChangeAnimatorSpeed()
    {
        animator.speed = 1f;
    }

    void ItemInActive()
    {
        Vector3 origin = itemGetPos.position; //player pivot�� �߳��̶� ���� ����
        Vector3 direction = itemGetPos.forward;
        DebugBox(origin, direction);
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Weapon"))
            {
                hit.collider.gameObject.SetActive(false);
                isWeaponGet = true;
                weaponIcon.SetActive(true);
                bulletText.gameObject.SetActive(true);
            }

            if(hit.collider.gameObject.CompareTag("Bullet"))
            {
                hit.collider.transform.SetParent(handPos.transform);
                //hit.collider.gameObject.SetActive(false);
                saveBulletCount += 30;
                if(saveBulletCount > 120)
                {
                    saveBulletCount = 120;
                }
                bulletText.text = fireBulletCount + "/" + saveBulletCount;
            }
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
        if (isWeaponGet && isWeponHold)
        {
            if (Input.GetMouseButtonDown(1)) //�����ؼ� ���ϱ�
            {
                if(isCrouching)
                {
                    characterController.height = 1.3f;
                }

                isAim = true;
                multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
                animator.SetLayerWeight(1, 1); //ù��° ���̾� ���� 1�� �ٲ��(Ȱ��ȭ)
                crosshairObj.SetActive(true);
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
                characterController.height = 1.9f;
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
                multiAimConstraint.data.offset = new Vector3(0, 0, 0);
                animator.SetLayerWeight(1, 0);
                crosshairObj.SetActive(false);
            }
        }

    }
    void WeaponChange()
    {
        if (isWeaponGet)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //audioSource.PlayOneShot(audioClipWeaponChange);
                animator.SetTrigger("isWeaponChange");
                shotGunObj.SetActive(true);
                isWeponHold = true;
            }
        }
        
        //animator.SetBool("isAim", isAim);
    }
    int cnt = 0;
    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && isAim && !isFire)
        {
            if (fireBulletCount > 0)
            {
                fireBulletCount--;
                bulletText.text = fireBulletCount + "/" + saveBulletCount;
            }
            else
            {
                //������
                //�Ѿ��� ���� �Ҹ� ��� �� ����
                SoundManager.Instance.PlaySFX("EmpthyTrigger", transform.position);
                return;
            }
            //�� ������ ���� �����Ÿ� ����
            weaponMaxDistance = 1000.0f;

            isFire = true;
            animator.SetTrigger("Fire");
            StartCoroutine(FireWithDealy(shotGunFireDelay));

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward); //ī�޶󿡼� ������ ���� 
            RaycastHit hit;

            /*RaycastHit[] raycastHits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);
            if (raycastHits.Length > 0)
            {
                foreach (RaycastHit hitObj in raycastHits)
                {
                    //if (cnt == 2) break;
                    //cnt++;
                    Debug.Log("�浹 : " + hitObj.collider.name);
                    Debug.DrawLine(ray.origin, hitObj.point, Color.red, 2.0f);
                }
            }*/

            GameObject hitObject;

            if(Physics.Raycast(ray, out hit, weaponMaxDistance, TargetLayerMask))
            {
                hitObject = hit.collider.gameObject;
                if(hitObject.CompareTag("Enemy"))
                {
                    ParticleSystem particle = Instantiate(DamageParticleSystem, hit.point,Quaternion.identity);

                    particle.Play();
                    //audioSource.PlayOneShot(audioClipDamage);
                    StartCoroutine(hitObject.GetComponent<ZombieManager>()?.TakeDamage(attackPower));
                    
                }
                
                Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                    
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
            }

        }

        /*if (Input.GetMouseButtonUp(0))
        {
            isFire = false;
        }*/
    }

    private IEnumerator FireWithDealy(float fireDelay)
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
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
        SoundManager.Instance.PlaySFX("ItemPickUP", transform.position);
    }

    public void ShootingGunSoundOn()
    {
        SoundManager.Instance.PlaySFX("ShotGunShoot", transform.position);
        shotGunEffect.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PlayerDamage"))
        {
            hpManager.DecreaseHealth(10);

            transform.gameObject.GetComponent<CharacterController>().enabled = false;
            transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z-1);
            transform.gameObject.GetComponent<CharacterController>().enabled = true;

            animator.SetTrigger("Damage");
            playerHP -= 10;
            SoundManager.Instance.PlaySFX("PlayerDamaged");
            other.gameObject.SetActive(false);
        }
    }

    private void ChangeCharacterHeight()
    {
        characterController.height = 1.3f;
    }
}
