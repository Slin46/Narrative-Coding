using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //drag player into this field in the inspector
    public Transform player;
    //drag background into this field in the inspector
    public SpriteRenderer background;
    //how fast the camera follows the player
    public float followSpeed = 5f;

    //min and max boundaries for the cameras positions
    private float minX;
    private float maxX;
    private float cameraY;
    private float cameraZ;

    void Start()
    {
        //grab the camera component
        Camera cam = GetComponent<Camera>();

        cameraY = transform.position.y;
        cameraZ = transform.position.z;

        float halfCameraWidth = cam.orthographicSize * cam.aspect;

        //calculate the min and max x positions for the camera based on the background size
        minX = background.bounds.min.x + halfCameraWidth;
        maxX = background.bounds.max.x - halfCameraWidth;

        //if background is smaller than camera view
        if (minX > maxX)
        {
            minX = maxX = background.bounds.center.x;
        }

        //place camera inside the boundaries immediately
        Vector3 startPosition = transform.position;
        startPosition.x = Mathf.Clamp(startPosition.x, minX, maxX);
        transform.position = startPosition;
    }

    void LateUpdate()
    {
        //if player or background is null, do nothing
        if (player == null || background == null)
            return;

        //clamp to the min and max x positions, basically lock it
        float targetX = Mathf.Clamp(
            player.position.x,
            minX,
            maxX
        );

        float newX = Mathf.Lerp(
            transform.position.x,
            targetX,
            followSpeed * Time.deltaTime
        );

        transform.position = new Vector3(newX, cameraY, cameraZ);

        //make sure camera never exceeds boundaries
        newX = Mathf.Clamp(newX, minX, maxX);

        transform.position = new Vector3(newX, cameraY, cameraZ);
    }
}
