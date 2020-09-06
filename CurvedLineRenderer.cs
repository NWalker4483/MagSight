// Code from https://forum.unity.com/threads/easy-curved-line-renderer-free-utility.391219/
// and https://github.com/gpvigano/EasyCurvedLine

using System.Collections.Generic;
using System;
using UnityEngine;

    /// <summary>
    /// Render in 3D a curved line based on its control points.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class CurvedLineRenderer : MonoBehaviour
    {
        /// <summary>
        /// Size of line segments (in meters) used to approximate the curve.
        /// </summary>
        [Tooltip("Size of line segments (in meters) used to approximate the curve")]
        public float lineSegmentSize = 0.15f;
        /// <summary>
        /// Thickness of the line (initial thickness if useCustomEndWidth is true).
        /// </summary>
        [Tooltip("Width of the line (initial width if useCustomEndWidth is true)")]
        public float startWidth = 0.1f;
        /// <summary>
        /// Use a different thickness for the line end.
        /// </summary>
        [Tooltip("Enable this to set a custom width for the line end")]
        public bool useCustomEndWidth = false;
        /// <summary>
        /// Thickness of the line at its end point (initial thickness is lineWidth).
        /// </summary>
        [Tooltip("Custom width for the line end")]
        public float endWidth = 0.1f;
        public int _positionCount = 0;
            public int positionCount {
            get{return _positionCount;}
            set{
                _positionCount = value;
                Vector3[] temp = new Vector3[positionCount];
                int old_size = linePositions.Length;
                for (int i = 0; i < positionCount; i++) {
                    if (i < old_size){
                        temp[i] = linePositions[i];
                    } else{
                        temp[i] = new Vector3(0,0,0);
                    }
                }
                linePositions = temp;
                //SetPointsToLine();
            }
        }
        [Header("Gizmos")]
        /// <summary>
        /// Show gizmos at control points in Unity Editor.
        /// </summary>
        [Tooltip("Show gizmos at control points.")]
        public bool showGizmos = true;
        /// <summary>
        /// Size of the gizmos of control points.
        /// </summary>
        [Tooltip("Size of the gizmos of control points.")]
        public float gizmoSize = 0.1f;
        /// <summary>
        /// Color for rendering the gizmos of control points.
        /// </summary>
        [Tooltip("Color for rendering the gizmos of control points.")]
        // public Color gizmoColor = new Color(1, 0, 0, 0.5f);
        public Vector3[] linePositions = new Vector3[0];
        // private Vector3[] linePositionsOld = new Vector3[0];
        private LineRenderer lineRenderer = null;
        private Material lineRendererMaterial;

        /// <summary>
        /// Collect control points positions and update the line renderer.
        /// </summary>
        public bool Live;
        public void Update()
        {
            if (Live){
                SetPointsToLine();
            }
            UpdateMaterial();
        }
        private void Start() {
            lineRenderer = GetComponent<LineRenderer>();
            lineRendererMaterial = new Material(Shader.Find("Sprites/Default"));
            positionCount = 0;
        }
        public void SetPosition(int index, Vector3 position){
            if(linePositions.Length <= index){
                positionCount = index + 1;
            }
            linePositions[index] = position;
            SetPointsToLine();
        }
        private void SetPointsToLine()
        {
            if (lineRenderer==null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            //get smoothed values
            Vector3[] smoothedPoints = EasyCurvedLine.LineSmoother.SmoothLine(linePositions, lineSegmentSize);

            //set line settings
            lineRenderer.positionCount = smoothedPoints.Length;
            lineRenderer.SetPositions(smoothedPoints);
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = useCustomEndWidth ? endWidth : startWidth;
        }

        private void UpdateMaterial()
        {
            if (lineRenderer==null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            Material lineMaterial = lineRenderer.sharedMaterial;
            if (lineRendererMaterial != lineMaterial)
            {
                if (lineMaterial != null)
                {
                    lineRenderer.generateLightingData = !lineMaterial.shader.name.StartsWith("Unlit");
                }
                else
                {
                    lineRenderer.generateLightingData = false;
                }
            }
            lineRendererMaterial = lineMaterial;
        }
    }
