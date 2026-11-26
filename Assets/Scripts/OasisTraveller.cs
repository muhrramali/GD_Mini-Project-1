using UnityEngine;

public sealed class OasisTraveller : MonoBehaviour
{
    private Transform leftArm, rightArm, leftLeg, rightLeg;
    private Material cloth, trim, leather;
    private float stride;

    public void Build()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        cloth = new Material(shader) { color = new Color(.10f, .42f, .39f) };
        trim = new Material(shader) { color = new Color(.91f, .65f, .28f) };
        leather = new Material(shader) { color = new Color(.25f, .15f, .10f) };
        Part("Coat", transform, new Vector3(0, 1.12f, 0), new Vector3(.53f, .62f, .29f), cloth);
        Part("Hood", transform, new Vector3(0, 1.62f, 0), new Vector3(.35f, .34f, .34f), trim, PrimitiveType.Sphere);
        Part("Pack", transform, new Vector3(0, 1.16f, -.20f), new Vector3(.37f, .44f, .20f), leather);
        leftArm = Limb("Left arm", -.33f, 1.36f, .46f, cloth);
        rightArm = Limb("Right arm", .33f, 1.36f, .46f, cloth);
        leftLeg = Limb("Left leg", -.14f, .85f, .79f, leather);
        rightLeg = Limb("Right leg", .14f, .85f, .79f, leather);
    }

    public void Animate(float travelled, float delta)
    {
        stride += travelled * 5.5f;
        float angle = travelled > .0001f ? Mathf.Sin(stride) * 30f : 0f;
        Swing(leftLeg, angle, delta);
        Swing(rightLeg, -angle, delta);
        Swing(leftArm, -angle * .65f, delta);
        Swing(rightArm, angle * .65f, delta);
    }

    private static void Swing(Transform limb, float angle, float delta)
    {
        limb.localRotation = Quaternion.Slerp(limb.localRotation, Quaternion.Euler(angle, 0f, 0f), 1f - Mathf.Exp(-18f * delta));
    }

    private Transform Limb(string label, float x, float y, float length, Material material)
    {
        var joint = new GameObject(label).transform;
        joint.SetParent(transform, false);
        joint.localPosition = new Vector3(x, y, 0);
        Part(label + " mesh", joint, new Vector3(0, -length / 2, 0), new Vector3(.16f, length, .17f), material);
        return joint;
    }

    private static void Part(string label, Transform parent, Vector3 position, Vector3 size,
        Material material, PrimitiveType type = PrimitiveType.Cube)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = label;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = size;
        part.GetComponent<Collider>().enabled = false;
        part.GetComponent<Renderer>().sharedMaterial = material;
    }

    private void OnDestroy()
    {
        Destroy(cloth);
        Destroy(trim);
        Destroy(leather);
    }
}
