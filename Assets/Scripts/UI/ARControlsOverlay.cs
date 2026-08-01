using UnityEngine;
using UnityEngine.UI;

public class ARControlsOverlay : MonoBehaviour
{
    [SerializeField] Button rotateLeftButton;
    [SerializeField] Button rotateRightButton;
    [SerializeField] Button deleteButton;
    [SerializeField] Button resetButton;
    [SerializeField] CanvasGroup canvasGroup;

    FurnitureRotation rotation;
    DeleteFurniture deleteFurniture;
    FurnitureInteraction interaction;
    bool wired;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        TryWireControls();
    }

    void Update()
    {
        if (!wired)
            TryWireControls();

        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        if (interaction == null)
            interaction = FindObjectOfType<FurnitureInteraction>();

        bool hasSelection = interaction != null && interaction.selectedObject != null;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = hasSelection ? 1f : 0f;
            canvasGroup.interactable = hasSelection;
            canvasGroup.blocksRaycasts = hasSelection;
        }
    }

    void TryWireControls()
    {
        rotation = FindObjectOfType<FurnitureRotation>();
        deleteFurniture = FindObjectOfType<DeleteFurniture>();
        interaction = FindObjectOfType<FurnitureInteraction>();

        if (rotation == null || deleteFurniture == null)
            return;

        if (wired)
            return;

        wired = true;

        if (rotateLeftButton != null)
        {
            var trigger = rotateLeftButton.GetComponent<HoldButtonTrigger>();
            if (trigger == null)
                trigger = rotateLeftButton.gameObject.AddComponent<HoldButtonTrigger>();

            trigger.onPress.RemoveAllListeners();
            trigger.onRelease.RemoveAllListeners();
            trigger.onPress.AddListener(() => rotation.StartRotateLeft());
            trigger.onRelease.AddListener(() => rotation.StopRotateLeft());
        }

        if (rotateRightButton != null)
        {
            var trigger = rotateRightButton.GetComponent<HoldButtonTrigger>();
            if (trigger == null)
                trigger = rotateRightButton.gameObject.AddComponent<HoldButtonTrigger>();

            trigger.onPress.RemoveAllListeners();
            trigger.onRelease.RemoveAllListeners();
            trigger.onPress.AddListener(() => rotation.StartRotateRight());
            trigger.onRelease.AddListener(() => rotation.StopRotateRight());
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                deleteFurniture.DeleteSelectedFurniture();
                if (interaction != null) interaction.selectedObject = null;
            });
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() =>
            {
                if (interaction != null && interaction.selectedObject != null)
                {
                    interaction.selectedObject.transform.rotation = Quaternion.identity;
                    interaction.selectedObject.transform.localScale = Vector3.one * 0.3f;
                }
            });
        }
    }

    void OnDisable()
    {
        wired = false;
        rotation = null;
        deleteFurniture = null;
        interaction = null;
    }
}

public class HoldButtonTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
{
    public UnityEngine.Events.UnityEvent onPress = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent onRelease = new UnityEngine.Events.UnityEvent();

    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) => onPress.Invoke();
    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) => onRelease.Invoke();
}
