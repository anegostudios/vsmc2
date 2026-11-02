using VSMC;

public class TaskSetPlayAlongsideAnimation : IEditTask
{

    string animCode;
    string playAlongsideCode;
    public ElementCodeInShape addCode;
    string prevPlayAlongsideCode;

    public TaskSetPlayAlongsideAnimation(Animation anim, string playAlongsideCode, ElementCodeInShape forElem)
    {
        animCode = anim.Code;
        this.playAlongsideCode = playAlongsideCode;
        addCode = forElem;
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(addCode);
        prevPlayAlongsideCode = "";
        if (las.data.animationPlaybackMapping.ContainsKey(anim.Code))
        {
            prevPlayAlongsideCode = las.data.animationPlaybackMapping[anim.Code];
        }
    }

    public override void DoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(addCode);
        if (playAlongsideCode == "")
        {
            //Null, remove entry.
            las.data.animationPlaybackMapping.Remove(animCode);
        }
        else
        {
            las.data.animationPlaybackMapping[animCode] = playAlongsideCode;
        }
        AnimationEditorManager.main.OnAnimationDataChanged();
    }

    public override void UndoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(addCode);
        if (prevPlayAlongsideCode == "")
        {
            //Null, remove entry.
            las.data.animationPlaybackMapping.Remove(animCode);
        }
        else
        {
            las.data.animationPlaybackMapping[animCode] = prevPlayAlongsideCode;
        }
        AnimationEditorManager.main.OnAnimationDataChanged();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Animation;
    }

    public override string GetTaskName()
    {
        return "Set Play Alongside Animation";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
