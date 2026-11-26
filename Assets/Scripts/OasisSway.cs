using UnityEngine;

public class OasisSway : MonoBehaviour
{
    [SerializeField] private Vector3 angle = new Vector3(0, 0, 5);
    [SerializeField] private float frequency = 0.35f;
    [SerializeField] private float phase;
    private Quaternion restingRotation;

    private void Awake() => restingRotation = transform.localRotation;

    private void Update()
    {
        float wave = Mathf.Sin((Time.time * frequency + phase) * 2f * Mathf.PI);
        transform.localRotation = restingRotation * Quaternion.Euler(angle * wave);
    }
}
