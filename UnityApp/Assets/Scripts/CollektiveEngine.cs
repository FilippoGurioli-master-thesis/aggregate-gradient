using System.Collections.Generic;
using UnityEngine;

public class CollektiveEngine : MonoBehaviour
{
    [SerializeField] private int nodeCount = 10;
    [SerializeField] private int maxDegree = 3;
    [SerializeField] private List<int> sources = new List<int> { 0 };
    [SerializeField] private float timeScale = 0.1f;
    [SerializeField] private int rounds = 10;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float distance = 3f;

    private int _handle;
    private int _currentRound;

    private void Start()
    {
        _handle = CollektiveNativeApi.Create(nodeCount, maxDegree);
        foreach (var source in sources)
            CollektiveNativeApi.SetSource(_handle, source, true);
        Time.timeScale = timeScale;
        CreateNodeGrid();
    }

    private void FixedUpdate() //internal simulation loop -> currently bound to unity | to be uncorrelated
    {
        if (_currentRound >= rounds) return;
        _currentRound++;
        CollektiveNativeApi.Step(_handle, 1);
    }

    private void CreateNodeGrid()
    {
        for (var i = 0; i < nodeCount; i++)
        {
            var position = ComputePositionForNode(i);
            var go = Instantiate(nodePrefab, position, Quaternion.identity);
            var node = go.GetComponent<NodeBehaviour>();
            node.Initialize(i, this);
        }
    }

    private Vector3 ComputePositionForNode(int nodeId) => new(
        nodeId % 5 * distance, (int)(nodeId / 5) * distance, 0);

    public int GetValue(int id) => CollektiveNativeApi.GetValue(_handle, id);
}