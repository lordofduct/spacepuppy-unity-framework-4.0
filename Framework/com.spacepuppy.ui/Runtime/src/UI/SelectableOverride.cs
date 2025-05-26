using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;

using com.spacepuppy.Events;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(Selectable))]
    [Infobox("This component must exist after/below the attached 'Selectable' on the GameObject which it is overriding for event timing to work correctly.")]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class SelectableOverride : SPMonoBehaviour, IObservableTrigger, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {

        public enum SelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }
        public static SelectionState GetCurrentSelectionState(Selectable selectable) => (SelectionState)SelectableProtectedHook.GetCurrentSelectionState_Internal(selectable);

        #region Fields

        [SerializeField, ForceFromSelf]
        private Selectable _selectable;

        [SerializeField, Tooltip("Highlight the selectable even if the mouse pointer isn't over it.")]
        private bool _overrideHighlighted;

        [SerializeField]
        private InterfacePicker<ITransition> _transition = new(null);

        [SerializeField, Tooltip("Activates if CurrentSelectionState or OverrideHighlighted changes.")]
        private SPEvent _onSelectionStateChanged = new SPEvent("OnSelectionStateChanged");

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            if (!_selectable) _selectable = this.GetComponent<Selectable>();
            base.Awake();
        }

        protected virtual void OnEnable()
        {
            if (!this.started) return;
            this.ForceEvaluateCurrentStateAndSync(true);
        }

        protected virtual void Start()
        {
            this.started = true;
            if (_selectable) _selectable.transition = Selectable.Transition.None;
            this.ForceEvaluateCurrentStateAndSync(true);
        }

        protected virtual void OnDisable()
        {
            this.SetCurrentSelectionState(SelectionState.Normal, true);
        }

        #endregion

        #region Properties

        public bool started { get; private set; }

        public Selectable Selectable => _selectable;

        public bool OverrideHighlighted
        {
            get => _overrideHighlighted;
            set
            {
                if (_overrideHighlighted == value) return;
                _overrideHighlighted = value;

#if UNITY_EDITOR
                if (Application.isPlaying && this.isActiveAndEnabled)
#else
                if (this.isActiveAndEnabled)
#endif
                {
                    int v = _onSelectionStateChanged.ActivationToken;
                    this.EvaluateCurrentStateAndSync(false);
                    if (v != _onSelectionStateChanged.ActivationToken && _onSelectionStateChanged.HasReceivers) _onSelectionStateChanged.ActivateTrigger(this, null);
                }
            }
        }

        public ITransition Transition
        {
            get => _transition.Value;
            set => _transition.Value = value;
        }

        public SelectionState CurrentSelectionState { get; private set; }

        public SPEvent OnSelectStateChanged => _onSelectionStateChanged;

        #endregion

        #region Methods

        public void SetCurrentSelectionState(SelectionState state, bool instant)
        {
            if (this.CurrentSelectionState != state)
            {
                this.CurrentSelectionState = state;
                this.Transition.PerformTransition(_selectable, state, instant);
#if UNITY_EDITOR
                if (Application.isPlaying && _onSelectionStateChanged.HasReceivers) _onSelectionStateChanged.ActivateTrigger(this, null);
#else
                if (_onSelectionStateChanged.HasReceivers) _onSelectionStateChanged.ActivateTrigger(this, null);
#endif
            }
        }

        public void ForceSetCurrentSelectionState(SelectionState state, bool instant)
        {
            this.CurrentSelectionState = state;
            this.Transition.PerformTransition(_selectable, state, instant);
#if UNITY_EDITOR
            if (Application.isPlaying && _onSelectionStateChanged.HasReceivers) _onSelectionStateChanged.ActivateTrigger(this, null);
#else
            if (_onSelectionStateChanged.HasReceivers) _onSelectionStateChanged.ActivateTrigger(this, null);
#endif
        }

        void EvaluateCurrentStateAndSync(bool instant)
        {
            var state = _selectable ? GetCurrentSelectionState(_selectable) : SelectionState.Normal;
            switch (state)
            {
                case SelectionState.Normal:
                    this.SetCurrentSelectionState(this.OverrideHighlighted ? SelectionState.Highlighted : state, instant);
                    break;
                case SelectionState.Highlighted:
                    this.SetCurrentSelectionState(state, instant);
                    break;
                case SelectionState.Pressed:
                    this.SetCurrentSelectionState(state, instant);
                    break;
                case SelectionState.Selected:
                    this.SetCurrentSelectionState(this.OverrideHighlighted ? SelectionState.Highlighted : state, instant);
                    break;
                case SelectionState.Disabled:
                default:
                    this.SetCurrentSelectionState(state, instant);
                    break;
            }
        }

        void ForceEvaluateCurrentStateAndSync(bool instant)
        {
            var state = _selectable ? GetCurrentSelectionState(_selectable) : SelectionState.Normal;
            switch (state)
            {
                case SelectionState.Normal:
                    this.ForceSetCurrentSelectionState(this.OverrideHighlighted ? SelectionState.Highlighted : state, instant);
                    break;
                case SelectionState.Highlighted:
                    this.ForceSetCurrentSelectionState(state, instant);
                    break;
                case SelectionState.Pressed:
                    this.ForceSetCurrentSelectionState(state, instant);
                    break;
                case SelectionState.Selected:
                    this.ForceSetCurrentSelectionState(this.OverrideHighlighted ? SelectionState.Highlighted : state, instant);
                    break;
                case SelectionState.Disabled:
                default:
                    this.ForceSetCurrentSelectionState(state, instant);
                    break;
            }
        }

        public void SimulatePressVisuals(float duration = 0.1f)
        {
            if (this.CurrentSelectionState == SelectionState.Pressed) return;

            this.StartCoroutine(this.DoSimulatePressVisuals(duration));
        }
        System.Collections.IEnumerable DoSimulatePressVisuals(float duration)
        {
            this.SetCurrentSelectionState(SelectionState.Pressed, false);
            yield return new WaitForSeconds(duration);
            this.EvaluateCurrentStateAndSync(false);
        }

        #endregion

        #region IPointerXXXX interface

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && _selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && _selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (_selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (_selectable && _selectable.IsActive() && _selectable.IsInteractable())
            {
                this.EvaluateCurrentStateAndSync(false);
            }
        }

        #endregion

        #region Special Types

        class SelectableProtectedHook : Selectable
        {
            const string PROP_CURRENTSELECTIONSTATE = nameof(SelectableProtectedHook.currentSelectionState);
            public static readonly System.Func<Selectable, int> GetCurrentSelectionState_Internal = typeof(Selectable).GetProperty(PROP_CURRENTSELECTIONSTATE, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty)
                                                                                                               .GetMethod.CreateDelegate(typeof(System.Func<Selectable, int>)) as System.Func<Selectable, int>;

        }

        #endregion

        #region ITransition Modes

        public interface ITransition
        {
            void PerformTransition(Selectable selectable, SelectionState state, bool instant);
        }

        [System.Serializable, SerializeRefLabel("Color Tint", Order = -3)]
        public class ColorTintTransition : ITransition
        {
            public ColorBlock colors = ColorBlock.defaultColorBlock;

            public void PerformTransition(Selectable selectable, SelectionState state, bool instant)
            {
                Color color;
                switch (state)
                {
                    case SelectionState.Normal:
                        color = colors.normalColor;
                        break;
                    case SelectionState.Highlighted:
                        color = colors.highlightedColor;
                        break;
                    case SelectionState.Pressed:
                        color = colors.pressedColor;
                        break;
                    case SelectionState.Selected:
                        color = colors.selectedColor;
                        break;
                    case SelectionState.Disabled:
                        color = colors.disabledColor;
                        break;
                    default:
                        color = Color.black;
                        break;
                }

                if (selectable && selectable.targetGraphic)
                {
                    selectable.targetGraphic.CrossFadeColor(color * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
                }
            }

        }

        [System.Serializable, SerializeRefLabel("Sprite Swap", Order = -2)]
        public class SpriteSwapTransition : ITransition
        {
            public SpriteState spriteState;

            public void PerformTransition(Selectable selectable, SelectionState state, bool instant)
            {
                Sprite spr;
                switch (state)
                {
                    case SelectionState.Normal:
                        spr = null;
                        break;
                    case SelectionState.Highlighted:
                        spr = spriteState.highlightedSprite;
                        break;
                    case SelectionState.Pressed:
                        spr = spriteState.pressedSprite;
                        break;
                    case SelectionState.Selected:
                        spr = spriteState.selectedSprite;
                        break;
                    case SelectionState.Disabled:
                        spr = spriteState.disabledSprite;
                        break;
                    default:
                        spr = null;
                        break;
                }

                if (selectable && selectable.image)
                {
                    selectable.image.overrideSprite = spr;
                }
            }
        }

        [System.Serializable, SerializeRefLabel("Animation", Order = -1)]
        public class AnimationTriggersTransition : ITransition
        {
            [SerializeField]
            private AnimationTriggers _animationTriggers = new();
            public AnimationTriggers animationTriggers => _animationTriggers;

            public void PerformTransition(Selectable selectable, SelectionState state, bool instant)
            {
                string triggername;
                switch (state)
                {
                    case SelectionState.Normal:
                        triggername = _animationTriggers.normalTrigger;
                        break;
                    case SelectionState.Highlighted:
                        triggername = _animationTriggers.highlightedTrigger;
                        break;
                    case SelectionState.Pressed:
                        triggername = _animationTriggers.pressedTrigger;
                        break;
                    case SelectionState.Selected:
                        triggername = _animationTriggers.selectedTrigger;
                        break;
                    case SelectionState.Disabled:
                        triggername = _animationTriggers.disabledTrigger;
                        break;
                    default:
                        triggername = string.Empty;
                        break;
                }

                if (string.IsNullOrEmpty(triggername)) return; //this mimics the way Selectable works...

                var animator = selectable?.animator;
                if (animator && animator.isActiveAndEnabled && animator.hasBoundPlayables)
                {
                    animator.ResetTrigger(_animationTriggers.normalTrigger);
                    animator.ResetTrigger(_animationTriggers.highlightedTrigger);
                    animator.ResetTrigger(_animationTriggers.pressedTrigger);
                    animator.ResetTrigger(_animationTriggers.selectedTrigger);
                    animator.ResetTrigger(_animationTriggers.disabledTrigger);
                    animator.SetTrigger(triggername);
                }
            }

            public static AnimationTriggersTransition Copy(AnimationTriggers triggers)
            {
                var result = new AnimationTriggersTransition();
                result.animationTriggers.normalTrigger = triggers.normalTrigger;
                result.animationTriggers.highlightedTrigger = triggers.highlightedTrigger;
                result.animationTriggers.pressedTrigger = triggers.pressedTrigger;
                result.animationTriggers.selectedTrigger = triggers.selectedTrigger;
                result.animationTriggers.disabledTrigger = triggers.disabledTrigger;
                return result;
            }
        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            if (_selectable && _selectable.transition != Selectable.Transition.None)
            {
                switch (_selectable.transition)
                {
                    case Selectable.Transition.ColorTint:
                        this.Transition = new ColorTintTransition()
                        {
                            colors = _selectable.colors,
                        };
                        break;
                    case Selectable.Transition.SpriteSwap:
                        this.Transition = new SpriteSwapTransition()
                        {
                            spriteState = _selectable.spriteState,
                        };
                        break;
                    case Selectable.Transition.Animation:
                        this.Transition = AnimationTriggersTransition.Copy(_selectable.animationTriggers);
                        break;
                }

                _selectable.transition = Selectable.Transition.None;
            }

            this.ForceEvaluateCurrentStateAndSync(true);
        }
#endif

    }

}
