using UnityEngine;
using UnityEngine.InputSystem;

public class OasisExplorer : MonoBehaviour
{
    [SerializeField] private float speed = 9f;
    [SerializeField] private float turnSpeed = 65f;
    [SerializeField] private Transform[] viewpoints;
    [SerializeField] private Terrain ground;
    [SerializeField] private float horizontalLimit = 82f;
    [SerializeField] private float ceiling = 30f;
    [SerializeField] private float waterLevel = 2.05f;
    [SerializeField] private float groundClearance = 1.1f;
    [SerializeField] private float walkingSpeed = 4.5f;
    [SerializeField] private float runningSpeed = 7.5f;
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float followHeight = 2.7f;
    [SerializeField] private GameObject flightPlayerPrefab;
    [SerializeField] private Material flightPlayerMaterial;
    private GameObject flightPlayer;
    private BasicBehaviour flightCharacter;
    private Camera explorerCamera;
    private AudioListener explorerAudio;
    private bool restoreExplorerAudio;
    private OasisTraveller traveller;
    private CharacterController controller;
    private bool walking;
    private float walkYaw;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        explorerCamera = GetComponent<Camera>();
        explorerAudio = GetComponent<AudioListener>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        var keys = Keyboard.current;
        if (keys == null) return;

        if (keys.bKey.wasPressedThisFrame)
        {
            SetFlightPlayer(flightPlayer == null);
            return;
        }

        if (keys.digit1Key.wasPressedThisFrame) Snap(0);
        if (keys.digit2Key.wasPressedThisFrame) Snap(1);
        if (keys.digit3Key.wasPressedThisFrame) Snap(2);
        if (keys.rKey.wasPressedThisFrame)
        {
            SetFlightPlayer(false);
            SetWalking(false);
            transform.SetPositionAndRotation(startPosition, startRotation);
        }

        if (flightPlayer != null)
        {
            if (flightCharacter.transform.position.y < -12f)
            {
                SetFlightPlayer(false);
                SetFlightPlayer(true);
            }
            return;
        }

        if (keys.vKey.wasPressedThisFrame) SetWalking(!walking);
        if (walking)
        {
            Walk(keys);
            return;
        }

