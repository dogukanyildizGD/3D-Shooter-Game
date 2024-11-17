using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private float jumpForce = 5f; // Z�plama g�c�

    private PlayerMotor motor;
    private Rigidbody rb;

    private bool isGrounded; // Karakterin zeminde olup olmad���n� kontrol eder

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Yukar� do�ru z�plama g�c� uygula
        isGrounded = false; // Z�plad�ktan sonra yere temas edilmiyor
    }

    // Zeminle temas olup olmad���n� kontrol ediyoruz (OnCollisionEnter ve OnCollisionExit)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Zeminle temas etti�inde tekrar z�plama m�mk�n
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // Zeminle temas kesildi�inde z�plama yap�lamaz
        }
    }
}
