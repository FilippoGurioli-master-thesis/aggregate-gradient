using System.Collections.Generic;
using UnityEngine;

public class CollektiveEngine : MonoBehaviour
{
    [SerializeField] private int nodeCount = 10;
    [SerializeField] private int maxDegree = 3;
    [SerializeField] private List<int> sources = new List<int> { 0 };
    [SerializeField] private float timeScale = 0.1f;
    [SerializeField] private int rounds = 10;

    private int _handle;
    private int _currentRound;

    private void Start()
    {
        _handle = CollektiveNativeApi.Create(nodeCount, maxDegree);
        foreach (var source in sources)
            CollektiveNativeApi.SetSource(_handle, source, true);
        Time.timeScale = timeScale;
    }

    private void Update()
    {
        if (_currentRound >= rounds) return;
        _currentRound++;
        CollektiveNativeApi.Step(_handle, 1);
        for (var id = 0; id < nodeCount; id++)
            Debug.Log($"{id} -> {CollektiveNativeApi.GetValue(_handle, id)}");
    }
}