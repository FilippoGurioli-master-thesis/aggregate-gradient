using System.Collections.Generic;
using UnityEngine;

public class LinksManager : MonoBehaviour
{
    [SerializeField] private LinkBehaviour linkPrefab;

    private IEngineWithLinks _engine;
    private readonly Dictionary<(int a, int b), LinkBehaviour> _linksByKey = new();

    private void Start()
    {
        _engine = GetComponent<IEngineWithLinks>();
        RebuildLinks();
    }

    private void Update() => UpdateLinks();

    private void UpdateLinks()
    {
        var toRemove = new List<(int, int)>(_linksByKey.Keys);
        foreach (var (aNode, bNode) in _engine.GetAllLinks())
        {
            var key = MakeKey(aNode.Id, bNode.Id);
            toRemove.Remove(key);
            if (_linksByKey.TryGetValue(key, out var link) && link != null)
                continue;
            var linkInstance = Instantiate(linkPrefab);
            linkInstance.Initialize(aNode, bNode);
            _linksByKey[key] = linkInstance;
        }

        foreach (var key in toRemove)
        {
            if (_linksByKey.TryGetValue(key, out var link) && link != null)
                Destroy(link.gameObject);
            _linksByKey.Remove(key);
        }
    }

    private void RebuildLinks()
    {
        foreach (var kv in _linksByKey)
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        _linksByKey.Clear();
        UpdateLinks();
    }

    private static (int a, int b) MakeKey(int id1, int id2) => id1 < id2 ? (id1, id2) : (id2, id1);
}

public interface IEngineWithLinks
{
    public List<(NodeBehaviour, NodeBehaviour)> GetAllLinks();
}

