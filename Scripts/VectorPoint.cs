using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorPoint : MonoBehaviour
{
    public Vector3 direction;
    private Vector3 default_value;
    public LineRenderer line;
    void Start(){
             line = GetComponent<LineRenderer>();
             line.material = new Material(Shader.Find("Sprites/Default"));
             
         }
    public void SetDefault(Vector3 value){
        default_value = value;
        direction = default_value;
    }
    private void ForBuild(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f){
        line.positionCount = 0;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 5;
        line.SetPosition(0, transform.position);
        /////
        Color color = new Color(1,1,1);
        line.startColor = color;
        line.endColor = color;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        line.SetPosition(1, pos + direction);
        line.SetPosition(2, pos + direction + (right * .25f));
        line.SetPosition(3, pos + direction);
        line.SetPosition(4, pos + direction + (left * .25f));
    }
     private void ForBuild(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f){
        line.positionCount = 0;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 5;
        line.SetPosition(0, transform.position);
        ///                           ///
        line.startColor = color;
        line.endColor = color;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        line.SetPosition(1, pos + direction);
        line.SetPosition(2, pos + direction + (right * .25f));
        line.SetPosition(3, pos + direction);
        line.SetPosition(4, pos + direction + (left * .25f));
    }
    public void Draw(){
        if (direction.magnitude != 0){
            Vector3 norm = direction;
            norm.Normalize();
            Color color = new Color(Mathf.Abs(norm.x),Mathf.Abs(norm.y),Mathf.Abs(norm.z));
            ForBuild(transform.position,norm,color);
        } else {
            line.positionCount = 0;
        }
        direction = default_value;
    }
    void Update(){}
}
