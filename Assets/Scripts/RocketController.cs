using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Настройки полета")]
    public float enginePower = 15f;   // Сила тяги вверх
    public float rotationPower = 100f; // Скорость поворота

    private Rigidbody rb;

    // Переменные, чтобы знать, какие кнопки нажаты прямо сейчас
    private bool isThrusting = false;
    private int rotationDirection = 0; // -1 влево, 0 стоим, 1 вправо

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 1. Летим вверх
        if (isThrusting)
        {
            // Прикладываем силу снизу вверх относительно ракеты
            rb.AddRelativeForce(Vector3.up * enginePower);
        }

        // 2. Поворачиваем
        if (rotationDirection != 0)
        {
            float rot = rotationDirection * rotationPower * Time.fixedDeltaTime;
            // Крутим вокруг оси Z (наклоны влево-вправо)
            transform.Rotate(0, 0, -rot);
        }
    }

    // ЭТИ МЕТОДЫ МЫ ПРИВЯЖЕМ К КНОПКАМ:

    // Газ (Вверх)
    public void StartEngine() => isThrusting = true;
    public void StopEngine() => isThrusting = false;

    // Поворот Влево
    public void StartLeft() => rotationDirection = -1;
    public void StopRotate() => rotationDirection = 0;

    // Поворот Вправо
    public void StartRight() => rotationDirection = 1;
    // StopRotate используем общий
}