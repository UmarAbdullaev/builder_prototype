using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float reachDistance = 5f;
    [SerializeField] private LayerMask buildObjectLayer;
    [SerializeField] private Vector3 holdOffset;
    [SerializeField] private float rotationStep = 45f;

    private BuildObject heldObject = null;
    private BuildObject targetObject = null;
    private float currentRotation = 0f;

    private Vector3 HoldPosition => playerCamera.transform.position + playerCamera.transform.TransformVector(holdOffset);

    private void Update()
    {
        if (heldObject != null)
        {
            UpdateHeldObject();
        }
        else
        {
            CheckForTargetObject();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(HoldPosition, 0.25f);
    }

    private void UpdateHeldObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, heldObject.PlaceableLayer))
        {
            heldObject.MoveToPosition(hit);
            heldObject.EnablePlacementLogic(true);

            HandleRotation();

            if (Input.GetMouseButtonDown(0) && heldObject.IsPlaceable())
            {
                heldObject.FinalizePlacement(hit);
                heldObject.SetBuildMode(false);
                heldObject = null;
            }
        }
        else
        {
            heldObject.transform.position = HoldPosition;
            heldObject.EnablePlacementLogic(false);

            heldObject.Rotation(transform.localRotation);
        }
    }

    private void CheckForTargetObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, buildObjectLayer))
        {
            if (hit.transform.TryGetComponent(out BuildObject buildCandidate))
            {
                if (targetObject != buildCandidate)
                {
                    ClearPreviousHighlight();
                    targetObject = buildCandidate;
                    targetObject.Highlight(true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    heldObject = targetObject;
                    heldObject.SetBuildMode(true);
                    currentRotation = 0f;
                }
            }
        }
        else
        {
            ClearPreviousHighlight();
        }
    }

    private void ClearPreviousHighlight()
    {
        if (targetObject != null)
        {
            targetObject.Highlight(false);
            targetObject = null;
        }
    }

    private void HandleRotation()
    {
        if (heldObject == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            currentRotation += rotationStep;
        }
        else if (scroll < 0f)
        {
            currentRotation -= rotationStep;
        }

        heldObject.SetSurfaceRotation(currentRotation);
    }
}
