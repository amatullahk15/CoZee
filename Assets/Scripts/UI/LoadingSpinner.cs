using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] GameObject spinnerRoot;
    [SerializeField] float spinSpeed = 180f;

    bool visible;

    void Awake()
    {
        if (spinnerRoot == null)
            spinnerRoot = gameObject;

        SetVisible(false);
    }

    void Update()
    {
        if (!visible)
            return;

        spinnerRoot.transform.Rotate(0f, 0f, -spinSpeed * Time.unscaledDeltaTime);
    }

    public void SetVisible(bool value)
    {
        visible = value;
        spinnerRoot.SetActive(value);
    }
}
