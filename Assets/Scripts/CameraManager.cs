using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CameraManager : Singleton<CameraManager>
{
    public GameObject mainCamera;

    public bool isFirstPerson = false; //1인칭 모드 여부
    private bool isRotateAroundPlayer = true; //카메라가 플레이어 주위를 회전하는지 여부 


    public float thirdPersonDistance = 3.0f; //3인칭모드에서 플레이어와 카메라의 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0f);//3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; //플레이어 시야 위치

    public float zoomDistance = 1.0f;//카메라가 확대될 때의 거리 - 3인칭 모드
    public float zoomSpeed = 5.0f; //확대 축소가 되는 속도
    public float defaultFov = 60.0f; //기본 카메라 시야 각
    public float zoomFov = 30.0f;// 확대 시 카메라 시야각 - 1인칭 모드

    private float currentDistance; //현재 카메라와의 거리 - 3인칭 모드
    private float targetDistance; //목표 카메라 거리
    private float targetFov; //목표 FOv
    private bool isZoomed = false; //확대 여부 확인
    private Coroutine zoomCoroutine; // 코루틴을 사용하여 확대축소 처리
    public MultiAimConstraint multiAimConstraint; //처음시작할 때 틀어져있게 돼서 코드로 수정
    public GameObject crosshairObj;


    private PlayerManager pm;

    public Transform aimTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera.GetComponent<Camera>().fieldOfView = defaultFov;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;

        pm = PlayerManager.Instance;

        crosshairObj.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        CameraSet();
    }

    void CameraSet()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야를 따라서 회전합니다.");
        }
        ZoomCamera();
    }

    void ZoomCamera()
    {
        if (pm.isWeaponGet && pm.isWeponHold)
        {
            if (Input.GetMouseButtonDown(1)) //조준해서 줌하기
            {
                if(pm.isCrouching)
                {
                    pm.characterController.height = 1.3f;
                }

                pm.isAim = true;
                multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
                pm.animator.SetLayerWeight(1, 1); //첫번째 레이어 값을 1로 바꿔라(활성화)
                crosshairObj.SetActive(true);
                if (zoomCoroutine != null)
                {
                    StopCoroutine(zoomCoroutine); //실행중인 코루틴 있으면 멈춤
                }
                if (isFirstPerson) //1인칭일 때
                {
                    SetTargetFOV(zoomFov); //타겟 거리를 1인칭 줌 거리로 설정
                    zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //1인칭일 때 카메라 줌 코루틴 실행
                }
                else //3인칭 일때 
                {
                    SetTargetDistance(zoomDistance); //타겟 거리를 줌 거리로 설정
                    zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //3인칭일 때 카메라 줌 코루틴 실행 
                }

            }
            if (Input.GetMouseButtonUp(1)) //돌아오기 
            {
                pm.characterController.height = 1.9f;
                if (zoomCoroutine != null)
                {
                    StopCoroutine(zoomCoroutine); //실행중인 코루틴 있으면 멈춤
                }
                if (isFirstPerson) //1인칭일 때
                {
                    SetTargetFOV(defaultFov); //타겟 거리를 1인칭 줌 거리로 설정
                    zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //1인칭일 때 카메라 줌 코루틴 실행
                }
                else //3인칭 일때 
                {
                    SetTargetDistance(thirdPersonDistance); //타겟 거리를 줌 거리로 설정
                    zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //3인칭일 때 카메라 줌 코루틴 실행 
                }
                pm.isAim = false;
                multiAimConstraint.data.offset = new Vector3(0, 0, 0);
                pm.animator.SetLayerWeight(1, 0);
                crosshairObj.SetActive(false);
            }
        }

    }

    public void UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            //카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pm.pitch, pm.yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            mainCamera.transform.position = pm.gameObject.transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정(플레이어를 바라보는 것이 아니라 살짝 옆을 보게)
            mainCamera.transform.LookAt(pm.gameObject.transform.position + new Vector3(thirdPersonOffset.x, thirdPersonOffset.y, 0));
        }
        else //플레이어가 직접 도는 
        {
            pm.gameObject.transform.rotation = Quaternion.Euler(0f, pm.yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);

            mainCamera.transform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pm.pitch, pm.yaw, 0) * direction;
            mainCamera.transform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();

        }
    }
    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }
    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    IEnumerator ZoomCamera(float targetDistance) //3인칭일 때 카메라 위치를 조정
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) //현재 거리에서 목표거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; //목표거리 도달 후 값 고정
    }

    IEnumerator ZoomFieldOfView(float targetFov) //1인칭일 때(카메라 자체가 움직이는 게 아니라 줌만 되면 되니까)
    {
        while (Mathf.Abs(mainCamera.GetComponent<Camera>().fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.GetComponent<Camera>().fieldOfView = targetFov;
    }


}
