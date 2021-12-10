#pragma warning disable 0414
#if AG_ASTAR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Pathfinding;

namespace com.spacepuppy.Pathfinding
{

    [RequireComponent(typeof(GraphUpdateScene))]
    public class DynamicGUO : MonoBehaviour
    {

        private Vector3 _lastPos;
        private GraphUpdateScene _graph;
        
        private GraphUpdateObject _guo;

        private float _t = 0f;

        private void Start()
        {
            _lastPos = this.transform.position;
            _graph = this.GetComponent<GraphUpdateScene>();
            
            this.Sync();
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if(_t > 0.5f)
            {
                this.Sync();
                _t = 0f;
            }
        }

        private void Sync()
        {
            if (_guo != null) AstarPath.active.UpdateGraphs(_guo);

            _guo = _graph.GetGUO();
            AstarPath.active.UpdateGraphs(_guo);

            _graph.InvertSettings();
            _guo = _graph.GetGUO();
            _graph.InvertSettings();
        }

    }

}

#endif