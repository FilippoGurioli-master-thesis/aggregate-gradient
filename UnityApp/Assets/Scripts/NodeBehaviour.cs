using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class NodeBehaviour : MonoBehaviour
{
    private CollektiveEngine _engine;
    private int _id;
    private Renderer _renderer;
    private readonly Color _minColor = Color.blue;
    private readonly Color _maxColor = Color.red;

    public void Initialize(int id, CollektiveEngine engine)
    {
        _id = id;
        _engine = engine;
        _renderer = GetComponent<Renderer>();
    }

    private void Update() => DisplayGradient(_engine.GetValue(_id));

    private void DisplayGradient(int value)
    {
        var t = Mathf.InverseLerp(0f, int.MaxValue, value);
        var color = Color.Lerp(_minColor, _maxColor, t);
        _renderer.material.color = color;
    }
}