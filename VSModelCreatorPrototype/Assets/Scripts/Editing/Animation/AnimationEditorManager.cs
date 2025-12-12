using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// This is the entry point to any animation features. 
    /// Most animation classes will interact with this in one way or another.
    /// </summary>
    public class AnimationEditorManager : MonoBehaviour
    {
        [Header("Unity References")]
        public ShapeHolder shapeHolder;

        public GameObject animPrefab;
        public Transform animListParent;
        bool isInAnimatorMode = false;

        Dictionary<string, AnimationMetaData> allAnimations;
        Dictionary<string, AnimationMetaData> activeAnimations;

        ClientAnimator animator;

        private void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeSelect);
            EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselect);
        }

        void OnEditModeSelect(VSEditMode select)
        {
            if (select != VSEditMode.Animation) return;
            Shape shape = shapeHolder.cLoadedShape;
            System.DateTime t1 = System.DateTime.Now;
            shape.InitForAnimations("root");
            Debug.Log("Calculating " + shape.Animations.Length + " animations took " + (System.DateTime.Now - t1).TotalMilliseconds + "ms.");
            shapeHolder.ReparentGameObjectsByJoints();
            
            if (shape.Animations == null)
            {
                shape.Animations = new Animation[0];
            }
            animator = ClientAnimator.Create(shape.Animations, shape.Elements, shape.JointsById);

            //Create animation lists
            allAnimations = new Dictionary<string, AnimationMetaData>();
            activeAnimations = new Dictionary<string, AnimationMetaData>();
            int animID = 0;

            foreach (Transform child in animListParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var anim in shape.Animations)
            {
                AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                allAnimations.Add(anim.Code, meta);
                Instantiate(animPrefab, animListParent).GetComponent<AnimationEntryPrefab>().InitializePrefab(anim.Name, anim.Code, this);
                animID++;
            }

            isInAnimatorMode = true;
        }

        void OnEditModeDeselect(VSEditMode deselect)
        {
            if (deselect != VSEditMode.Animation) return;
            
            //All joints need zero-ing.
            for (int i = 0; i < shapeHolder.jointParents.Count; i++)
            {
                Transform joint = shapeHolder.jointParents[i];
                joint.localPosition = Vector3.zero;
                joint.localRotation = Quaternion.identity;
                joint.localScale = Vector3.one;
            }

            isInAnimatorMode = false;
        }

        private void Update()
        {
            if (animator == null || !isInAnimatorMode) return;
            animator.OnFrame(activeAnimations, Time.deltaTime);
            
            for (int i = 0; i < animator.MaxJointId; i++)
            {
                Transform joint = shapeHolder.jointParents[i];
                
                //Flip animation matrices too.
                Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                Matrix4x4 m = flipZ * animator.Matrices[i] * flipZ;

                joint.localPosition = m.GetPosition();
                joint.localRotation = m.rotation;
                joint.localScale = m.lossyScale;
            }

        }

        public void SetAnimationPlaying(string animID, bool isPlaying)
        {
            if (isPlaying)
            {
                activeAnimations.Add(animID, allAnimations[animID]);
            }
            else
            {
                activeAnimations.Remove(animID);
            }
        }



    }
}