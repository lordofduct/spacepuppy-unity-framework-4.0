using System;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Graphs
{

    /// <summary>
    /// Implementation of A* algorithm that can act on an IGraph<typeparamref name="T"/>.
    /// 
    /// When reducing the algorithm walks backwards from goal to start. This means calls to 
    /// IGraph.GetNeighbours for graphs that have 1-way connections should return the 
    /// neighbours that can be traversed from towards the node, not from the node to the neighbour.
    /// 
    /// If you'd like to invert this, just invert start and goal. The resulting path with also be inverted 
    /// and should be traversed backwards.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AStarPathResolver<T> : ISteppingPathResolver<T> where T : class
    {

        #region Fields

        private IGraph<T> _graph;
        private IHeuristic<T> _heuristic;

        private BinaryHeap<VertexInfo> _open;
        private HashSet<T> _closed = new HashSet<T>();
        private HashSet<VertexInfo> _tracked = new HashSet<VertexInfo>();
        private List<T> _neighbours = new List<T>();

        private T _start;
        private T _goal;

        private StepPathingResult _calculating;

        #endregion

        #region CONSTRUCTOR

        public AStarPathResolver(IGraph<T> graph, IHeuristic<T> heuristic)
        {
            _graph = graph;
            _heuristic = heuristic;
            _open = new BinaryHeap<VertexInfo>(graph?.Count ?? 4, VertexComparer.Default);
        }

        #endregion

        #region Properties

        public IGraph<T> Graph => _graph;

        public IHeuristic<T> Heuristic => _heuristic;

        public bool IsCalculating => _calculating != StepPathingResult.Idle;

        #endregion

        #region Methods

        public void Configure(IGraph<T> graph, IHeuristic<T> heuristic)
        {
            if (this.IsCalculating) throw new System.InvalidOperationException("Can not configure AStarPathResolver while its in the middle of a calculation.");

            _graph = graph;
            _heuristic = heuristic;
            this.Reset();
        }

        #endregion

        #region IPathResolver Interface

        public T Start
        {
            get { return _start; }
            set
            {
                if (this.IsCalculating) throw new InvalidOperationException("Cannot update start node when calculating.");
                _start = value;
            }
        }

        public T Goal
        {
            get { return _goal; }
            set
            {
                if (this.IsCalculating) throw new InvalidOperationException("Cannot update goal node when calculating.");
                _goal = value;
            }
        }

        public IList<T> Reduce()
        {
            if (this.IsCalculating) throw new InvalidOperationException("PathResolver is already running.");
            if (_graph == null || _heuristic == null || _start == null || _goal == null) throw new InvalidOperationException("PathResolver is not initialized.");

            var lst = new List<T>();
            this.Reduce(lst);
            return lst;
        }

        public int Reduce(IList<T> path)
        {
            if (this.IsCalculating) throw new InvalidOperationException("PathResolver is already running.");
            if (_graph == null || _heuristic == null || _start == null || _goal == null) throw new InvalidOperationException("PathResolver is not initialized.");

            this.Reset();
            _calculating = StepPathingResult.Calculating;

            try
            {
                //we solve this backwards so reconstructing the path is done forwards, also its consistent with how GetNeighbours is expected to work
                _open.Add(this.CreateInfo(_goal, _heuristic.Weight(_goal), _start));

                while (_open.Count > 0)
                {
                    var u = _open.Pop();

                    if (u.Node == _start)
                    {
                        return this.ReconstructPath(path, u);
                    }

                    _closed.Add(u.Node);

                    _graph.GetNeighbours(u.Node, _neighbours);
                    var e = _neighbours.GetEnumerator();
                    while (e.MoveNext())
                    {
                        var n = e.Current;
                        if (_closed.Contains(n)) continue;

                        float g = u.g + _heuristic.Distance(u.Node, n) + _heuristic.Weight(n);

                        int i = GetInfo(_open, n);
                        if (i < 0)
                        {
                            var v = this.CreateInfo(n, g, _start);
                            v.Next = u;
                            _open.Add(v);
                        }
                        else if (g < _open[i].g)
                        {
                            var v = _open[i];
                            v.Next = u;
                            v.g = g;
                            v.f = g + v.h;
                            _open.Update(i);
                        }
                    }
                    _neighbours.Clear();
                }

            }
            finally
            {
                this.Reset();
            }

            return 0;
        }

        private VertexInfo CreateInfo(T node, float g, T goal)
        {
            var v = _pool.GetInstance();
            v.Node = node;
            v.Next = null;
            v.g = g;
            v.h = _heuristic.Distance(node, goal);
            v.f = g + v.h;
            _tracked.Add(v);
            return v;
        }

        private static int GetInfo(BinaryHeap<VertexInfo> heap, T node)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (heap[i].Node == node) return i;
            }
            return -1;
        }

        private int ReconstructPath(IList<T> path, VertexInfo node)
        {
            int cnt = 0;
            while (node.Next != null)
            {
                path.Add(node.Node);
                node = node.Next;
                cnt++;
            }
            path.Add(node.Node);
            return cnt + 1;
        }

        #endregion

        #region ISteppingPathResolver Interface

        private VertexInfo _steppedCompletedParentNode;

        /// <summary>
        /// Start the stepping path resolver for reducing.
        /// </summary>
        public void BeginSteppedReduce()
        {
            if (_calculating != StepPathingResult.Idle) throw new InvalidOperationException("PathResolver is already running.");
            if (_graph == null || _heuristic == null || _start == null || _goal == null) throw new InvalidOperationException("PathResolver is not initialized.");

            _calculating = StepPathingResult.Calculating;

            _open.Clear();
            _closed.Clear();
            _tracked.Clear();
            _neighbours.Clear();

            //we solve this backwards so reconstructing the path is done forwards, also its consistent with how GetNeighbours is expected to work
            _open.Add(this.CreateInfo(_goal, _heuristic.Weight(_goal), _start));
        }

        /// <summary>
        /// Take a step at reducing the path resolver.
        /// </summary>
        /// <returns>Returns true if reached goal.</returns>
        public StepPathingResult Step()
        {
            switch (_calculating)
            {
                case StepPathingResult.Failed:
                    return _calculating;
                case StepPathingResult.Idle:
                    throw new InvalidOperationException("You must begin a SteppingResolver before stepping through it.");
                case StepPathingResult.Calculating:
                    if (_open.Count > 0)
                    {
                        var u = _open.Pop();

                        if (u.Node == _start)
                        {
                            _steppedCompletedParentNode = u;
                            _calculating = StepPathingResult.Complete;
                            return _calculating;
                        }

                        _closed.Add(u.Node);

                        _graph.GetNeighbours(u.Node, _neighbours);
                        var e = _neighbours.GetEnumerator();
                        while (e.MoveNext())
                        {
                            var n = e.Current;
                            if (_closed.Contains(n)) continue;

                            float g = u.g + _heuristic.Distance(u.Node, n) + _heuristic.Weight(n);

                            int i = GetInfo(_open, n);
                            if (i < 0)
                            {
                                var v = this.CreateInfo(n, g, _start);
                                v.Next = u;
                                _open.Add(v);
                            }
                            else if (g < _open[i].g)
                            {
                                var v = _open[i];
                                v.Next = u;
                                v.g = g;
                                v.f = g + v.h;
                                _open.Update(i);
                            }
                        }
                        _neighbours.Clear();
                    }
                    _calculating = _open.Count > 0 ? StepPathingResult.Calculating : StepPathingResult.Failed;
                    return _calculating;
                case StepPathingResult.Complete:
                    return _calculating;
                default:
                    throw new System.InvalidOperationException("AStarPathResolver entered an unknonwn state.");
            }
        }

        /// <summary>
        /// Get the result of reducing the path.
        /// </summary>
        /// <param name="path"></param>
        public int EndSteppedReduce(IList<T> path)
        {
            switch (_calculating)
            {
                case StepPathingResult.Complete:
                    int cnt = this.ReconstructPath(path, _steppedCompletedParentNode);
                    this.Reset();
                    return cnt;
                default:
                    this.Reset();
                    return 0;
            }
        }

        /// <summary>
        /// Reset the resolver so a new Step sequence could be started.
        /// </summary>
        public void Reset()
        {
            _steppedCompletedParentNode = null;

            if (_tracked.Count > 0)
            {
                var e = _tracked.GetEnumerator();
                while (e.MoveNext())
                {
                    _pool.Release(e.Current);
                }
            }
            _open.Clear();
            _closed.Clear();
            _tracked.Clear();
            _calculating = StepPathingResult.Idle;
        }

        #endregion

        #region Special Types

        private static ObjectCachePool<VertexInfo> _pool = new ObjectCachePool<VertexInfo>(-1, () => new VertexInfo(), (v) =>
        {
            v.Node = null;
            v.Next = null;
            v.g = 0f;
            v.h = 0f;
            v.f = 0f;
        });

        private class VertexInfo
        {
            public T Node;
            public VertexInfo Next;
            public float g;
            public float h;
            public float f;
        }

        private class VertexComparer : IComparer<VertexInfo>
        {
            public readonly static VertexComparer Default = new VertexComparer();

            public int Compare(VertexInfo x, VertexInfo y)
            {
                //sort inverted so our 'open' heap pops min
                return y.f.CompareTo(x.f);
            }
        }

        #endregion

    }

}
