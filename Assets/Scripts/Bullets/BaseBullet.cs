using Services;
using UnityEngine;
using DG.Tweening;
using System;
using Unity.VisualScripting;

public class BaseBullet : MonoBehaviour
{
    public event EventHandler OnCollisionStart;
    public event EventHandler OnCollisionEnd;

    [SerializeField] protected Transform bulletVisual;

    private Transform currentActiveColumn;

    protected GridSystem gridSystem;
    protected GameManager gameManager;

    protected virtual void Start()
    {
        gridSystem = ServiceLocator.Get<GridSystem>();
        gameManager = ServiceLocator.Get<GameManager>();

        gameManager.OnActiveColumnChanged += GameManager_OnActiveColumnChanged;

        currentActiveColumn = gameManager.GetActiveColumnTransform();
    }

    protected void GameManager_OnActiveColumnChanged(object sender, EventArgs e)
    {
        Debug.Log("GameManager_OnActiveColumnChanged");

        Transform prevActiveColumn = currentActiveColumn;
        currentActiveColumn = gameManager.GetActiveColumnTransform();
        Vector3 vectorMovement = (prevActiveColumn.position - currentActiveColumn.position);

        float dotProductVectorMovement = GetVectorDotProductFromCameraView(vectorMovement);

        GridCell nearestGridCell = gridSystem.GetNearestGridCell(transform.position);
        Debug.LogWarning(nearestGridCell);

        if (dotProductVectorMovement < 0)
        {
            // Going right from camera view
            GridCell rightGridCell = gridSystem.GetRightGridCell(nearestGridCell);
            bool isRightGridCellFree = rightGridCell.gridObject is null;

            if (!isRightGridCellFree)
                return;
        }
        else if (dotProductVectorMovement > 0)
        {
            // Going left from camera view
            GridCell leftGridCell = gridSystem.GetLeftGridCell(nearestGridCell);
            bool isLeftGridCellFree = leftGridCell.gridObject is null;

            if (!isLeftGridCellFree)
                return;
        }

        float distance = vectorMovement.magnitude;
        //Debug.Log(distance);

        if (distance < 2.0f)
        {
            Vector3 newPosition = new Vector3(currentActiveColumn.position.x, transform.position.y, currentActiveColumn.position.z);
            transform.position = newPosition;
        }

    }

    private float GetVectorDotProductFromCameraView(Vector3 vector)
    {
        Vector3 cameraRightVersor = Camera.main.transform.right;
        float dotProduct = Vector3.Dot(vector, cameraRightVersor);
        return dotProduct; // < 0 (left); < 0 (right);
    }

    public virtual void CollisionEffect(BaseBlob collidedBlob)
    {
        Debug.LogError("CollisionEffect not implemented!");
    }

    protected void CollisionStart() => OnCollisionStart?.Invoke(this, EventArgs.Empty);

    protected void CollisionEnd() => OnCollisionEnd?.Invoke(this, EventArgs.Empty);

    protected void DestroyBullet()
    {
        gameManager.OnActiveColumnChanged -= GameManager_OnActiveColumnChanged;
        Destroy(this);
    }

}
