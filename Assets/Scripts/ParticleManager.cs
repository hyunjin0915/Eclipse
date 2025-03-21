using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ParticleType
{
    Explosion,
    WeaponFire,
    WeaponSmoke
}
public class ParticleManager : Singleton<ParticleManager>
{
    public Dictionary<ParticleType, ParticleSystem> particleSystemDic = new Dictionary<ParticleType, ParticleSystem>();
    //private Dictionary<ParticleType, Queue<GameObject>> particlePool = new Dictionary<ParticleType, Queue<GameObject>>();   


    public ParticleSystem weaponExplosionParticle;
    public ParticleSystem weaponSmokeParticle;
    public ParticleSystem weaponFireParticle;

    private int poolSize = 30;

    override public  void Awake()
    {
        base.Awake();
        InitializeDic();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeDic()
    {
        particleSystemDic.Add(ParticleType.Explosion, weaponExplosionParticle);
        particleSystemDic.Add(ParticleType.WeaponFire, weaponFireParticle);
        particleSystemDic.Add(ParticleType.WeaponSmoke, weaponSmokeParticle);

/*        foreach (var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleSystemDic[type]);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
            particlePool.Add(type, pool);
        }
*/   }
    
    //파티클 끄는 코루틴
/*    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while(particleSystem.isPlaying)
        {
            yield return null;
        }
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePool[type].Enqueue(particleObj);

    }
*/
/*    public void PlayParticlePool(ParticleType type, Vector3 position,Vector3 scale)
    {
        if (particlePool.ContainsKey(type))
        {
            GameObject particleObj = particlePool[type].Dequeue();
            //큐에 들어있는 파티클 다 꺼내오는 코드도 원래 있어야 함

            if (particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                particleObj.transform.localScale = scale;
                particleObj.SetActive(true);
                particleSystem.Play();
                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }
    }
*/
    public void PlayParticle(ParticleType type, Vector3 position)
    {
        if (particleSystemDic.ContainsKey(type))
        {
            Debug.Log("파티클 재생");
            ParticleSystem particle = Instantiate(particleSystemDic[type], position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, particle.GetComponent<ParticleSystem>().main.duration);
        }
        
    }

    public void PlayParticle(ParticleType type, Vector3 position, Vector3 scale)
    {
        if (particleSystemDic.ContainsKey(type))
        {
            ParticleSystem particle = Instantiate(particleSystemDic[type], position, Quaternion.identity);
            particle.gameObject.transform.localScale = scale;
            particle.Play();
            Destroy(particle.gameObject, particle.GetComponent<ParticleSystem>().main.duration);
        }

    }


}
