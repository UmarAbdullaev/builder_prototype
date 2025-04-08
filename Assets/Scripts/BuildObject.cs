using UnityEngine;

public class BuildObject : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private float scale = 0.5f;
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private Collider objectCollider;
    [SerializeField] private Renderer objectRenderer;

    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material placeAvailableMaterial;
    [SerializeField] private Material placeUnavailableMaterial;

    private bool isInBuildMode;
    private float surfaceRotationY = 0f;

    public LayerMask PlaceableLayer => placeableLayer;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * scale);
    }

    public void MoveToPosition(RaycastHit hit)
    {
        Vector3 offset = hit.normal * (scale / 2f);
        transform.position = hit.point + offset;

        // Align object's up to the surface normal
        Quaternion alignToSurface = Quaternion.FromToRotation(Vector3.up, hit.normal);

        // Rotate around the surface normal
        Quaternion spin = Quaternion.AngleAxis(surfaceRotationY, hit.normal);

        // Final rotation = spin * surface alignment
        transform.rotation = spin * alignToSurface;
    }

    public bool IsPlaceable()
    {
        Vector3 halfExtents = Vector3.one * (scale / 2f);
        Collider[] hits = Physics.OverlapBox(transform.position, halfExtents, Quaternion.identity, ~placeableLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Debug.Log($"Blocked by: {hit.gameObject.name} (Layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
            return false;
        }

        return true;
    }

    public void Highlight(bool enable)
    {
        ApplyMaterial(enable ? highlightedMaterial : defaultMaterial);
    }

    public void SetBuildMode(bool enable)
    {
        isInBuildMode = enable;
        objectCollider.isTrigger = enable;

        ApplyMaterial(enable ? placeAvailableMaterial : defaultMaterial);
    }

    public void EnablePlacementLogic(bool enable)
    {
        enabled = enable;
    }

    public void FinalizePlacement(RaycastHit hit)
    {
        MoveToPosition(hit);
    }

    public void SetSurfaceRotation(float yRotation)
    {
        surfaceRotationY = yRotation;
    }

    private void ApplyMaterial(Material mat)
    {
        if (objectRenderer != null && mat != null)
        {
            objectRenderer.material = mat;
        }
    }

    private void Update()
    {
        if (isInBuildMode)
        {
            ApplyMaterial(IsPlaceable() ? placeAvailableMaterial : placeUnavailableMaterial);
        }
    }
}
