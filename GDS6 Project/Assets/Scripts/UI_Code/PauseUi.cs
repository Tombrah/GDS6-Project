using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUi : MonoBehaviour
{
    public static bool IsPaused;

    [SerializeField] private Button closeButton;
    [SerializeField] private Slider sensitivitySlider;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        sensitivitySlider.value = PlayerData.Instance.GetSensitivity();

        sensitivitySlider.onValueChanged.AddListener(delegate { ValueChanged(); });

        Hide();
    }

    private void ValueChanged()
    {
        PlayerData.Instance.SetSensitivity(sensitivitySlider.value);
    }

    public void Show()
    {
        IsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        IsPaused = false;
        if (!GameManager.Instance.IsGameOver())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        gameObject.SetActive(false);
    }
}
