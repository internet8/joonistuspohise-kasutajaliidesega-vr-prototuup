using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HivemindRatController : MonoBehaviour
{
    [Header("GPU instancing")]
    public bool useGPU = false;
    public Mesh ratMesh;
    public Material[] materials;
    List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
    [Header("rat hive")]
    public int ratCount = 5000;
    public float spawnAreaSize = 23f;
    public float defaultHeight = 0.25f;
    public float ratSizeFactor = 1f;
    [Header("general moving")]
    Vector3 acceleration = Vector3.zero;
    public float speed = 0.1f;
    public float force = 0.1f;
    public float rotationSpeed = 0.1f;
    [Header("seeking")]
    public float maxSeekingVeocity = 15;
    public float seekForce = 3;
    public bool enablePlayerSeeking = true;
    public int maxSeekers = 300;
    int seekerCount = 0;
    public float detectionRad = 3f;
    public GameObject player;
    Vector3 desiredVel = Vector3.zero;
    [Header("evade")]
    public float maxEvadingVeocity = 15;
    public bool enableEvading = true;
    public float evadeRadius = 5;
    public float jumpBackcircleRadius = 1;
    public float jumpBackHeightFactor = 0.3f;
    public List<GameObject> lights = new List<GameObject>();
    float jumpBackRadius;
    [Header("wander")]
    public float maxWanderingVelocity = 7;
    public float wanderCircleDist = 5;
    public float wanterCircleRad = 5f;
    public float wanderMaxDisplacement = 0.5f;
    float wanderCircleAngle = 0;
    [Header("bait")]
    public float maxBaitVelocity = 5;
    public float baitForceDivider = 2;
    public bool enableBaitSeeking = true;
    public float baitDetectionRadius = 10;
    public int maxBaitSeekers = 100;
    public float eatingHeight = 1;
    public float eatingDist = 1;
    public List<GameObject> baitObjects = new List<GameObject>();
    List<GameObject> baits = new List<GameObject>();
    List<int> baitSeekers = new List<int>();
    [Header("compute shader")]
    public ComputeShader ratAIComputeShader;
    Rat[] rats;

    public struct Rat
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public int evading;
    }

    void Start()
    {
        InitRatAI();
        InitRatInstancing();
        foreach (GameObject bait in baitObjects)
        {
            AddBait(bait);
        }
    }

    void Update()
    {
        if (useGPU) { ControlRatsGPU(); } else { ControlRatsCPU(); }
        RenderRats();
    }

    public void AddBait(GameObject o)
    {
        baits.Add(o);
        baitSeekers.Add(0);
    }

    public void AddLight(GameObject o)
    {
        lights.Add(o);
    }

    public void RemoveBaits()
    {
        foreach (GameObject o in baits)
        {
            Destroy(o);
        }
        baits = new List<GameObject>();
        baitSeekers = new List<int>();
    }

    void InitRatAI()
    {
        jumpBackRadius = evadeRadius - jumpBackcircleRadius;
        // fill rat arrays
        rats = new Rat[ratCount];
        for (int i = 0; i < rats.Length; i++)
        {
            rats[i] = new Rat();
            Vector3 pos = transform.position;
            rats[i].position = new Vector3(Random.Range(-spawnAreaSize, spawnAreaSize) + pos.x, defaultHeight, Random.Range(-spawnAreaSize, spawnAreaSize) + pos.z);
            rats[i].rotation = Quaternion.Euler(0, 0, 0);
            rats[i].velocity = Vector3.zero;
            rats[i].evading = 0;
        }
    }

    void InitRatInstancing()
    {
        int addedMatricies = 0;
        batches.Add(new List<Matrix4x4>());

        for (int i = 0; i < ratCount; i++)
        {
            if (addedMatricies < 1000)
            {
                batches[batches.Count - 1].Add(Matrix4x4.TRS(rats[i].position, rats[i].rotation, Vector3.one * ratSizeFactor));
                addedMatricies ++;
            } else
            {
                batches.Add(new List<Matrix4x4>());
                addedMatricies = 0;
            }
        }
    }

    void ControlRatsGPU ()
    {
        int vectorSize = sizeof(float) * 3;
        int quaternionSize = sizeof(float) * 4;
        int intSize = sizeof(int);
        int totalSize = vectorSize * 2 + quaternionSize + intSize;

        ComputeBuffer ratsBuffer = new ComputeBuffer(rats.Length, totalSize);
        ratsBuffer.SetData(rats);

        ratAIComputeShader.SetBuffer(0, "rats", ratsBuffer);
        ratAIComputeShader.SetFloat("speed", speed);
        ratAIComputeShader.SetFloat("force", force);
        ratAIComputeShader.Dispatch(0, rats.Length / 8, 8, 1);

        ratsBuffer.GetData(rats);
        ratsBuffer.Dispose();
    }

    void ControlRatsCPU()
    {
        seekerCount = 0;
        if (baits.Count > 0)
        {
            for (int i = 0; i < baitSeekers.Count; i++) { baitSeekers[i] = 0; }
        }
        for (int i = 0; i < rats.Length; i++)
        {
            float maxVelocity = maxWanderingVelocity;
            float ratHeight = defaultHeight;
            Vector3 ratPosition = rats[i].position;
            Vector3 velocity = rats[i].velocity;
            bool evading = rats[i].evading > 0;

            // wander
            acceleration = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            acceleration = Vector3.ClampMagnitude(acceleration, force);

            // seek player
            float distFromPlayer = Vector3.Distance(ratPosition, player.transform.position);
            if (distFromPlayer < detectionRad && !evading && seekerCount < maxSeekers && enablePlayerSeeking)
            {
                seekerCount ++;
                desiredVel = Vector3.Normalize(new Vector3(player.transform.position.x, 0, player.transform.position.z) - new Vector3(ratPosition.x, 0, ratPosition.z)) * speed;
                acceleration = Vector3.Normalize(desiredVel - velocity);
                acceleration = Vector3.ClampMagnitude(acceleration * seekForce, force * seekForce);
                maxVelocity = maxSeekingVeocity;
            }

            // evade player
            if (Vector3.Distance(ratPosition, player.transform.position) < evadeRadius && enableEvading)
            {
                evading = true;
                desiredVel = Vector3.Normalize(new Vector3(ratPosition.x, 0, ratPosition.z) - new Vector3(player.transform.position.x, 0, player.transform.position.z)) * speed;
                acceleration = Vector3.Normalize(desiredVel - velocity);
                acceleration = Vector3.ClampMagnitude(acceleration, force * 3);
                maxVelocity = maxEvadingVeocity;
            }
            else if (evading)
            {
                evading = false;
            }

            // bait
            if (baits.Count > 0 && !evading && enableBaitSeeking)
            {
                for (int b = 0; b < baits.Count; b++)
                {
                    // seek bait
                    float dist = Vector3.Distance(ratPosition, baits[b].transform.position);

                    if (baitSeekers[b] < maxBaitSeekers && (dist < distFromPlayer || !enablePlayerSeeking))
                    {
                        if (dist < eatingDist)
                        {
                            ratHeight += (eatingDist - dist) * eatingHeight;
                            break;
                        }
                        else if (dist < baitDetectionRadius)
                        {
                            baitSeekers[b] ++;
                            desiredVel = Vector3.Normalize(new Vector3(baits[b].transform.position.x, 0, baits[b].transform.position.z) - new Vector3(ratPosition.x, 0, ratPosition.z)) * speed;
                            acceleration = Vector3.Normalize(desiredVel - velocity);
                            acceleration = Vector3.ClampMagnitude(acceleration, force / baitForceDivider);
                            maxVelocity = maxBaitVelocity;
                            break;
                        }
                    }
                }
            }

            // jump if evading
            if (evading)
            {
                float jumpHeight = 0;
                float distFromEvadingArea = Mathf.Abs(Vector3.Distance(ratPosition, player.transform.position) - jumpBackRadius);
                if (distFromEvadingArea < jumpBackcircleRadius / 2)
                {
                    jumpHeight = distFromEvadingArea;
                }
                else if (distFromEvadingArea < jumpBackcircleRadius)
                {
                    jumpHeight = jumpBackcircleRadius - distFromEvadingArea;
                }
                ratHeight += jumpHeight * jumpBackHeightFactor;
            }

            // light (or other things rats hate)
            if (lights.Count > 0)
            {
                for (int l = 0; l < lights.Count; l++)
                {
                    if (Vector3.Distance(ratPosition, lights[l].transform.position) < evadeRadius && enableEvading)
                    {
                        // evade light
                        evading = true;
                        desiredVel = Vector3.Normalize(new Vector3(ratPosition.x, 0, ratPosition.z) - new Vector3(lights[l].transform.position.x, 0, lights[l].transform.position.z)) * speed;
                        acceleration = Vector3.Normalize(desiredVel - velocity);
                        acceleration = Vector3.ClampMagnitude(acceleration, force * 3);
                        maxVelocity = maxEvadingVeocity;
                    }
                }
            }

            // avoiding collision
            RaycastHit hit;
            if (Physics.Raycast(ratPosition, velocity, out hit, 3))
            {
                //Debug.DrawRay(ratPosition, velocity * hit.distance, Color.yellow);
                if (hit.transform.tag == "Wall")
                {
                    velocity = velocity * -1;
                    //desiredVel = Vector3.Normalize(velocity * -1) * speed;
                    //acceleration = Vector3.Normalize(desiredVel - velocity);
                    //acceleration = Vector3.ClampMagnitude(acceleration, force);
                }
            }

            // moving the rat object
            velocity += acceleration * Time.deltaTime * 75;
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
            rats[i].velocity = velocity;
            ratPosition = ratPosition + velocity * Time.deltaTime;
            ratPosition = new Vector3(ratPosition.x, ratHeight, ratPosition.z);
            rats[i].position = ratPosition;
            rats[i].evading = evading ? 1 : 0;

            // rotating the rat object
            Quaternion ratRotation;
            Quaternion lookRot = Vector3.Distance(ratPosition, player.transform.position) < detectionRad && seekerCount < maxSeekers && enablePlayerSeeking ? Quaternion.LookRotation(new Vector3(player.transform.position.x, 0, player.transform.position.z) - new Vector3(ratPosition.x, 0, ratPosition.z)) : Quaternion.LookRotation(velocity);
            ratRotation = Quaternion.Slerp(rats[i].rotation, lookRot, Mathf.Clamp01(rotationSpeed * Time.maximumDeltaTime));
            rats[i].rotation = ratRotation;

        }
    }

    void RenderRats()
    {
        int addedMatricies = 0;
        int batchIndex = 0;

        for (int i = 0; i < ratCount; i++)
        {
            if (addedMatricies < 1000)
            {
                batches[batchIndex][addedMatricies] = Matrix4x4.TRS(rats[i].position, rats[i].rotation, Vector3.one * ratSizeFactor);
                addedMatricies++;
            }
            else
            {
                batchIndex++;
                addedMatricies = 0;
            }
        }

        foreach (var batch in batches)
        {
            for (int i = 0; i < ratMesh.subMeshCount; i++)
            {
                Graphics.DrawMeshInstanced(ratMesh, i, materials[i], batch);
            }
        }
    }

    private void OnDestroy()
    {
        
    }
}
