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
            GameObject Particle = Instantiate<GameObject>(VirtualParticle, this.transform) as GameObject;
            Particle.transform.parent = this.transform;
        }
        VirtualParticles = GameObject.FindGameObjectsWithTag("Particle");
    }

    void SpawnMagnet(Vector3 position){}

    struct MagnetState {
        Vector3 position;
        Quaternion rotation;
        float moment;
        public MagnetState(Vector3 pos, Quaternion rot, float mom){
            this.position = pos;
            this.rotation = rot;
            this.moment = mom;
        }
    }
    
    private void InitParticles(){
        for(int angle = 0; angle<360;angle+=36){
            float x = Mathf.Cos(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
            float y = Mathf.Sin(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
    }}

    List<Vector3> GetParticleSpawnPoints(){
        List<Vector3> Spawns = new List<Vector3>();
        Spawns.Add(new Vector3(0,0,0));
        Spawns.Add(new Vector3(0,0,0));
        foreach(GameObject Magnet in Magnets){}
        return Spawns;   
    }

    List<List<Vector3>> CalculateParticlePaths(List<Vector3> spawns){ // Run Compute Shader 
        // int kernelHandle = shader.FindKernel("CSMain");

        // RenderTexture tex = new RenderTexture(256,256,24);
        // tex.enableRandomWrite = true;
        // tex.Create();

        // Vector3 position = transform.position;
        // // foreach(GameObject mag in mags){
        // //     temp += mag.GetComponent<Magnet>().CalculateB_Field(position);
        // for (int i = 1; i <= MaxSteps; i++){
        //     Vector3 temp = new Vector3(0,0,0);
        //     foreach(GameObject mag in mags){
        //         temp += mag.GetComponent<Magnet>().CalculateB_Field(position);
        //         if ((mag.transform.position - position).magnitude < 1){
        //             path.positionCount = (int)i/SegmentSize;
        //             return;
        //         }
        //     }
        //     position += temp;

        // shader.SetTexture(kernelHandle, "Result", tex);
        // shader.Dispatch(kernelHandle, 256/8, 256/8, 1);   
    }

    void RenderCurvedPaths(List<List<Vector3>> paths){
        int index = 0;
        foreach (GameObject Particle in VirtualParticles){
            Particle.GetComponent<LineRenderer>().positionCount = 0;
            int point_num = 0;
            foreach(Vector3 point in paths[index]){ // Iterate Across Points 
                Particle.GetComponent<LineRenderer>().positionCount++;
                Particle.GetComponent<LineRenderer>().SetPosition(0,point);
                point_num++;
            }
            index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RenderCurvedPaths(CalculateParticlePaths(GetParticleSpawnPoints()));   
    }
}
