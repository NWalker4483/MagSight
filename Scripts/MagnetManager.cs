using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MagnetManager : MonoBehaviour {
    private GameObject[] Magnets;

    public GameObject VirtualParticle;
    private GameObject[] VirtualParticles;

    public ComputeShader shader;
    public int ParticlePerMagnetTemp;

    // Start is called before the first frame update
    void Start () {
        Magnets = GameObject.FindGameObjectsWithTag ("Magnet");
        foreach (Vector3 spawn in GetParticleSpawnPoints ()) {
            // Create Particle to hold Line Renderer 
            GameObject Particle = Instantiate<GameObject> (VirtualParticle, this.transform) as GameObject;

            LineRenderer path = Particle.GetComponent<LineRenderer> ();
            path.startWidth = 0.05f;
            path.endWidth = 0.05f;

            path.material = new Material (Shader.Find ("Sprites/Default"));
            Gradient gradient = new Gradient ();
            gradient.SetKeys (
                new GradientColorKey[] { new GradientColorKey (Color.red, 0.0f), new GradientColorKey (Color.blue, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey (1, 0.0f), new GradientAlphaKey (1, 1.0f) }
            );
            path.colorGradient = gradient;

            Particle.transform.parent = this.transform;
        }
        VirtualParticles = GameObject.FindGameObjectsWithTag ("Particle");
    }

    void SpawnMagnet (Vector3 position) { }

    Vector3[] GetParticleSpawnPoints () {
        Vector3[] Spawns = new Vector3[20];
        int i = 0;
        foreach (GameObject Magnet in Magnets) {
            for (int angle = 0; angle < 360; angle += 36) {
                float x = Mathf.Cos (angle * Mathf.PI / 180) * 1; //transform.localScale.x;
                float y = Mathf.Sin (angle * Mathf.PI / 180) * 1; //transform.localScale.x;

                Spawns[i++] = (Magnet.transform.position + new Vector3 (x, y, 1));
            }
        }
        return Spawns;
    }

    List<List<Vector3>> CalculateParticlePaths_CPU (List<Vector3> spawns) { // Run Compute Shader    
        List<List<Vector3>> Paths = new List<List<Vector3>> ();
        foreach (Vector3 spawn in spawns) {
            List<Vector3> Path = new List<Vector3> ();
            Path.Add (spawn);
            int step = 0;
            Vector3 point = spawn;
            while (step <= 1000) {
                Vector3 temp = new Vector3 (0, 0, 0);
                foreach (GameObject mag in Magnets) {
                    if ((mag.transform.position - point).magnitude < 1) {
                        break;
                    }
                    // Reverse Rotation of the Magnet    
                    Vector3 place = point - mag.transform.position;
                    place = Quaternion.Inverse (mag.transform.rotation) * place;
                    // Math Function 
                    Vector3 _temp = new Vector3 (3 * place.z * place.x, 3 * place.z * place.y, 2 * Mathf.Pow (place.z, 2) - Mathf.Pow (place.x, 2) - Mathf.Pow (place.y, 2));
                    float ot = (mag.GetComponent<Magnet> ().moment / (4 * Mathf.PI * Mathf.Pow (place.magnitude, 5)));
                    temp += (mag.transform.rotation * (ot * _temp)); // mag.GetComponent<Magnet>().CalculateB_Field(point);
                }
                point += temp;
                if (step % 50 == 0) { Path.Add (point); }
                step++;
            }
            Paths.Add (Path);
        }
        return Paths;
    }
    Vector4 Quat2Vec4 (Quaternion quat) {
        return new Vector4 (quat.x, quat.y, quat.z, quat.w);
    }
    List<List<Vector3>> CalculateParticlePaths_GPU (Vector3[] spawns) { // Run Compute Shader    
        int kernel = shader.FindKernel ("ParticlePath");
        // Group Magnet States
        shader.SetInt ("MagnetCount", Magnets.Length);
        Vector3[] magnetPositions = new Vector3[Magnets.Length];
        Vector4[] magnetRotations = new Vector4[Magnets.Length];
        float[] magnetMoments = new float[Magnets.Length];
        int index = 0;
        foreach (GameObject mag in Magnets) {
            magnetPositions[index] = mag.transform.position;
            magnetRotations[index] = Quat2Vec4 (mag.transform.rotation);
            magnetMoments[index] = mag.GetComponent<Magnet> ().moment;
            index++;
        }

        ComputeBuffer magnetPositionsBuffer = new ComputeBuffer (magnetPositions.Length, 12);
        ComputeBuffer magnetRotationsBuffer = new ComputeBuffer (magnetRotations.Length, 16);
        ComputeBuffer magnetMomentsBuffer = new ComputeBuffer (magnetMoments.Length, sizeof (float));

        magnetPositionsBuffer.SetData (magnetPositions);
        magnetRotationsBuffer.SetData (magnetRotations);
        magnetMomentsBuffer.SetData (magnetMoments);

        shader.SetBuffer (kernel, "magnetPositions", magnetPositionsBuffer);
        shader.SetBuffer (kernel, "magnetRotations", magnetRotationsBuffer);
        shader.SetBuffer (kernel, "magnetMoments", magnetMomentsBuffer);

        // StartPoint Buffer 
        Vector3[] paths = new Vector3[spawns.Length * 500];
        index = 0;
        for (int i = 0; i < paths.Length; i += 500) {
            paths[i] = spawns[index++];
        }
        ComputeBuffer spawnPointBuffer = new ComputeBuffer (spawns.Length, spawns.Length * 12 * 500);
        spawnPointBuffer.SetData (paths);
        shader.SetBuffer (kernel, "paths", spawnPointBuffer);

        // Run compute shader
        shader.Dispatch (kernel, paths.Length, 1, 1);
        spawnPointBuffer.GetData (paths);

        // Release buffers
        spawnPointBuffer.Release ();
        magnetPositionsBuffer.Release ();
        magnetRotationsBuffer.Release ();
        magnetMomentsBuffer.Release ();

        List<List<Vector3>> Placeholder = new List<List<Vector3>> ();

        for (int i = 0; i < paths.Length; i += 500) {
            List<Vector3> temp = new List<Vector3> ();
            for (int j = 0; i < 500; j++) {
                temp.Add (paths[i + j]);
            }
            Placeholder.Add (temp);
        }
        Debug.Log(Placeholder);
        return Placeholder;
    }

    void RenderCurvedPaths (List<List<Vector3>> paths) {
        int _index = 0;
        foreach (GameObject Particle in VirtualParticles) {
            Particle.GetComponent<LineRenderer> ().positionCount = 0;
            int point_num = 0;
            foreach (Vector3 point in paths[_index]) { // Iterate Across Points 
                Particle.GetComponent<LineRenderer> ().positionCount++;
                Particle.GetComponent<LineRenderer> ().SetPosition (Particle.GetComponent<LineRenderer> ().positionCount - 1, point);
                point_num++;
            }
            _index++;
        }
    }

    // Update is called once per frame
    void Update () {
        if (SystemInfo.supportsComputeShaders) {
            RenderCurvedPaths (CalculateParticlePaths_GPU (GetParticleSpawnPoints ()));
        } else {
            //RenderCurvedPaths (CalculateParticlePaths_CPU (GetParticleSpawnPoints ()));
        }
    }
}