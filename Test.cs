using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 direction; 
    public bool Draw = false;
    public CurvedLineRenderer line;
    void Start()
    {
        line = GetComponent<CurvedLineRenderer>();
    }
    // Update is called once per frame
    void Update(){
        if (Draw){
            line.SetPosition(line.positionCount + 1, transform.position);
            Draw = false;
        }
    }
}
