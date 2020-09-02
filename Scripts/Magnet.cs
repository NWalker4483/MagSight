using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class Magnet : MonoBehaviour
{
    [Range(0.05f,10.00f)]
    public float moment = 1;
    [Range(2,20)]
    public float AreaOfEffect = 5;
    private float Last_AOE = 1;
    public SphereCollider EffectiveArea;
    private void OnTriggerStay(Collider other){
        if (other.gameObject.GetComponent<VectorPoint>() != null){
            // DrawArrow.ForDebug(transform.position,other.transform.position - transform.position,new Color(1,0,0));
            other.gameObject.GetComponent<VectorPoint>().direction += CalculateB_Field(other.transform.position);
        } else if (other.gameObject.GetComponent<Rigidbody>() != null){
            DrawArrow.ForDebug(transform.position,other.transform.position - transform.position,new Color(1,0,0));
            other.gameObject.GetComponent<Rigidbody>().AddForce(CalculateB_Field(other.transform.position) * Time.deltaTime);
        }
    }
    private void OnTriggerExit(Collider other){
        if (other.gameObject.GetComponent<VirtualParticle>() != null){
            //DrawArrow.ForDebug(transform.position,other.transform.position - transform.position,new Color(1,0,0));
            //other.gameObject.GetComponent<Particle>().Reset();
        }   
    }
    public GameObject VirtualParticle;
    void InitParticles(){
        for(int angle = 0; angle<360;angle+=36){
            float x = Mathf.Cos(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
            float y = Mathf.Sin(angle * Mathf.PI / 180)* 1; //transform.localScale.x;
            GameObject Particle = Instantiate<GameObject>(VirtualParticle, this.transform) as GameObject;
            Particle.transform.parent = this.transform;
            Particle.GetComponent<VirtualParticle>().Init(new Vector3(x,y,1));
    }}

    void Start(){
        EffectiveArea = GetComponent<SphereCollider>();
        InitParticles();
    }

    public List<Vector3> CalculateB_Field(List<Vector3> at){
        List<Vector3> B_Field = new List<Vector3>();
        foreach (var point in at) {
            B_Field.Add(CalculateB_Field(point));
        } 
        return B_Field;
    }
    public Vector3 CalculateB_Field(Vector3 point){
        Vector3 place = point - transform.position;
        place = Quaternion.Inverse(transform.rotation) * place;

        Vector3 temp = new Vector3(3*place.z*place.x,3*place.z*place.y,2*Mathf.Pow(place.z,2) - Mathf.Pow(place.x,2) - Mathf.Pow(place.y,2));
        float ot = (moment/(4*Mathf.PI*Mathf.Pow(place.magnitude,5)));
        return (transform.rotation * (ot * temp));
    }
    // Update is called once per frame
    void Update(){
        if (Last_AOE != AreaOfEffect){
            EffectiveArea.radius = AreaOfEffect;
            Last_AOE = AreaOfEffect;
        }
    }
}
