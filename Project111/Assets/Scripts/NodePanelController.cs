using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class NodePanelController : MonoBehaviour
{
    [Header("UI (Assign)")]
    public GameObject panelRoot;

    public TMP_Text titleText;
    public TMP_Text demandText;
    public TMP_Text deliveredText;
    public TMP_Text coverageText;
    public TMP_Text lackText;

    public Slider prioritySlider;
    public TMP_Text priorityValueText;

    public Button closeButton;

    [Header("Pick Settings")]
    public Camera cam;
    public LayerMask nodeLayer; // 노드 구슬 레이어만
    public float rayDistance = 2000f;

    private WaterNode selected;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (prioritySlider != null)
        {
            prioritySlider.minValue = 0.5f;
            prioritySlider.maxValue = 2.0f;
            prioritySlider.onValueChanged.RemoveAllListeners();
            prioritySlider.onValueChanged.AddListener(OnPriorityChanged);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(() => SetPanel(false));

        SetPanel(false);
    }

    private void Update()
    {
        // UI 위 클릭이면 무시 (슬라이더 조절할 때 Raycast가 먹는 문제 방지)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (TryPickNode(out var node))
                Select(node);
        }

        if (selected != null)
            RefreshTexts();
    }

    private bool TryPickNode(out WaterNode node)
    {
        node = null;
        if (cam == null) return false;

        var mouse = Mouse.current;
        if (mouse == null) return false;

        Vector2 pos = mouse.position.ReadValue();
        Ray r = cam.ScreenPointToRay(pos);

        if (Physics.Raycast(r, out RaycastHit hit, rayDistance, nodeLayer))
        {
            node = hit.collider.GetComponentInParent<WaterNode>();
            return node != null;
        }
        return false;
    }

    private void Select(WaterNode node)
    {
        selected = node;

        // District만 컨트롤
        if (selected.kind != NodeKind.District)
        {
            SetPanel(false);
            return;
        }

        SetPanel(true);
        RefreshTexts();

        // ✅ 노드 우선순위(priority) 조절
        if (prioritySlider != null)
        {
            prioritySlider.gameObject.SetActive(true);

            // selected.priority가 없으면 WaterNode에 필드 추가 필요
            prioritySlider.SetValueWithoutNotify(selected.priority);
            UpdatePriorityValueText(selected.priority);
        }
    }

    private void RefreshTexts()
    {
        if (selected == null) return;

        float pct = selected.coverage * 100f;
        float lack = Mathf.Max(0f, selected.demand - selected.delivered);

        if (titleText) titleText.text = selected.name;
        if (demandText) demandText.text = $"수요: {selected.demand:0.#}";
        if (deliveredText) deliveredText.text = $"공급: {selected.delivered:0.#}";
        if (coverageText) coverageText.text = $"공급률: {pct:0.#}%";
        if (lackText) lackText.text = $"부족: {lack:0.#}";

        // 보너스: 우선순위도 현재값 표시(디버그/가시성)
        if (priorityValueText) priorityValueText.text = $"우선순위: {selected.priority:0.00}";
    }

    private void OnPriorityChanged(float v)
    {
        if (selected == null) return;
        if (selected.kind != NodeKind.District) return;

        selected.priority = v;
        UpdatePriorityValueText(v);

        // 턴제라면 여기서 수치가 즉시 바뀌진 않고, NextTurn 후에 반영되는 게 정상
        // Debug.Log($"[NodePriority] {selected.name} priority={selected.priority:0.00}");
    }

    private void UpdatePriorityValueText(float v)
    {
        if (priorityValueText != null)
            priorityValueText.text = $"우선순위: {v:0.00}";
    }

    private void SetPanel(bool on)
    {
        if (panelRoot != null) panelRoot.SetActive(on);
    }
}