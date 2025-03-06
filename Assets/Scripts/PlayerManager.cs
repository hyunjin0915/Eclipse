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

    public float zoomDistace = 1.0f;//ī�޶� Ȯ��� ���� �Ÿ� - 3��Ī ���
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
    private bool isRotaterAroundPlayer = true; //ī�޶� �÷��̾� ������ ȸ���ϴ��� ���� 

    //�߷� ���� ����
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

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī ���" : "3��Ī ���");
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾ �þ߸� ���� ȸ���մϴ�.");
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
            //ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����(�÷��̾ �ٶ󺸴� ���� �ƴ϶� ��¦ ���� ����)
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else //�÷��̾ ���� ���� 
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);

            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }

}
