//using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponMode
{
    Rifle,
    ShotGun
}

public class PlayerManager : Singleton<PlayerManager>
{
    public Transform playerHead;//�÷��̾� �Ӹ� ��ġ - 1��Ī ��带 ����

    public HPScriptableObject hpManager;

    public float moveSpeed = 5.0f; //�÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f;//���콺 ����
    public CharacterController characterController; // ĳ���� ��Ʈ�ѷ� ������Ʈ
    private Transform mainCamera; //ī�޶� ������Ʈ

    public float pitch = 0.0f; //���Ʒ� ȸ����
    public float yaw = 0.0f; // �¿� ȸ����

    //�߷� ���� ����
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;

    public Animator animator;
    float horizontal;
    float vertical;
    private bool isRunning = false;
    public bool isCrouching = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float crouchSpeed = 3.0f;

    public bool isAim = false;
    private bool isFire = false;

    public GameObject shotGunObj;

    // private int animationSpeed = 1;
    // private string currentAnimation = "Idle";
    // //private bool isPickingUP = false;
    // private AnimatorStateInfo animatorStateInfo;

    private float weaponMaxDistance = 100f;

    public LayerMask TargetLayerMask;


    public Vector3 boxSize = new Vector3(2.0f, 2.0f, 2.0f);
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos; //raycast �� ��� ��Ÿ���� �����ִ�

    public bool isWeaponGet = false;
    public GameObject weaponIcon;
    public bool isWeponHold = false;

    //public ParticleSystem shotGunEffect;
    public GameObject shotGunPoint;

    private float shotGunFireDelay = 0.5f;

    public float attackPower = 2.0f;

    public Text bulletText;
    private int fireBulletCount;
    private int saveBulletCount;


    public GameObject flashLightObj;
    private bool isFlashLightOn = false;

    public int playerHP;

    public GameObject PauseObj;
    private bool isPause = false;

    public GameObject handPos;

    private RaycastHit[] currentHitItems;
    private bool isPotionHold = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = CameraManager.Instance.mainCamera.transform;
        animator = GetComponent<Animator>();
        shotGunObj.SetActive(false);

        animator.SetFloat("Horizontal", horizontal);

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
        WeaponChange();
        Attack();


        ItemPickUP();

        AnimationSet();
        Crouch();
        Reload();
        ActionFlashLight();

        PauseMenu();
        CheckTree();
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
            Vector3 origin = itemGetPos.position; //player pivot�� �߳��̶� ���� ����
            Vector3 direction = itemGetPos.forward;
            DebugBox(origin, direction);
            currentHitItems = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);

            animator.SetTrigger("PickUp");
            SoundManager.Instance.PlaySFX("ItemPickUP", transform.position);
        }
    }
    GameObject holdPotion;
    private void CheckTree()
    {
        //GameObject holdPotion = null;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!isPotionHold)
            {
                Vector3 origin = playerHead.position; //player pivot�� �߳��̶� ���� ����
                Vector3 direction = itemGetPos.forward;
                DebugBox(origin, direction);

                currentHitItems = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);

                bool isTreeForward = false;
                

                foreach (RaycastHit hit in currentHitItems)
                {
                    if (hit.collider.gameObject.CompareTag("Tree"))
                    {
                        isTreeForward = true;
                    }
                    if(hit.collider.gameObject.CompareTag("Eatable"))
                    {
                        isPotionHold = true;
                        holdPotion = hit.collider.gameObject;

                    }
                }
                if(isTreeForward && isPotionHold)
                {
                    animator.SetTrigger("Jump");
                    holdPotion.transform.SetParent(handPos.transform);
                    holdPotion.transform.localPosition = new Vector3(0.3f, -0.1f, 0.1f);
                }
                isTreeForward = false ;
            }
            else
            {
                SoundManager.Instance.PlaySFX("Eating");
                holdPotion.SetActive(false);
                hpManager.IncreaseHealth(10);
                isPotionHold =false;
            }
            
        }
    }

    private void ChangeAnimatorSpeed()
    {
        animator.speed = 1f;
    }

    void ItemInActive()
    {
       
        foreach (RaycastHit hit in currentHitItems)
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

            if(hit.collider.gameObject.name == "Door")
            {
                if(hit.collider.GetComponent<DoorManager>().isOpen)
                {
                    hit.collider.GetComponent<Animator>().SetTrigger("OpenBackward");

                    hit.collider.GetComponent<DoorManager>().isOpen = false;
                }
                else
                {
                    hit.collider.GetComponent<Animator>().SetTrigger("OpenForward");

                    hit.collider.GetComponent<DoorManager>().isOpen = true;

                }
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


        if (CameraManager.Instance.isFirstPerson)
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

    void WeaponChange()
    {
        if (isWeaponGet)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                animator.SetTrigger("isWeaponChange");
                shotGunObj.SetActive(true);
                isWeponHold = true;
            }
        }
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
                    ParticleManager.Instance.PlayParticle(ParticleType.Explosion, hit.point);
                    StartCoroutine(hitObject.GetComponent<ZombieManager>()?.TakeDamage(attackPower));
                }
                Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
            }

        }

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
        Vector3 moveDirection = mainCamera.forward * vertical + mainCamera.right * horizontal;
        moveDirection.y = 0.0f; //y�� ������ 0
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime); // �� �������� movespeed��ŭ �ӷ����� �̵�


        //ī�޶� ��ġ�� �÷��̾� �Ӹ��� 
        mainCamera.position = playerHead.position;
        //1��Ī���� ī�޶� �ٶ󺸴� ������ ���콺 ������ ���� ����
        mainCamera.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        //�÷��̾ ī�޶� y������ ��ġ��Ŵ
        transform.rotation = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0.0f);
    }

    void ThirdPersonMovemnet()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);


        CameraManager.Instance.UpdateCameraPosition();
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

    public void WeaponChangeSoundOn()
    {
        SoundManager.Instance.PlaySFX("ItemPickUP", transform.position);
    }

    public void ShootingGunSoundOn()
    {
        SoundManager.Instance.PlaySFX("ShotGunShoot", transform.position);
        ParticleManager.Instance.PlayParticle(ParticleType.WeaponFire, shotGunPoint.transform.position);
        //shotGunEffect.Play();
    }
}
