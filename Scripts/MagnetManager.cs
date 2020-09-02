using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetManager : MonoBehaviour
{
    private GameObject[] Magnets;
    
    public GameObject VirtualParticle;
    private GameObject[] VirtualParticles;
    
    public ComputeShader shader;
    public int ParticlePerMagnetTemp;
 
    // Start is called before the first frame update
    void Start()
    {
        Magnets = GameObject.FindGameObjectsWithTag("Magnet");
        foreach(Vector3 spawn in GetParticleSpawnPoints()){
            // Create Particle to hold Line Renderer 
            GameObject Particle = Instantiate<GameObject>(VirtualParticle, this.transform) as GameObject;

            LineRenderer path = Particle.GetComponent<LineRenderer>();
            path.startWidth = 0.05f;
            path.endWidth = 0.05f;

            path.material = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f)}
                );
            path.colorGradient = gradient;
    
            Particle.transform.parent = this.transform;
        }
        VirtualParticles = GameObject.FindGameObjectsWithTag("Particle");
    }

    void SpawnMagnet(Vector3 position){}

    struct MagnetState {
        public Vector3 position;
        public Quaternion rotation;
        public float moment;
        public MagnetState(Vector3 pos, Quaternion rot, float mom){
            this.position = pos;
            this.rotation = rot;
            this.moment = mom;
        }
    }
    

    List<Vector3> GetParticleSpawnPoints(){
        List<Vector3> Spawns = new List<Vector3>();
        foreach(GameObject Magnet in Magnets){
            for(int angle = 0; angle<360;angle+=36){
                float x = Mathf.Cos(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
                float y = Mathf.Sin(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
                
                Spawns.Add(Magnet.transform.position + new Vector3(x,y,1));
            }       
        }
        return Spawns;   
    }
    List<MagnetState> ActiveMagnetStates(){
        List<MagnetState> states = new List<MagnetState>();
        foreach(GameObject mag in Magnets){
            states.Add(new MagnetState(mag.transform.position,mag.transform.rotation,mag.GetComponent<Magnet>().moment));
        }
        return states;   
    }
    List<List<Vector3>> CalculateParticlePaths(List<Vector3> spawns, List<MagnetState> states){ // Run Compute Shader    
        List<List<Vector3>> Paths = new List<List<Vector3>>(); 
        foreach (Vector3 spawn in spawns){
            List<Vector3> Path = new List<Vector3>(); 
            Path.Add(spawn);
            int step = 0; 
            Vector3 point = spawn;
            while (step <= 1000){
                Vector3 temp = new Vector3(0,0,0);
                foreach(MagnetState mag in states){ 
                    if ((mag.position - point).magnitude < 1){
                        break;
                    }        
                    // Reverse Rotation of the Magnet    
                    Vector3 place = point - mag.position;
                    place = Quaternion.Inverse(mag.rotation) * place;
                    // Math Function 
                    Vector3 _temp = new Vector3(3*place.z*place.x,3*place.z*place.y,2*Mathf.Pow(place.z,2) - Mathf.Pow(place.x,2) - Mathf.Pow(place.y,2));
                    float ot = (mag.moment/(4*Mathf.PI*Mathf.Pow(place.magnitude,5)));
                    temp +=  (transform.rotation * (ot * _temp)); // mag.GetComponent<Magnet>().CalculateB_Field(point);
                } 
                point += temp;
                if (step % 50 == 0){Path.Add(point);}
                step++;
            }
            Paths.Add(Path);
        }
        return Paths;
    }

    void RenderCurvedPaths(List<List<Vector3>> paths){
        int _index = 0;
        foreach (GameObject Particle in VirtualParticles){
            Particle.GetComponent<LineRenderer>().positionCount = 0;
            int point_num = 0;
            foreach(Vector3 point in paths[_index]){ // Iterate Across Points 
                Particle.GetComponent<LineRenderer>().positionCount++;
                Particle.GetComponent<LineRenderer>().SetPosition(Particle.GetComponent<LineRenderer>().positionCount - 1,point);
                point_num++;
            }
            _index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RenderCurvedPaths(CalculateParticlePaths(GetParticleSpawnPoints(),ActiveMagnetStates()));   
    }
}
