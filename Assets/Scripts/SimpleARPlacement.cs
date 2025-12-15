using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem; // <-- ВАЖНАЯ БИБЛИОТЕКА ДЛЯ НОВОГО ВВОДА

public class SimpleARPlacement : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject gameLevel; // Ссылка на наш AR_Game_Level
    public GameObject instructionsUI; // Текст "Найди пол и тапни"

    private bool isPlaced = false;

    void Update()
    {
        if (isPlaced) return; // Если уже поставили — ничего не делаем

        // ПРОВЕРКА НАЖАТИЯ (NEW INPUT SYSTEM)
        // Если тачскрина нет (например, тестируешь мышкой на ПК) или нет касания - выходим
        if (Touchscreen.current == null) return;

        // Проверяем, было ли нажатие в этом кадре (аналог TouchPhase.Began)
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            // Получаем позицию пальца
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            // Пускаем луч
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                // Нашли пол!
                Pose hitPose = hits[0].pose;

                // Переносим уровень в эту точку
                gameLevel.transform.position = hitPose.position;

                // Поворачиваем ракету к игроку (чтобы не стояла боком)
                Vector3 lookPos = new Vector3(Camera.main.transform.position.x, hitPose.position.y, Camera.main.transform.position.z);
                gameLevel.transform.LookAt(lookPos);
                gameLevel.transform.Rotate(0, 180, 0); // Разворот, если модель стоит спиной

                // Включаем уровень
                gameLevel.SetActive(true);

                // Прячем инструкцию
                if (instructionsUI) instructionsUI.SetActive(false);

                isPlaced = true; // Запоминаем, что игра началась
            }
        }
    }
}