        float lateral = Axis(keys.dKey.isPressed, keys.aKey.isPressed);
        float vertical = Axis(keys.eKey.isPressed, keys.qKey.isPressed);
        float forward = Axis(keys.wKey.isPressed, keys.sKey.isPressed);
        var heading = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        var motion = heading * Vector3.ClampMagnitude(new Vector3(lateral, vertical, forward), 1f);
        var next = transform.position + motion * (speed * Time.deltaTime);
        next.x = Mathf.Clamp(next.x, -horizontalLimit, horizontalLimit);
        next.z = Mathf.Clamp(next.z, -horizontalLimit, horizontalLimit);
        float floor = ground == null ? 0 : ground.SampleHeight(next) + ground.transform.position.y;
        next.y = Mathf.Clamp(next.y, Mathf.Max(floor, waterLevel) + groundClearance, ceiling);
        transform.position = next;
        transform.Rotate(Vector3.up, Axis(keys.rightArrowKey.isPressed, keys.leftArrowKey.isPressed)
            * turnSpeed * Time.deltaTime, Space.World);
    }

    private void SetFlightPlayer(bool active)
    {
        if (!active)
        {
            if (flightPlayer == null) return;
            flightPlayer.SetActive(false);
            Destroy(flightPlayer);
            flightPlayer = null;
            flightCharacter = null;
            explorerCamera.enabled = true;
            if (explorerAudio != null) explorerAudio.enabled = restoreExplorerAudio;
            return;
        }

        if (flightPlayerPrefab == null) return;

        var instance = new GameObject("Third-person flight");
        instance.SetActive(false);
        var player = Instantiate(flightPlayerPrefab, instance.transform);
        var character = player.GetComponentInChildren<BasicBehaviour>(true);
        if (character == null || character.playerCamera == null)
        {
            Debug.LogError("The third-person flight prefab requires a player and orbit camera.", this);
            Destroy(instance);
            return;
        }

        var orientation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Vector3 spawn = walking ? traveller.transform.position : startPosition;
        spawn.y = GroundHeight(spawn) + .15f;
        character.transform.SetPositionAndRotation(spawn, orientation);
        foreach (var renderer in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (flightPlayerMaterial == null) break;
            var materials = renderer.sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
                materials[slot] = flightPlayerMaterial;
            renderer.sharedMaterials = materials;
        }

        SetWalking(false);
        explorerCamera.enabled = false;
        if (explorerAudio != null)
        {
            restoreExplorerAudio = explorerAudio.enabled;
            explorerAudio.enabled = false;
        }
        flightPlayer = instance;
        flightCharacter = character;
        instance.SetActive(true);
    }

    private void OnDestroy()
    {
        if (flightPlayer != null) Destroy(flightPlayer);
    }

    private void SetWalking(bool active)
    {
        if (walking == active) return;
        walking = active;
        if (!active)
        {
            traveller.gameObject.SetActive(false);
            return;
        }

        walkYaw = transform.eulerAngles.y;
        if (traveller == null)
        {
            var actor = new GameObject("Caravan traveller");
            traveller = actor.AddComponent<OasisTraveller>();
            traveller.Build();
            controller = actor.AddComponent<CharacterController>();
            controller.center = new Vector3(0, .88f, 0);
            controller.height = 1.76f;
            controller.radius = .30f;
            Vector3 spawn = startPosition + Quaternion.Euler(0f, walkYaw, 0f) * Vector3.forward * 5f;
            spawn.y = GroundHeight(spawn) + .08f;
            actor.transform.SetPositionAndRotation(spawn, Quaternion.Euler(0f, walkYaw, 0f));
        }
        traveller.gameObject.SetActive(true);
        Follow();
    }

    private void Walk(Keyboard keys)
    {
        float delta = Time.deltaTime;
        walkYaw += Axis(keys.rightArrowKey.isPressed, keys.leftArrowKey.isPressed) * turnSpeed * delta;
        var input = new Vector3(Axis(keys.dKey.isPressed, keys.aKey.isPressed), 0,
            Axis(keys.wKey.isPressed, keys.sKey.isPressed));
        var direction = Quaternion.Euler(0f, walkYaw, 0f) * Vector3.ClampMagnitude(input, 1f);
        float pace = keys.leftShiftKey.isPressed || keys.rightShiftKey.isPressed ? runningSpeed : walkingSpeed;
        var actor = traveller.transform;
        Vector3 before = actor.position;
        Vector3 next = before + direction * (pace * delta);
        next.x = Mathf.Clamp(next.x, -horizontalLimit, horizontalLimit);
        next.z = Mathf.Clamp(next.z, -horizontalLimit, horizontalLimit);
        if (GroundHeight(next) < waterLevel) next = before;
        controller.Move(next - before + Vector3.down * (12f * delta));
        if (direction.sqrMagnitude > .001f)
            actor.rotation = Quaternion.RotateTowards(actor.rotation, Quaternion.LookRotation(direction), 540f * delta);
        Vector3 displacement = actor.position - before;
        displacement.y = 0;
        traveller.Animate(displacement.magnitude, delta);
        Follow();
    }

    private float GroundHeight(Vector3 position) =>
        ground == null ? 0f : ground.SampleHeight(position) + ground.transform.position.y;

    private void Follow()
    {
        Vector3 target = traveller.transform.position + Vector3.up * 1.25f;
        Vector3 offset = Quaternion.Euler(0f, walkYaw, 0f) * Vector3.back * followDistance;
        Vector3 cameraPosition = traveller.transform.position + offset + Vector3.up * followHeight;
        cameraPosition.y = Mathf.Max(cameraPosition.y, GroundHeight(cameraPosition) + groundClearance);
        transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(target - cameraPosition));
    }

    private static float Axis(bool positive, bool negative) =>
        (positive ? 1f : 0f) - (negative ? 1f : 0f);

    private void Snap(int index)
    {
        if (viewpoints == null || index >= viewpoints.Length || viewpoints[index] == null) return;
        SetWalking(false);
        SetFlightPlayer(false);
        var view = viewpoints[index];
        transform.SetPositionAndRotation(view.position, view.rotation);
    }
}



