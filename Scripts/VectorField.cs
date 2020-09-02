using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorField : MonoBehaviour
{
    private List<Vector3> points;
    private List<Vector3> field;
    // Start is called before the first frame update
    public GameObject Prefab; 
    void Start(){
        initZero(GetSurroundingPointField(20));
    }
    public int NeighCnt = 0;
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<Magnet>() != null){
            NeighCnt++;
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.GetComponent<Magnet>() != null){
            NeighCnt--;
        }
    }
    public void initZero(List<Vector3> points){
        for(int i = 0; i < points.Count; i++) 
        {
            GameObject pnt = Instantiate(Prefab,points[i],Quaternion.identity);
            pnt.transform.parent = this.transform;
            pnt.GetComponent<VectorPoint>().SetDefault(new Vector3(0,0,0));
        }
    }
    public void setData(List<Vector3> points, List<Vector3> data){
        for(int i = 0; i < points.Count; i++) 
        {
            GameObject pnt = Instantiate(Prefab,points[i],Quaternion.identity);
            pnt.transform.parent = this.transform;
            pnt.GetComponent<VectorPoint>().SetDefault(data[i]);
        }
        field = data;
    }
    public void Draw(){
        for(int i = 0; i < transform.childCount; i++){
            transform.GetChild(i).GetComponent<VectorPoint>().Draw();
        }
    }
    public List<Vector3> GetSurroundingPointField(int b, float step = 1){
        List<Vector3> f = new List<Vector3>();
        for(float x = -b/2; x < b/2; x+=step){
            for(float y = -b/2; y < b/2; y+=step){
                for(float z = -b/2; z < b/2; z+=step){
                    f.Add(transform.position + new Vector3(x,y,z));
                }
            }
        }
        return f;
    }
    void Update()
    {
        Draw();
    }
}