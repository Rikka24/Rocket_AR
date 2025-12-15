using UnityEngine;
using TMPro;

public class RocketController : MonoBehaviour
{
    [Header("Настройки")]
    public float enginePower = 25f;
    public float rotationPower = 90f;

    [Header("Условия Посадки")]
    public float maxLandingSpeed = 1.5f;
    public float maxLandingAngle = 20f;
    public float timeToLand = 2.0f;

    [Header("Ссылки")]
    public ParticleSystem explosionEffect;
    public TextMeshProUGUI statusText;
    public ParticleSystem engineParticles;

    private Rigidbody rb;
    private bool isThrusting = false;
    private int rotationDirection = 0;

    private bool isGameActive = true;
    private bool hasLanded = false;

    private float landingTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ResetRocket();
    }

    void FixedUpdate()
    {
        if (!isGameActive) return;

        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        // Если мы уже коснулись базы (hasLanded) - МЫ ВЫХОДИМ ИЗ УПРАВЛЕНИЯ.
        // Кнопки больше не работают. Работает только проверка таймера.
        if (hasLanded)
        {
            CheckStability();
            return; // <--- Вот этот return запрещает дальнейший код (газ и поворот)
        }
        // -------------------------

        // УПРАВЛЕНИЕ (Работает только пока мы в воздухе)

        // 1. Газ
        if (isThrusting)
        {
            rb.AddRelativeForce(Vector3.up * enginePower);
            if (engineParticles && !engineParticles.isPlaying) engineParticles.Play();
        }
        else
        {
            if (engineParticles) engineParticles.Stop();
        }

        // 2. Поворот
        if (rotationDirection != 0)
        {
            float rot = rotationDirection * rotationPower * Time.fixedDeltaTime;
            transform.Rotate(0, 0, -rot);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isGameActive || hasLanded) return;

        // 1. Смерть от скорости
        if (collision.relativeVelocity.magnitude > maxLandingSpeed)
        {
            Crash("Слишком быстро!\nСкорость: " + collision.relativeVelocity.magnitude.ToString("F1"));
            return;
        }

        // 2. Смерть от промаха
        if (!collision.gameObject.CompareTag("LandingPad"))
        {
            Crash("Мимо площадки!");
            return;
        }

        // 3. Начало посадки
        // Коснулись базы мягко. Всё, управление отключается.
        hasLanded = true;

        // Принудительно гасим огонь, даже если кнопка нажата
        if (engineParticles) engineParticles.Stop();

        if (statusText) statusText.text = "Ждем стабилизации...";
        if (statusText) statusText.color = Color.yellow;
    }

    void CheckStability()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);

        // Если ракета стоит ровно
        if (angle < maxLandingAngle)
        {
            landingTimer += Time.fixedDeltaTime;

            if (statusText) statusText.text = $"УДЕРЖАНИЕ: {(timeToLand - landingTimer).ToString("F1")}";

            if (landingTimer >= timeToLand)
            {
                Win();
            }
        }
        else
        {
            // Если после посадки она начала крениться и упала (угол стал большим)
            Crash("Ракета упала!");
        }
    }

    void Win()
    {
        if (statusText) statusText.text = "УСПЕШНАЯ ПОСАДКА!";
        if (statusText) statusText.color = Color.green;
        isGameActive = false;
        rb.isKinematic = true; // Фиксируем победу
    }

    void Crash(string reason)
    {
        if (statusText) statusText.text = "АВАРИЯ:\n" + reason;
        if (statusText) statusText.color = Color.red;

        if (explosionEffect)
        {
            explosionEffect.transform.position = transform.position;
            explosionEffect.gameObject.SetActive(true);
            explosionEffect.Play();
            GetComponentInChildren<Renderer>().enabled = false;
        }

        isGameActive = false;
        if (engineParticles) engineParticles.Stop();
    }

    public void ResetRocket()
    {
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        GetComponentInChildren<Renderer>().enabled = true;

        float randomX = Random.Range(-2f, 2f);
        float randomY = Random.Range(2.0f, 4.0f);
        transform.localPosition = new Vector3(randomX, randomY, 0);

        float randomAngle = Random.Range(-45f, 45f);
        transform.localRotation = Quaternion.Euler(0, 0, randomAngle);

        isGameActive = true;
        hasLanded = false; // Снова разрешаем управление
        landingTimer = 0f;

        if (statusText) statusText.text = "";
        if (explosionEffect) explosionEffect.gameObject.SetActive(false);
    }

    public void StartEngine() => isThrusting = true;
    public void StopEngine() => isThrusting = false;
    public void StartLeft() => rotationDirection = -1;
    public void StopRotate() => rotationDirection = 0;
    public void StartRight() => rotationDirection = 1;
}