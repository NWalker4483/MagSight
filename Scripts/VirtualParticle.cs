using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VirtualParticle: MonoBehaviour
{
    public Vector3 SeedPosition = new Vector3(.5f,.5f,.5f);
    public LineRenderer path;
    [Range(10,10000)]
    public int MaxSteps = 10000;
    
    [Range(2,10)]
    public int SegmentSize = 5;
    private bool travelled;
    // Start is called before the first frame update
    GameObject[] mags;
    void Start()
    {
        path = GetComponent<LineRenderer>();
        mags = GameObject.FindGameObjectsWithTag("Magnet");
        //path.material = new Material(Shader.Find("Sprites/Default"));
        path.startWidth = 0.05f;
        path.endWidth = 0.05f;
        transform.position = transform.parent.position + SeedPosition;

        path.material = new Material(Shader.Find("Sprites/Default"));
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f)}
            );
        path.colorGradient = gradient;
    }}
