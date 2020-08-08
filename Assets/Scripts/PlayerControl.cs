using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerControl : MonoBehaviour
{
    public float sensitivity = 4.0f;
    public float speed = 4.0f;

    [SerializeField]
    private Transform head = null;


    private Vector3 angles;
    private Rigidbody draggingObject;

    private void Reset()
    {
        if (head == null)
        {
            head = this.transform;
        }
    }

    private void Start()
    {
        if (head == null)
        {
            this.enabled = false;
            return;
        }

        angles = this.transform.eulerAngles;
    }

    private void Update()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Camera Control
        if (Input.GetMouseButton((int)MouseButton.RightMouse))
        {
            // Move
            {
                float boost = 1.0f;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    boost = 5.0f;
                }

                float FB = Input.GetAxis("Vertical");
                float LR = Input.GetAxis("Horizontal");
                Vector3 direction = Camera.main.transform.TransformDirection(new Vector3(LR, 0, FB));
                head.transform.position += direction * Time.deltaTime * speed * boost;
            }

            // Rotate
            {
                float rotY = Input.GetAxis("Mouse X") * sensitivity;
                float rotX = -Input.GetAxis("Mouse Y") * sensitivity;
                angles.y = (angles.y + rotY) % 360;
                angles.x = Mathf.Clamp(angles.x + rotX, -89, 89);
                head.transform.eulerAngles = angles;
            }
        }

        // Drag Objects
        {
            if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
            {
                Physics.Raycast(cursorRay, out RaycastHit hit);
                Debug.DrawLine(cursorRay.origin, hit.point, Color.red, 5);
                if (hit.rigidbody != null)
                {
                    draggingObject = hit.rigidbody;
                }
            }
            if (Input.GetMouseButtonUp((int)MouseButton.LeftMouse))
            {
                draggingObject = null;
            }

            if (draggingObject != null)
            {
                Vector3 posFromOrigin = draggingObject.position - cursorRay.origin;
                Vector3 newPosFromOrigin = Vector3.Project(posFromOrigin, cursorRay.direction);
                draggingObject.position = cursorRay.origin + newPosFromOrigin;
                Debug.DrawRay(cursorRay.origin, draggingObject.position, Color.green);
            }
        }

        // Scaling Objects
        if (draggingObject != null)
        {
            Vector2 scroll = Input.mouseScrollDelta;
            if (scroll.y != 0)
            {
                Physics.Raycast(cursorRay, out RaycastHit hit);
                if (hit.rigidbody != null)
                {
                    float scale = (1 + scroll.y);
                    hit.transform.localScale *= scale;
                    hit.rigidbody.mass *= scale;
                }
            }
        }
    }
}