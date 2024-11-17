using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private float jumpForce = 5f; // Zýplama gücü

    private PlayerMotor motor;
    private Rigidbody rb;

    private bool isGrounded; // Karakterin zeminde olup olmadýðýný kontrol eder

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Yukarý doðru zýplama gücü uygula
        isGrounded = false; // Zýpladýktan sonra yere temas edilmiyor
    }

    // Zeminle temas olup olmadýðýný kontrol ediyoruz (OnCollisionEnter ve OnCollisionExit)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Zeminle temas ettiðinde tekrar zýplama mümkün
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // Zeminle temas kesildiðinde zýplama yapýlamaz
        }
    }
}
