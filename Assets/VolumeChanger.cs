using UnityEngine;
using UnityEngine.UI;

public class VolumeChanger : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // Initialize the slider with the current volume
        volumeSlider.value = SoundManager.volume;

        // Add listener for when the slider value changes
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
    }

    private void OnVolumeSliderChanged(float value)
    {
        SoundManager.SetVolume(value);
    }
}
