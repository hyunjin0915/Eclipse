using UnityEngine; //nameSpace : 소속

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f; //플레이어 이동 속도
    public float mouseSensitivity = 100.0f;//마우스 감도
    public Transform cameraTransform; //카메라 트랜스폼
    public CharacterController characterController; // 캐릭터 컨트롤러 컴포넌트
    public Transform playerHead;//플레이어 머리 위치 - 1인칭 모드를 위해
    public float thirdPersonDistance = 3.0f; //3인칭모드에서 플레이어와 카메라의 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0f);//3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; //플레이어 시야 위치

    public float zoomDistace = 1.0f;//카메라가 확대될 때의 거리 - 3인칭 모드
    public float zoomSpeed = 5.0f; //확대 축소가 되는 속도
    public float defaultFov = 60.0f; //기본 카메라 시야 각
    public float zoomFov = 30.0f;// 확대 시 카메라 시야각 - 1인칭 모드

    private float currentDistance; //현재 카메라와의 거리 - 3인칭 모드
    private float targetDistance; //목표 카메라 거리
    private float targetFov; //목표 FOv
    private bool isZoomed = false; //확대 여부 확인
    private Coroutine zoomCoroutine; // 코루틴을 사용하여 확대축소 처리
    private Camera mainCamera; //카메라 컴포넌트

    private float pitch = 0.0f; //위아래 회전값
    private float yaw = 0.0f; // 좌우 회전값
    private bool isFirstPerson = false; //1인칭 모드 여부
    private bool isRotaterAroundPlayer = true; //카메라가 플레이어 주위를 회전하는지 여부 

    //중력 관련 변수
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
    }

    void Update()
    {
        //마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY; 
        pitch = Mathf.Clamp(pitch, -45, 45); //좌우 시야각 제한 

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0.0f) //혹시 몰라서 대비용 변수
        {
            velocity.y -= 2f;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야를 따라서 회전합니다.");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovemnet();
        }

    }

    void FirstPersonMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //카메라가 바라보는 방향 
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal; 
        moveDirection.y = 0.0f; //y출 방향은 0
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime); // 그 방향으로 movespeed만큼 속력으로 이동

        //카메라 위치를 플레이어 머리로 
        cameraTransform.position = playerHead.position;
        //1인칭에서 카메라 바라보는 각도를 마우스 각도에 따라서 변경
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        //플레이어를 카메라 y각도랑 일치시킴
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0.0f);
    }

    void ThirdPersonMovemnet()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if(isRotaterAroundPlayer)
        {
            //카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정(플레이어를 바라보는 것이 아니라 살짝 옆을 보게)
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else //플레이어가 직접 도는 
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);

            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }

}
