using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Maximum speed in units per second.")]
    public float maxSpeed = 5f;

    [Tooltip("How fast the player accelerates.")]
    public float acceleration = 20f;

    [Tooltip("How fast the player decelerates when no input is given.")]
    public float deceleration = 30f;

    [Tooltip("Deadzone for analog inputs to filter out drift.")]
    public float inputDeadzone = 0.1f;

    [Tooltip("Currently selected device type (PC or Console).")]
    public Enums.Device device = Enums.Device.PC;

    [Header("Input System")]
    [Tooltip("Input action reference for movement (Vector2).")]
    public InputActionReference moveAction;

    [Tooltip("Input action reference for building/attacking")]
    public InputActionReference action;

    [Tooltip("Input action reference for destroying blocks / interacting")]
    public InputActionReference additionalAction;

    [Header("Player position")]
    [Tooltip("Player position")]
    public Vector3Int playerPosition;

    [Header("Aiming")]
    [Tooltip("Camera used for aiming. If null, Camera.main will be used.")]
    public Camera cam;

    [Tooltip("Offset applied to the aim plane on the Z axis relative to the player.")]
    public float aimPlaneZOffset = 0f;

    [Tooltip("Block target sprite.")]
    public Sprite blockTargetSprite;

    [Tooltip("Block position player is looking at")]
    public Vector3Int targetPosition;

    [Tooltip("Max ray length to show block placing target.")]
    public float maxRayLength = 2.0f;

    [Header("Debug")]
    [Tooltip("Whether to draw debug gizmos for the aim ray.")]
    public bool drawGizmo = true;

    [Tooltip("Radius of the debug gizmo sphere at the aim point.")]
    public float gizmoPointRadius = 0.06f;

    [Header("Other")]
    [Tooltip("Ground check distance (not used directly in this script).")]
    public float checkDistance = 3f;

    [Tooltip("Whether the player is currently grounded.")]
    public bool isGrounded;

    [Tooltip("Spawn point to reset the player to when falling.")]
    public GameObject spawnPoint;

    public Vector2 CurrentVelocity => velocity;

    /// <summary>Current world aim point projected onto the XY plane.</summary>
    public Vector3 AimPoint { get; private set; }

    /// <summary>True if a valid aim point was computed this frame.</summary>
    public bool HasAim { get; private set; }

    /// <summary>
    /// Aim angle in degrees, where 0° points to +Y (up), increasing counter-clockwise, normalized to [0, 360).
    /// </summary>
    public float AimAngleDeg;

    /// <summary>
    /// Aim distance in world units from the player position to AimPoint on the XY plane.
    /// </summary>
    public float AimDistance;

    /// <summary>Current facing direction of the player (Up, Down, Left, Right).</summary>
    public Enums.Direction playerDirection = Enums.Direction.Down;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 velocity;
    private GroundDetector groundDetector;

    private Tilemap baseTilemap;
    private BlocksManager baseTilemapBlocksManager;

    private Tilemap objectsTilemap;
    private BlocksManager objectsTilemapBlocksManager;

    private GameObject playerBlockTargetObject;
    private SpriteRenderer playerBlockTargetSpriteRenderer;
    private bool lmbClicked = false;
    private bool rmbClicked = false;
    private int selectedSlot = 0;

    private Equipment eq;

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        groundDetector = GetComponentInChildren<GroundDetector>();
        playerBlockTargetObject = gameObject.transform.Find("PlayerBlockTarget").gameObject;

        if (playerBlockTargetObject)
        {
            playerBlockTargetSpriteRenderer = playerBlockTargetObject.GetComponent<SpriteRenderer>();
            playerBlockTargetSpriteRenderer.sprite = blockTargetSprite;
            playerBlockTargetObject.SetActive(false);
        }

        var mapObject = GameObject.Find("Map");
        if (mapObject)
        {
            var gridObject = mapObject.transform.Find("Grid");
            if (gridObject)
            {
                var baseTilemapObject = gridObject.transform.Find("BaseTilemap");
                if (baseTilemapObject)
                    baseTilemap = baseTilemapObject.GetComponent<Tilemap>();
                else
                    Debug.LogError("Not found 'BaseTilemap' in gameobject tree.");

                if (baseTilemapObject)
                    baseTilemapBlocksManager = baseTilemapObject.GetComponent<BlocksManager>();

                var objectsTilemapObject = gridObject.transform.Find("ObjectsTilemap");
                if (objectsTilemapObject)
                    objectsTilemap = objectsTilemapObject.GetComponent<Tilemap>();
                else
                    Debug.LogError("Not found 'ObjectsTilemap' in gameobject tree.");

                if (objectsTilemapObject)
                    objectsTilemapBlocksManager = objectsTilemapObject.GetComponent<BlocksManager>();
            }
        }

        eq = gameObject.GetComponent<Equipment>();
        if (!eq)
            Debug.LogError("Not found equipment component in Player GameObject.");
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            if (device == Enums.Device.PC)
            {
                var map = moveAction.action.actionMap;
                if (map != null) map.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
            }
            moveAction.action.Enable();
        }
        if (action != null)
        {
            if (device == Enums.Device.PC)
            {
                var map = action.action.actionMap;
                if (map != null) map.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
            }
            action.action.Enable();
        }
        if (additionalAction != null)
        {
            if (device == Enums.Device.PC)
            {
                var map = additionalAction.action.actionMap;
                if (map != null) map.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
            }
            additionalAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            var map = moveAction.action.actionMap;
            if (map != null) map.bindingMask = default;
            moveAction.action.Disable();
        }
        if (action != null)
        {
            var map = action.action.actionMap;
            if (map != null) map.bindingMask = default;
            action.action.Disable();
        }
        if (additionalAction != null)
        {
            var map = additionalAction.action.actionMap;
            if (map != null) map.bindingMask = default;
            additionalAction.action.Disable();
        }
    }

    private void Update()
    {
        Vector2 raw = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        float dz = Mathf.Clamp01(inputDeadzone);
        raw.x = Mathf.Abs(raw.x) < dz ? 0f : raw.x;
        raw.y = Mathf.Abs(raw.y) < dz ? 0f : raw.y;

        input = Vector2.ClampMagnitude(raw, 1f);

        UpdatePlayerDirection(input);

        isGrounded = groundDetector.IsGrounded;
        if (!isGrounded)
        {
            transform.position = new Vector3(
                spawnPoint.transform.position.x,
                spawnPoint.transform.position.y,
                transform.position.z
            );
        }

        ComputeAim();

        playerPosition = baseTilemap.WorldToCell(transform.position);

        DefineBlock();

        HandleBlockPlacement();

        // Selected slot change
        if (Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scrollY) > 0.01f)
            {
                int steps = scrollY > 0 ? 1 : -1;
                ChangeSelectedSlot(steps);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = input * maxSpeed;
        float a = (targetVelocity.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
        Vector2 diff = targetVelocity - velocity;
        Vector2 step = Vector2.ClampMagnitude(diff, a * Time.fixedDeltaTime);
        velocity += step;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) ComputeAim();

        if (!drawGizmo || !HasAim) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, AimPoint);
        Gizmos.DrawSphere(AimPoint, gizmoPointRadius);
    }

    #endregion

    #region Aim

    /// <summary>
    /// Computes the world aim point by projecting the mouse position onto a plane at player Z + offset.
    /// Also updates AimAngleDeg (0° at +Y, CCW).
    /// </summary>
    private void ComputeAim()
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null) { HasAim = false; return; }

        Vector2 pos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Input.mousePosition;

        Vector3 sp = new Vector3(pos.x, pos.y, 0f);
        Vector3 rel = Display.RelativeMouseAt(sp);
        if (rel != Vector3.zero)
        {
            sp.x = rel.x;
            sp.y = rel.y;

            if (c.targetDisplay != (int)rel.z)
            {
                HasAim = false;
                return;
            }
        }

        Ray ray = c.ScreenPointToRay(sp);
        float planeZ = transform.position.z + aimPlaneZOffset;
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, planeZ));

        if (plane.Raycast(ray, out float enter))
        {
            AimPoint = ray.GetPoint(enter);
            HasAim = true;

            // --- Compute angle: 0° = +Y, CCW ---
            Vector2 toAim = (Vector2)(AimPoint - transform.position);
            // Mathf.Atan2 returns angle from +X; swap args to make 0° at +Y.
            float ang = Mathf.Atan2(toAim.x, toAim.y) * Mathf.Rad2Deg;
            if (ang < 0f) ang += 360f;
            AimAngleDeg = ang;
        }
        else
        {
            HasAim = false;
        }

        // Calculating ray length
        if (plane.Raycast(ray, out float hitDist))
        {
            AimPoint = ray.GetPoint(hitDist);
            HasAim = true;
            Vector2 toAim = (Vector2)(AimPoint - transform.position);
            float ang = Mathf.Atan2(toAim.x, toAim.y) * Mathf.Rad2Deg;
            if (ang < 0f) ang += 360f;
            AimAngleDeg = ang;
            AimDistance = toAim.magnitude;
        }
        else
        {
            HasAim = false;
            AimDistance = 0f;
        }
    }

    /// <summary>
    /// Defines block, player is looking at, that can be removed or placed at.
    /// </summary>
    private void DefineBlock()
    {
        //AimAngleDeg
        //Enums.Direction
        if ((AimAngleDeg >= 337.5f && AimAngleDeg <= 360) || (AimAngleDeg >= 0f && AimAngleDeg < 22.5f))
        {
            /*
             * [ ][x][ ]
             * [ ][ ][ ]
             * [ ][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x, playerPosition.y+1, playerPosition.z);
            BlockTarget( new Vector3Int(0, 1, playerPosition.z) );
        }
        else if (AimAngleDeg >= 22.5f && AimAngleDeg < 67.5f)
        {
            /*
             * [ ][ ][x]
             * [ ][ ][ ]
             * [ ][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x+1, playerPosition.y+1, playerPosition.z);
            BlockTarget( new Vector3Int(1, 1, playerPosition.z) );
        }
        else if (AimAngleDeg >= 67.5f && AimAngleDeg < 112.5f)
        {
            /*
             * [ ][ ][ ]
             * [ ][ ][x]
             * [ ][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x+1, playerPosition.y, playerPosition.z);
            BlockTarget( new Vector3Int(1, 0, playerPosition.z) );
        }
        else if (AimAngleDeg >= 112.5f && AimAngleDeg < 157.5f)
        {
            /*
             * [ ][ ][ ]
             * [ ][ ][ ]
             * [ ][ ][x]
             */
            targetPosition = new Vector3Int(playerPosition.x+1, playerPosition.y-1, playerPosition.z);
            BlockTarget( new Vector3Int(1, -1, playerPosition.z) );
        }
        else if (AimAngleDeg >= 157.5f && AimAngleDeg < 202.5f)
        {
            /*
             * [ ][ ][ ]
             * [ ][ ][ ]
             * [ ][x][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x, playerPosition.y-1, playerPosition.z);
            BlockTarget( new Vector3Int(0, -1, playerPosition.z) );
        }
        else if (AimAngleDeg >= 202.5f && AimAngleDeg < 247.5f)
        {
            /*
             * [ ][ ][ ]
             * [ ][ ][ ]
             * [x][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x-1, playerPosition.y-1, playerPosition.z);
            BlockTarget( new Vector3Int(-1, -1, playerPosition.z) );
        }
        else if (AimAngleDeg >= 247.5f && AimAngleDeg < 292.5f)
        {
            /*
             * [ ][ ][ ]
             * [x][ ][ ]
             * [ ][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x-1, playerPosition.y, playerPosition.z);
            BlockTarget( new Vector3Int(-1, 0, playerPosition.z) );
        }
        else if (AimAngleDeg >= 292.5f && AimAngleDeg < 337.5f)
        {
            /*
             * [x][ ][ ]
             * [ ][ ][ ]
             * [ ][ ][ ]
             */
            targetPosition = new Vector3Int(playerPosition.x-1, playerPosition.y+1, playerPosition.z);
            BlockTarget( new Vector3Int(-1, 1, playerPosition.z) );
        }
    }

    /// <summary>
    /// Places UI target sprite on targeted block
    /// </summary>
    private void BlockTarget(Vector3Int targetOffset)
    {
        if (AimDistance <= maxRayLength)
            playerBlockTargetObject.SetActive(true);
        else
        {
            playerBlockTargetObject.SetActive(false);
            return;
        }

        Vector3Int cell = playerPosition + targetOffset;
        Vector3 worldPos = baseTilemap.GetCellCenterWorld(cell);
        playerBlockTargetObject.transform.position = worldPos;
    }

    private void HandleBlockPlacement()
    {
        if (AimDistance > maxRayLength) return;

        bool lmbClickedNow = action != null && action.action.ReadValue<float>() >= 0.5f;
        bool rmbClickedNow = additionalAction != null && additionalAction.action.ReadValue<float>() >= 0.5f;

        // Player clicked left mouse button
        if (lmbClickedNow && !lmbClicked)
        {
            if (selectedSlot < 0 || selectedSlot >= eq.slots.Count || eq.GetItemAtSelectedSlot() == null)
                Debug.LogWarning($"Selected slot {selectedSlot} is invalid or prefab is null.");
            else
            {
                baseTilemapBlocksManager.PlaceBlock(targetPosition, eq.GetItemAtSelectedSlot().BlockGameObject);
            }
        }
        // Player clicked right mouse button
        if (rmbClickedNow && !rmbClicked)
        {
            if (selectedSlot < 0 || selectedSlot >= eq.slots.Count || eq.GetItemAtSelectedSlot() == null)
                Debug.LogWarning($"Selected slot {selectedSlot} is invalid or prefab is null.");
            else
            {
                baseTilemapBlocksManager.DestroyBlock(targetPosition, eq.GetItemAtSelectedSlot().BlockGameObject);
            }
        }
        lmbClicked = lmbClickedNow;
        rmbClicked = rmbClickedNow;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Updates the player's facing direction based on the current movement vector.
    /// Keeps the last direction if there is no input.
    /// </summary>
    /// <param name="dir">Current input vector.</param>
    private void UpdatePlayerDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.0001f) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            playerDirection = dir.x > 0 ? Enums.Direction.Right : Enums.Direction.Left;
        else
            playerDirection = dir.y > 0 ? Enums.Direction.Up : Enums.Direction.Down;
    }

    private void ChangeSelectedSlot(int delta)
    {
        if (eq.slots == null || eq.slots.Count == 0) return;

        int n = eq.slots.Count;
        selectedSlot = ((selectedSlot + delta) % n + n) % n;
        eq.SelectItemAtSlot(selectedSlot);
    }

    #endregion
}