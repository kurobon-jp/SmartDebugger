using System.Collections.Generic;
using SimpleScroll;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal class DropdownTest : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler, IDataSource
    {
        [SerializeField] private Text _text;
        [SerializeField] private FixedListScroll _listScroll;
        [SerializeField] private DropdownTestItem _listItem;
        [SerializeField] private Dropdown.DropdownEvent _onValueChanged = new();

        private int _value = -1;
        private bool _isShown;
        private GameObject _blocker;
        private RectTransform _dropdownRectTransform;
        private RectTransform _listRectTransform;
        private readonly List<string> _selections = new();

        public int Value
        {
            get => _value;
            set => Set(value);
        }

        public Dropdown.DropdownEvent OnValueChanged
        {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        protected override void Awake()
        {
            _dropdownRectTransform = transform as RectTransform;
            _listRectTransform = _listScroll.transform as RectTransform;
            _listScroll.SetDataSource(this);
        }

        protected override void OnEnable()
        {
            Hide();
        }

        private void Show()
        {
            if (_isShown || _selections.Count == 0 || !IsActive() || !IsInteractable()) return;
            _isShown = true;
            // Get root Canvas.
            var canvases = ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, canvases);
            if (canvases.Count == 0)
                return;

            // case 1064466 rootCanvas should be last element returned by GetComponentsInParent()
            var listCount = canvases.Count;
            var rootCanvas = canvases[listCount - 1];
            for (var i = 0; i < listCount; i++)
            {
                if (canvases[i].isRootCanvas || canvases[i].overrideSorting)
                {
                    rootCanvas = canvases[i];
                    break;
                }
            }

            ListPool<Canvas>.Release(canvases);

            var rootCanvasRectTransform = (RectTransform)rootCanvas.transform;
            var rootCanvasRect = rootCanvasRectTransform.rect;
            var dropdownRect = _dropdownRectTransform.rect;

            // Reposition all items now that all of them have been added
            var sizeDelta = _listRectTransform.sizeDelta;
            sizeDelta.y = 80 * _selections.Count;
            sizeDelta.y = Mathf.Min((rootCanvasRect.height - dropdownRect.height) * 0.5f, sizeDelta.y);
            _listRectTransform.sizeDelta = sizeDelta;

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            var corners = new Vector3[4];
            _listRectTransform.GetWorldCorners(corners);

            for (var axis = 0; axis < 2; axis++)
            {
                var outside = false;
                for (var i = 0; i < 4; i++)
                {
                    var corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if ((corner[axis] < rootCanvasRect.min[axis] &&
                         !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
                        (corner[axis] > rootCanvasRect.max[axis] &&
                         !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
                    {
                        outside = true;
                        break;
                    }
                }

                if (outside)
                    RectTransformUtility.FlipLayoutOnAxis(_listRectTransform, axis, false, false);
            }

            _listScroll.gameObject.SetActive(true);
            _listScroll.SetPositionIndex(Value, 0f, false);
            _blocker = CreateBlocker(rootCanvas);
        }

        private void Hide()
        {
            if (!_isShown) return;
            _isShown = false;
            _listScroll.gameObject.SetActive(false);
            Destroy(_blocker);
            _blocker = null;
        }

        public int GetDataCount()
        {
            return _selections.Count;
        }

        public void SetData(int index, GameObject go)
        {
            if (go.TryGetComponent<DropdownTestItem>(out var item))
            {
                item.Setup(index, _selections[index], _value == index, value =>
                {
                    Value = value;
                    Hide();
                });
            }
        }

        public GameObject GetCellView(int index)
        {
            return _listItem.gameObject;
        }

        /// <summary>
        /// Handling for when the dropdown is initially 'clicked'. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// Handling for when the dropdown is selected and a submit event is processed. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// This will hide the dropdown list.
        /// </summary>
        /// <remarks>
        /// Called by a BaseInputModule when a Cancel event occurs.
        /// </remarks>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }

        /// <summary>
        /// Create a blocker that blocks clicks to other controls while the dropdown list is open.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a blocker GameObject.
        /// </remarks>
        /// <param name="rootCanvas">The root canvas the dropdown is under.</param>
        /// <returns>The created blocker object</returns>
        private GameObject CreateBlocker(Canvas rootCanvas)
        {
            // Create blocker GameObject.
            var blocker = new GameObject("Blocker");

            // Set the game object layer to match the Canvas' game object layer, as not doing this can lead to issues
            // especially in XR applications like PolySpatial on VisionOS (UUM-62470).
            blocker.layer = rootCanvas.gameObject.layer;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            var blockerCanvas = blocker.AddComponent<Canvas>();
            var dropdownCanvas = _listScroll.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;
            blockerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            blocker.AddComponent<GraphicRaycaster>();
            // Add image since it's needed to block, but make it clear.
            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            //add canvas group to ensure clicking outside the dropdown will hide it (UUM-33691)
            var blockerCanvasGroup = blocker.AddComponent<CanvasGroup>();
            blockerCanvasGroup.ignoreParentGroups = true;

            return blocker;
        }

        public void SetSelections(IEnumerable<string> selections)
        {
            _selections.Clear();
            _selections.AddRange(selections);
        }

        /// <summary>
        /// Set index number of the current selection in the Dropdown without invoking onValueChanged callback.
        /// </summary>
        /// <param name="input"> The new index for the current selection. </param>
        public void SetValueWithoutNotify(int input)
        {
            Set(input, false);
        }

        private void Set(int value, bool sendCallback = true)
        {
            if (Application.isPlaying && (value == _value || _selections.Count == 0))
                return;

            _value = Mathf.Clamp(value, 0, _selections.Count - 1);
            RefreshShownValue();

            if (sendCallback)
            {
                // Notify all listeners
                _onValueChanged?.Invoke(_value);
            }
        }

        private void RefreshShownValue()
        {
            _text.text = _selections.Count == 0 ? "" : _selections[_value];
        }
    }
}