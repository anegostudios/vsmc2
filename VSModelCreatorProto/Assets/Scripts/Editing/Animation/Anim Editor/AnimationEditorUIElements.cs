using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// To avoid an insanely huge animation manager class, this manages all the UI and UI events for animation.
    /// </summary>
    public class AnimationEditorUIElements : MonoBehaviour
    {
        public AnimationEditorManager animEditor;

        [Header("Animation Details - Animation Setup")]
        public GameObject animationSetupObjectsHolder;
        public Button newAnimation;
        public Button deleteAnimation;
        public Button copyAnimation;
        public TMP_InputField animName;
        public TMP_InputField animCode;
        public TMP_Dropdown onAnimStopped;
        public TMP_Dropdown onAnimEnded;
        public TMP_InputField animDurationInFrames;
        public GameObject onAnimDurationChangeConfirmationOverlay;

        [Header("Animation Details - Frame Progressor")]
        public TMP_Text currentFrameLabel;
        public Button playPauseButton;
        public TMP_Text playPauseButtonText;
        public Button jumpToStartButton;
        public Button jumpToEndButton;

        [Header("Animation Details - Timeline")]
        public RectTransform timelineCurrentFrameMarker;
        public float timelinePixelsPerFrame;
        public float timelinePixelsInitialOffset;
        public TimelineFrameline timelineFrameline;
        public bool wrapTimelineDragging;
        public TimelineManager timelineManager;

        [Header("Keyframe Details")]
        public GameObject selectedObjectDetailsHolder;
        public TMP_InputField selectedElementName;
        public Toggle positionKeyframeToggle;
        public TMP_InputField positionKeyframeX;
        public TMP_InputField positionKeyframeY;
        public TMP_InputField positionKeyframeZ;
        public Toggle rotationKeyframeToggle;
        public RotationSlider rotationKeyframeX;
        public RotationSlider rotationKeyframeY;
        public RotationSlider rotationKeyframeZ;

        public CanvasGroup positionKeyFrameBlocker;
        public CanvasGroup rotationKeyFrameBlocker;

        //Temp Data
        TaskSetAnimationDuration setAnimDurationTaskAwaitingConfirmation;

        void Awake()
        {
            //Anim Setup UI events.
            newAnimation.onClick.AddListener(OnNewAnimationClick);
            animName.onEndEdit.AddListener(x => { OnAnimationNameSubmit(x); });
            animCode.onEndEdit.AddListener(x => { OnAnimationCodeSubmit(x); });
            onAnimStopped.onValueChanged.AddListener(x => { OnAnimationStopSelected(x); });
            onAnimEnded.onValueChanged.AddListener(x => { OnAnimationEndSelected(x); });
            animDurationInFrames.onEndEdit.AddListener(x => { OnAnimationFrameQuantitySubmit(x); });

            timelineFrameline.onFrameSliderValueChanged.AddListener(x => { OnSetFrameViaTimeline(x); });

            //Keyframe Events
            positionKeyframeToggle.onValueChanged.AddListener(x => { OnPositionKeyframeToggled(x); });
            rotationKeyframeToggle.onValueChanged.AddListener(x => { OnRotationKeyframeToggled(x); });
            positionKeyframeX.onEndEdit.AddListener(x => { OnPositionXSubmit(x); });
            positionKeyframeY.onEndEdit.AddListener(x => { OnPositionYSubmit(x); });
            positionKeyframeZ.onEndEdit.AddListener(x => { OnPositionZSubmit(x); });
            rotationKeyframeX.rotSlider.onValueChanged.AddListener(x => { OnRotationXSubmit(x); });
            rotationKeyframeY.rotSlider.onValueChanged.AddListener(x => { OnRotationYSubmit(x); });
            rotationKeyframeZ.rotSlider.onValueChanged.AddListener(x => { OnRotationZSubmit(x); });

            //Frame slider and progressor events.
            //frameSlider.onValueChanged.AddListener((x) => { OnFrameSliderChangeValue(x); });
            //frameSliderGoBack1Frame.onClick.AddListener(() => { IncrementCurrentFrame(-1); });
            //frameSliderGoForward1Frame.onClick.AddListener(() => { IncrementCurrentFrame(1); });
            playPauseButton.onClick.AddListener(() => {
                animEditor.ToggleAnimationPlaying();
            });
        }

        public void SetIsPlayingButtonText()
        {
            if (animEditor.IsPlayingAnimation())
            {
                playPauseButtonText.text = "Pause";
            }
            else
            {
                playPauseButtonText.text = "Play";
            }
        }

        #region Selection Events
        /// <summary>
        /// This should be called whenever a new animation is selected, or when the current animation setup data is changed..
        /// </summary>
        public void RefreshUIForChangeInAnimation()
        {
            Animation cSel = GetCurrentAnimation();
            if (cSel == null)
            {
                animationSetupObjectsHolder.SetActive(false);
                return;
            }
            animationSetupObjectsHolder.SetActive(true);

            //Anim Setup Details
            animName.SetTextWithoutNotify(cSel.Name);
            animCode.SetTextWithoutNotify(cSel.Code);
            onAnimStopped.SetValueWithoutNotify((int)cSel.OnActivityStopped);
            onAnimEnded.SetValueWithoutNotify((int)cSel.OnAnimationEnd);
            animDurationInFrames.SetTextWithoutNotify(cSel.QuantityFrames.ToString());
            SetTimelineBounds(GetFrameDurationOfCurrentAnimation());
            RefreshUIForChangeInKeyframes();
        }

        /// <summary>
        /// This should be called whenever a keyframe is created or removed in any animation.
        /// </summary>
        public void RefreshUIForChangeInKeyframes()
        {
            //Refresh the animation timeline, 
            timelineManager.OnNewAnimationSelected(GetCurrentAnimation());
            RefreshUIForFrameElementSelection(GetCurrentFrame());
        }

        /// <summary>
        /// This should be called whenever the currently selected frame is changed, or the selected object changes.
        /// </summary>
        public void RefreshUIForFrameElementSelection(int cFrame)
        {
            //Simply refresh anything in the right-hand-side menu, as well as the frame timeline position.

            //Timeline...
            timelineCurrentFrameMarker.anchoredPosition = new Vector2(timelinePixelsInitialOffset + (timelinePixelsPerFrame * cFrame), 0);
            currentFrameLabel.text = GetTextForFrame(cFrame);
            SetIsPlayingButtonText();

            if (GetCurrentSelectedShapeElement() == null)
            {
                //If nothing is selected, then hide the selected element menu.
                selectedObjectDetailsHolder.SetActive(false);
                return;
            }
            selectedObjectDetailsHolder.SetActive(true);
            selectedElementName.SetTextWithoutNotify(GetCurrentSelectedShapeElement().Name);
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null)
            {
                if (kfe.PositionSet)
                {
                    positionKeyframeToggle.SetIsOnWithoutNotify(true);
                    positionKeyFrameBlocker.interactable = true;
                    positionKeyFrameBlocker.alpha = 1;
                    positionKeyframeX.SetTextWithoutNotify(kfe.OffsetX.Value.ToString("0.###"));
                    positionKeyframeY.SetTextWithoutNotify(kfe.OffsetY.Value.ToString("0.###"));
                    positionKeyframeZ.SetTextWithoutNotify(kfe.OffsetZ.Value.ToString("0.###"));
                }
                else
                {
                    positionKeyframeToggle.SetIsOnWithoutNotify(false);
                    positionKeyFrameBlocker.interactable = false;
                    positionKeyFrameBlocker.alpha = 0.5f;
                }
                if (kfe.RotationSet)
                {
                    rotationKeyframeToggle.SetIsOnWithoutNotify(true);
                    rotationKeyFrameBlocker.interactable = true;
                    rotationKeyFrameBlocker.alpha = 1;
                    rotationKeyframeX.SetToRotationValue((float)kfe.RotationX.Value);
                    rotationKeyframeY.SetToRotationValue((float)kfe.RotationY.Value);
                    rotationKeyframeZ.SetToRotationValue((float)kfe.RotationZ.Value);
                }
                else
                {
                    rotationKeyframeToggle.SetIsOnWithoutNotify(false);
                    rotationKeyFrameBlocker.interactable = false;
                    rotationKeyFrameBlocker.alpha = 0.5f;
                }
            }
            else
            {
                positionKeyframeToggle.SetIsOnWithoutNotify(false);
                positionKeyFrameBlocker.interactable = false;
                positionKeyFrameBlocker.alpha = 0.5f;
                rotationKeyframeToggle.SetIsOnWithoutNotify(false);
                rotationKeyFrameBlocker.interactable = false;
                rotationKeyFrameBlocker.alpha = 0.5f;
            }
        }



        #endregion

        #region UI Events - Animation Setup

        private void OnNewAnimationClick()
        {
            TaskCreateNewAnimation newTask = new TaskCreateNewAnimation();
            newTask.DoTask();
            UndoManager.main.CommitTask(newTask);
        }

        private void OnAnimationDeleteClick()
        {
            if (GetCurrentAnimation() == null) return;
            TaskDeleteAnimation delTask = new TaskDeleteAnimation(GetCurrentAnimation());
            delTask.DoTask();
            UndoManager.main.CommitTask(delTask);
        }
        
        private void OnAnimationCopyClick()
        {
            
        }

        private void OnAnimationNameSubmit(string newName)
        {
            if (GetCurrentAnimation() == null) return;
            if (newName.Length <= 2)
            {
                animName.SetTextWithoutNotify(GetCurrentAnimation().Name);
                return;
            }
            TaskSetAnimationName renameTask = new TaskSetAnimationName(GetCurrentAnimation(), newName);
            renameTask.DoTask();
            UndoManager.main.CommitTask(renameTask);
        }

        private void OnAnimationCodeSubmit(string newCode)
        {
            if (GetCurrentAnimation() == null) return;
            bool hasDup = false;
            foreach (Animation i in ShapeHolder.CurrentLoadedShape.Animations)
            {
                if (i.Code == newCode)
                {
                    hasDup = true;
                    break;
                }
            }
            if (newCode.Length <= 2 || hasDup)
            {
                animCode.SetTextWithoutNotify(GetCurrentAnimation().Name);
                return;
            }
            TaskSetAnimationCode changeCodeTask = new TaskSetAnimationCode(GetCurrentAnimation(), newCode);
            changeCodeTask.DoTask();
            UndoManager.main.CommitTask(changeCodeTask);
        }

        private void OnAnimationStopSelected(int newStop)
        {
            if ((int)GetCurrentAnimation().OnActivityStopped == newStop) return;
            TaskSetAnimationOnActivityStopped t = new TaskSetAnimationOnActivityStopped(GetCurrentAnimation(), (EnumEntityActivityStoppedHandling)newStop);
            t.DoTask();
            UndoManager.main.CommitTask(t);
        }

        private void OnAnimationEndSelected(int newEnd)
        {
            if ((int)GetCurrentAnimation().OnAnimationEnd == newEnd) return;
            TaskSetOnAnimationEnded t = new TaskSetOnAnimationEnded(GetCurrentAnimation(), (EnumEntityAnimationEndHandling)newEnd);
            t.DoTask();
            UndoManager.main.CommitTask(t);
        }

        private void OnAnimationFrameQuantitySubmit(string newFrameCount)
        {
            int nQ = GetCurrentAnimation().QuantityFrames;
            if (!int.TryParse(newFrameCount, out nQ))
            {
                animDurationInFrames.SetTextWithoutNotify(GetCurrentAnimation().QuantityFrames.ToString());
                return;
            }
            TaskSetAnimationDuration t = new TaskSetAnimationDuration(GetCurrentAnimation(), nQ);
            if (t.removedKeyframes.Count > 0)
            {
                //Need to confirm, since doing this will delete keyframes.
                setAnimDurationTaskAwaitingConfirmation = t;
                onAnimDurationChangeConfirmationOverlay.SetActive(true);
                return;
            }
            t.DoTask();
            UndoManager.main.CommitTask(t);

        }

        public void ConfirmAnimationFrameQuantitySubmit()
        {
            if (setAnimDurationTaskAwaitingConfirmation != null)
            {
                setAnimDurationTaskAwaitingConfirmation.DoTask();
                UndoManager.main.CommitTask(setAnimDurationTaskAwaitingConfirmation);
            }
            onAnimDurationChangeConfirmationOverlay.SetActive(false);
        }
        
        #endregion

        #region UI Events - Frame Progressor

        private void OnPlayPauseAnimationClicked()
        {

        }

        private void OnJumpToAnimationStartClicked()
        {

        }

        private void OnJumpToAnimationEndClicked()
        {

        }

        #endregion

        #region UI Events - Timeline

        /// <summary>
        /// This will be called by the frame timeline.
        /// It will set the current frame, which in turn will update the actual position of the slider.
        /// </summary>
        private void OnSetFrameViaTimeline(int frame)
        {
            if (!IsAnyAnimationSelected()) return;
            if (wrapTimelineDragging)
            {
                while (frame < 0)
                {
                    frame += GetFrameDurationOfCurrentAnimation();
                }
                while (frame >= GetFrameDurationOfCurrentAnimation())
                {
                    frame -= GetFrameDurationOfCurrentAnimation();
                }
            }
            else
            {
                frame = Mathf.Clamp(frame, 0, GetFrameDurationOfCurrentAnimation() - 1);
            }

            if (frame == GetCurrentFrame()) return;
            animEditor.SetAnimationFrame(frame);
        }

        #endregion

        #region UI Events - Keyframe Details

        private void OnPositionKeyframeToggled(bool on)
        {
            Animation anim = GetCurrentAnimation();
            int cFrame = GetCurrentFrame();
            ShapeElement cSel = GetCurrentSelectedShapeElement();
            if (anim == null || cSel == null) return;
            if (on)
            {
                TaskEnablePositionKeyframeForElement t = new TaskEnablePositionKeyframeForElement(anim, cFrame, cSel.Name);
                t.DoTask();
                UndoManager.main.CommitTask(t);
            }
            else
            {
                TaskDisablePositionKeyframeForElement t2 = new TaskDisablePositionKeyframeForElement(anim, cFrame, cSel.Name);
                t2.DoTask();
                UndoManager.main.CommitTask(t2);
            }
        }

        private void OnPositionXSubmit(string val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.PositionSet && float.TryParse(val, out float v))
            {
                TaskSetKeyframePositionForElement t = new TaskSetKeyframePositionForElement(kfe,
                v, kfe.OffsetY.GetValueOrDefault(0), kfe.OffsetZ.GetValueOrDefault(0));
                t.DoTask();
                UndoManager.main.CommitTask(t);
            }
            else
            {
                positionKeyframeX.SetTextWithoutNotify(GetCurrentSelectedKeyframeElement().OffsetX.GetValueOrDefault(0).ToString("0.###"));
            }
        }

        private void OnPositionYSubmit(string val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.PositionSet && float.TryParse(val, out float v))
            {
                TaskSetKeyframePositionForElement t = new TaskSetKeyframePositionForElement(kfe,
                kfe.OffsetX.GetValueOrDefault(0), v, kfe.OffsetZ.GetValueOrDefault(0));
                t.DoTask();
                UndoManager.main.CommitTask(t);
            }
            else
            {
                positionKeyframeY.SetTextWithoutNotify(GetCurrentSelectedKeyframeElement().OffsetY.GetValueOrDefault(0).ToString("0.###"));
            }
        }

        private void OnPositionZSubmit(string val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.PositionSet && float.TryParse(val, out float v))
            {
                TaskSetKeyframePositionForElement t = new TaskSetKeyframePositionForElement(kfe,
                kfe.OffsetX.GetValueOrDefault(0), kfe.OffsetY.GetValueOrDefault(0), v);
                t.DoTask();
                UndoManager.main.CommitTask(t);
            }
            else
            {
                positionKeyframeZ.SetTextWithoutNotify(GetCurrentSelectedKeyframeElement().OffsetZ.GetValueOrDefault(0).ToString("0.###"));
            }
        }

        private void OnRotationKeyframeToggled(bool on)
        {
            Animation anim = GetCurrentAnimation();
            int cFrame = GetCurrentFrame();
            ShapeElement cSel = GetCurrentSelectedShapeElement();
            if (anim == null || cSel == null) return;
            if (on)
            {
                TaskEnableRotationKeyframeForElement t = new TaskEnableRotationKeyframeForElement(anim, cFrame, cSel.Name);
                t.DoTask();
                UndoManager.main.CommitTask(t);
            }
            else
            {
                TaskDisableRotationKeyframeForElement t2 = new TaskDisableRotationKeyframeForElement(anim, cFrame, cSel.Name);
                t2.DoTask();
                UndoManager.main.CommitTask(t2);
            }
        }

        private void OnRotationXSubmit(float val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.RotationSet)
            {
                TaskSetKeyframeRotationForElement t = new TaskSetKeyframeRotationForElement(kfe,
                val, kfe.RotationY.GetValueOrDefault(0), kfe.RotationZ.GetValueOrDefault(0));
                t.DoTask();
                UndoManager.main.CommitTask(t);
                UndoManager.main.MergeTopTasks();
            }
            else
            {
                rotationKeyframeX.SetToRotationValue((float)kfe.RotationX.Value);
            }
        }

        private void OnRotationYSubmit(float val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.RotationSet)
            {
                TaskSetKeyframeRotationForElement t = new TaskSetKeyframeRotationForElement(kfe,
                kfe.RotationX.GetValueOrDefault(0), val, kfe.RotationZ.GetValueOrDefault(0));
                t.DoTask();
                UndoManager.main.CommitTask(t);
                UndoManager.main.MergeTopTasks();
            }
            else
            {
                rotationKeyframeY.SetToRotationValue((float)kfe.RotationY.Value);
            }
        }
        
        private void OnRotationZSubmit(float val)
        {
            AnimationKeyFrameElement kfe = GetCurrentSelectedKeyframeElement();
            if (kfe != null && kfe.RotationSet)
            {
                TaskSetKeyframeRotationForElement t = new TaskSetKeyframeRotationForElement(kfe,
                kfe.RotationX.GetValueOrDefault(0), kfe.RotationY.GetValueOrDefault(0), val);
                t.DoTask();
                UndoManager.main.CommitTask(t);
                UndoManager.main.MergeTopTasks();
            }
            else
            {
                rotationKeyframeZ.SetToRotationValue((float)kfe.RotationZ.Value);
            }
        }

        #endregion

        /// <summary>
        /// Sets the maximum length of the frame line.
        /// </summary>
        /// <param name="maxVal"></param>
        private void SetTimelineBounds(int maxVal)
        {
            timelineFrameline.SetPropertiesOfFrameline(maxVal);
        }


        #region Anim Utilities
        Animation GetCurrentAnimation()
        {
            return AnimationSelector.main.GetCurrentlySelected();
        }

        bool IsAnyAnimationSelected()
        {
            return AnimationSelector.main.IsAnySelected();
        }

        int GetFrameDurationOfCurrentAnimation()
        {
            return GetCurrentAnimation().QuantityFrames;
        }

        int GetCurrentFrame()
        {
            return animEditor.cFrame;
        }

        string GetTextForFrame(int forFrame)
        {
            return forFrame + "/" + GetFrameDurationOfCurrentAnimation() + "    " +
            ((int)(forFrame / 30)) + ":" + ((int)((forFrame / 30f % 1) * 100)).ToString("00") + "/" + ((int)(GetFrameDurationOfCurrentAnimation() / 30)) + ":" + ((int)((GetFrameDurationOfCurrentAnimation() / 30f % 1) * 100)).ToString("00");
        }

        /// <summary>
        /// Gets the keyframe for the 
        /// </summary>
        /// <returns></returns>
        AnimationKeyFrame GetCurrentSelectedKeyframe()
        {
            if (GetCurrentAnimation() == null) return null;
            return GetCurrentAnimation().KeyFrames.FirstOrDefault(x => x.Frame == GetCurrentFrame());
        }

        AnimationKeyFrameElement GetCurrentSelectedKeyframeElement()
        {
            if (GetCurrentSelectedShapeElement() == null) return null;
            AnimationKeyFrame cskf = GetCurrentSelectedKeyframe();
            if (cskf == null) return null;
            return cskf.Elements.FirstOrDefault(x => x.Key == GetCurrentSelectedShapeElement().Name).Value;
        }
        
        ShapeElement GetCurrentSelectedShapeElement()
        {
            if (!ObjectSelector.main.IsAnySelected()) return null;
            return ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        }

        #endregion
    }
}