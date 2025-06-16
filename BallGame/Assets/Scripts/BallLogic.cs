using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class BallLogic : MonoBehaviour
{
    public UnityEvent OnWin;

    public UnityEvent OnLoss;

    public Transform initialBallSpawnPoint;

    private Rigidbody rigidbody;
    private bool IsGameOver = false;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        ResetBallState();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsGameOver)
        {
            return;
        }

        if (other.CompareTag("Goal"))
        {
            Debug.Log($"Goal reached! Object: {other.name}");
            IsGameOver = true;
            DisableBallPhysics();
            OnWin.Invoke();
        }
        else if (other.CompareTag("DeathPlane"))
        {
            Debug.Log($"Fell off into Death Plane! Object: {other.name}");
            IsGameOver = true;
            DisableBallPhysics();
            OnLoss.Invoke();
        }
    }

    public void ResetBallState()
    {
        if (initialBallSpawnPoint == null)
        {
            Debug.LogError("Initial Ball Spawn Point not assigned in the prefab!");
            return;
        }

        transform.position = initialBallSpawnPoint.position;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        IsGameOver = false;
        EnableBallPhysics();
        Debug.Log("Ball state reset.");
    }

    private void DisableBallPhysics()
    {
        rigidbody.isKinematic = true;
        rigidbody.detectCollisions = false;
    }

    private void EnableBallPhysics()
    {
        rigidbody.isKinematic = false;
        rigidbody.detectCollisions = true;
    }
}