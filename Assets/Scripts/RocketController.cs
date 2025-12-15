using UnityEngine;
using TMPro; // Для текста победы/поражения

public class RocketController : MonoBehaviour
{
    [Header("Настройки")]
    public float enginePower = 20f;
    public float rotationPower = 100f;
    public float maxLandingSpeed = 2f; // Если скорость выше - разбился
    public float maxLandingAngle = 20f; // Если наклон больше - разбился

    [Header("Ссылки")]
    public Transform spawnPoint; // Точка, куда возвращаться (обычно это позиция над площадкой)
    public GameObject explosionEffect; // Сюда потом сунем партиклы
    public TextMeshProUGUI statusText; // Текст "WIN" или "CRASH"

    private Rigidbody rb;
    private bool isThrusting = false;
    private int rotationDirection = 0;
    private bool isGameActive = true; // Чтобы нельзя было летать после взрыва

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Запоминаем, где стояли в начале (относительно родителя)
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        if (!isGameActive) return; // Если игра кончилась - не управляем

        if (isThrusting)
        {
            rb.AddRelativeForce(Vector3.up * enginePower);
        }

        if (rotationDirection != 0)
        {
            float rot = rotationDirection * rotationPower * Time.fixedDeltaTime;
            transform.Rotate(0, 0, -rot);
        }
    }

    // --- ОБРАБОТКА СТОЛКНОВЕНИЙ ---
    void OnCollisionEnter(Collision collision)
    {
        if (!isGameActive) return; // Чтобы не умирать дважды

        // 1. Проверяем скорость удара
        float impactSpeed = collision.relativeVelocity.magnitude;

        // 2. Проверяем, обо что ударились
        if (collision.gameObject.CompareTag("LandingPad"))
        {
            // Мы коснулись площадки. Проверяем, аккуратно ли?
            float angle = Vector3.Angle(transform.up, Vector3.up);

            if (impactSpeed < maxLandingSpeed && angle < maxLandingAngle)
            {
                Win();
            }
            else
            {
                Crash("Слишком грубая посадка!");
            }
        }
        else
        {
            // Ударились об пол или что-то еще
            Crash("Мимо площадки!");
        }
    }

    void Win()
    {
        Debug.Log("WIN!");
        if (statusText) statusText.text = "УСПЕШНАЯ ПОСАДКА!";
        if (statusText) statusText.color = Color.green;
        isGameActive = false; // Отключаем управление
    }

    void Crash(string reason)
    {
        Debug.Log("BOOM! " + reason);
        if (statusText) statusText.text = "АВАРИЯ:\n" + reason;
        if (statusText) statusText.color = Color.red;

        // Тут можно включить эффект взрыва
        if (explosionEffect) explosionEffect.SetActive(true);

        isGameActive = false; // Отключаем управление
    }

    // --- МЕТОД ДЛЯ КНОПКИ RESET ---
    public void ResetRocket()
    {
        // Сбрасываем физику
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Возвращаем на старт (используем localPosition, чтобы остаться внутри AR уровня)
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;

        // Включаем игру
        isGameActive = true;
        if (statusText) statusText.text = "";
        if (explosionEffect) explosionEffect.SetActive(false);
    }

    // Методы кнопок (без изменений)
    public void StartEngine() => isThrusting = true;
    public void StopEngine() => isThrusting = false;
    public void StartLeft() => rotationDirection = -1;
    public void StopRotate() => rotationDirection = 0;
    public void StartRight() => rotationDirection = 1;
}