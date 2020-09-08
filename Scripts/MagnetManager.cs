using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MagnetManager : MonoBehaviour
{
    private GameObject[] Magnets;

    public GameObject VirtualParticle;
    private GameObject[] VirtualParticles;

    public ComputeShader shader;
    public int samplesPerMagnet = 100;
    public int maxSteps = 50;
    [Range(0.1f, 0.8f)]
    public float displacementPerPoint;
    public bool useGpu = true;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemInfo.supportsComputeShaders && useGpu)
        {
            Debug.Log("Running on GPU");
        }
        else
        {
            Debug.Log("Running on CPU");
        }
        Magnets = GameObject.FindGameObjectsWithTag("Magnet");
        foreach (Vector3 spawn in GetParticleSpawnPoints())
        {
            // Create Particle to hold Line Renderer 
            GameObject Particle = Instantiate<GameObject>(VirtualParticle, this.transform) as GameObject;

            LineRenderer path = Particle.GetComponent<LineRenderer>();
            path.startWidth = 0.05f;
            path.endWidth = 0.05f;

            path.material = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f) }
            );
            path.colorGradient = gradient;

            Particle.transform.parent = this.transform;
        }
        VirtualParticles = GameObject.FindGameObjectsWithTag("Particle");
    }

    //GameObject SpawnMagnet (Vector3 position) { }
    //GameObject SpawnVirtualParticle(Vector3 position) { }
    Vector3[] GetParticleSpawnPoints()
    {
        List<Vector3> points = new List<Vector3>();
        float theta;
        float radius;
        float y;
        float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));// golden angle in radians
        for (int i = 0; i < samplesPerMagnet; i++)
        {
            y = 1 - (i / (float)(samplesPerMagnet - 1)) * 2;// y goes from 1 to -1
            radius = Mathf.Sqrt(1 - y * y);// radius at y
            theta = phi * i; // golden angle increment

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;
            points.Add(new Vector3(x, y, z));
        }
        List<Vector3> Spawns = new List<Vector3>();
        foreach (GameObject Magnet in Magnets)
        {
            foreach (Vector3 point in points)
            {
                // ? Correlate scale to spawn distance ik there is a better way though
                Spawns.Add(((Magnet.transform.localScale.x / 2) * 1.1f) * (Quaternion.Inverse(Magnet.transform.rotation) * point) + Magnet.transform.position);
            }
        }
        return Spawns.ToArray();
    }

    List<List<Vector3>> CalculateParticlePaths_CPU(Vector3[] spawns)
    { // Run Compute Shader    
        List<List<Vector3>> Paths = new List<List<Vector3>>();
        foreach (Vector3 spawn in spawns)
        {
            List<Vector3> Path = new List<Vector3>();
            Path.Add(spawn);
            int step = 0;
            Vector3 point = spawn;
            Vector3 last_point = point;
            while (step <= maxSteps)
            {
                Vector3 temp = new Vector3(0, 0, 0);
                foreach (GameObject mag in Magnets)
                {
                    if ((mag.transform.position - point).magnitude < 1)
                    {
                        break;
                    }
                    // Reverse Rotation of the Magnet    
                    Vector3 place = point - mag.transform.position;
                    place = Quaternion.Inverse(mag.transform.rotation) * place;
                    // Math Function 
                    Vector3 _temp = new Vector3(3 * place.z * place.x, 3 * place.z * place.y, 2 * Mathf.Pow(place.z, 2) - Mathf.Pow(place.x, 2) - Mathf.Pow(place.y, 2));
                    float ot = (mag.GetComponent<Magnet>().moment / (4 * Mathf.PI * Mathf.Pow(place.magnitude, 5)));
                    temp += (mag.transform.rotation * (ot * _temp)); // mag.GetComponent<Magnet>().CalculateB_Field(point);
                }
                point += temp;
                if ((last_point - point).magnitude >= displacementPerPoint)
                { // * To prevent renderind too many points we set a minimum displacement 
                    Path.Add(point);
                    last_point = point;
                }
                step++;
            }
            Paths.Add(Path);
        }
        return Paths;
    }

    Vector4 Quat2Vec4(Quaternion quat)
    {
        return new Vector4(quat.x, quat.y, quat.z, quat.w);
    }
    List<List<Vector3>> CalculateParticlePaths_GPU(Vector3[] spawns)
    { // Run Compute Shader    
      // Get kernel ID, probably 0 
        int kernel = shader.FindKernel("ParticlePath");

        shader.SetInt("MaxSteps", maxSteps);
        shader.SetFloat("DisplacementPerPoint", displacementPerPoint);
        // Group Magnet States into buffers to send to the GPU
        shader.SetInt("MagnetCount", Magnets.Length);
        Vector3[] magnetPositions = new Vector3[Magnets.Length];
        Vector4[] magnetRotations = new Vector4[Magnets.Length];
        float[] magnetMoments = new float[Magnets.Length];
        int index = 0;
        foreach (GameObject mag in Magnets)
        {
            magnetPositions[index] = mag.transform.position;
            magnetRotations[index] = Quat2Vec4(mag.transform.rotation);
            magnetMoments[index] = mag.GetComponent<Magnet>().moment;
            index++;
        }

        ComputeBuffer magnetPositionsBuffer = new ComputeBuffer(magnetPositions.Length, 12);
        ComputeBuffer magnetRotationsBuffer = new ComputeBuffer(magnetRotations.Length, 16);
        ComputeBuffer magnetMomentsBuffer = new ComputeBuffer(magnetMoments.Length, sizeof(float));

        magnetPositionsBuffer.SetData(magnetPositions);
        magnetRotationsBuffer.SetData(magnetRotations);
        magnetMomentsBuffer.SetData(magnetMoments);

        shader.SetBuffer(kernel, "magnetPositions", magnetPositionsBuffer);
        shader.SetBuffer(kernel, "magnetRotations", magnetRotationsBuffer);
        shader.SetBuffer(kernel, "magnetMoments", magnetMomentsBuffer);

        // StartPoint Buffer 
        ComputeBuffer spawnPointBuffer = new ComputeBuffer(spawns.Length, 12);// Number of points , byte size of points, Max Path Length 
        spawnPointBuffer.SetData(spawns);
        shader.SetBuffer(kernel, "spawnPoints", spawnPointBuffer);

        Vector3[] paths = new Vector3[spawns.Length * maxSteps];
        ComputeBuffer finalPathsBuffer = new ComputeBuffer(spawns.Length, spawns.Length * 12 * maxSteps);// Number of points , byte size of points, Max Path Length 
        finalPathsBuffer.SetData(paths);
        shader.SetBuffer(kernel, "paths", finalPathsBuffer);

        // Run compute shader
        shader.Dispatch(kernel, spawns.Length, 1, 1);
        finalPathsBuffer.GetData(paths);

        // Release buffers
        spawnPointBuffer.Release();
        finalPathsBuffer.Release();
        magnetPositionsBuffer.Release();
        magnetRotationsBuffer.Release();
        magnetMomentsBuffer.Release();

        List<List<Vector3>> FinalPaths = new List<List<Vector3>>();

        for (int i = 0; i < paths.Length; i += maxSteps)
        {
            List<Vector3> temp = new List<Vector3>();
            for (int j = 0; j < maxSteps; j++)
            {
                // ? Look into why float3 can have a bool this may speed up conversion to list 
                temp.Add(paths[i + j]);
            }
            FinalPaths.Add(temp);
        }
        return FinalPaths;
    }

    void RenderAvailablePaths(List<List<Vector3>> paths)
    {
        int index = 0;
        foreach (List<Vector3> path in paths)
        {
            if (index == VirtualParticles.Length) { break; }
            GameObject Particle = VirtualParticles[index++];
            Particle.GetComponent<LineRenderer>().positionCount = 0;
            foreach (Vector3 point in path)
            { // Iterate Across Points 
                Particle.GetComponent<LineRenderer>().positionCount++;
                Particle.GetComponent<LineRenderer>().SetPosition(Particle.GetComponent<LineRenderer>().positionCount - 1, point);
            }
        }
    }

    void RenderParticlePaths(List<List<Vector3>> paths)
    {
        int _index = 0;
        // * There must be a path for each particle 
        foreach (GameObject Particle in VirtualParticles)
        {
            Particle.GetComponent<LineRenderer>().positionCount = 0;
            int point_num = 0;
            foreach (Vector3 point in paths[_index])
            { // Iterate Across Points 
                Particle.GetComponent<LineRenderer>().positionCount++;
                Particle.GetComponent<LineRenderer>().SetPosition(Particle.GetComponent<LineRenderer>().positionCount - 1, point);
                point_num++;
            }
            _index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SystemInfo.supportsComputeShaders && useGpu)
        {
            RenderParticlePaths(
                CalculateParticlePaths_GPU(GetParticleSpawnPoints())
                );
        }
        else
        {
            RenderParticlePaths(
                CalculateParticlePaths_CPU(GetParticleSpawnPoints())
                );
        }
    }
}