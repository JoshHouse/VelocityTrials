using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator pAnimator;

    private Dictionary<string, string> animDict;

    private string currentAnim = "metarig|Idle";

    private bool keepPlaying = false;


    public void PlayAnim(string state)
    {
        if (!animDict.TryGetValue(state, out string animName))
            return;

        if (currentAnim == animName)
            return;

        if ((currentAnim == animDict["ShootGrapple"] ||
            currentAnim == animDict["Jump"] ||
            currentAnim == animDict["WallJumpLeft"] ||
            currentAnim == animDict["WallJumpRight"] ||
            currentAnim == animDict["WallJumpBack"] ||
            currentAnim == animDict["Mantle"]) &&
            keepPlaying)
            return;


        if (animName == animDict["ShootGrapple"] ||
            animName == animDict["Jump"] ||
            animName == animDict["WallJumpLeft"] ||
            animName == animDict["WallJumpRight"] ||
            animName == animDict["WallJumpBack"] ||
            animName == animDict["Mantle"])
        {
            pAnimator.Play(animName);
            currentAnim = animName;
            keepPlaying = true;
            return;
        }

        pAnimator.Play(animName);
        currentAnim = animName;
        return;
    }

    // Start is called before the first frame update
    void Awake()
    {
            animDict = new()
            {
                { "GrIdle", "metarig|Idle" },
                { "Walk", "metarig|Walking" },
                { "Sprint", "metarig|Sprinting" },
                { "CrouchIdle", "metarig|CrouchIdle" },
                { "CrouchWalk", "metarig|CrouchWalk" },
                { "Jump", "metarig|Jump" },
                { "Slide", "metarig|Sliding" },
                { "Mantle", "metarig|Mantle" },
                { "Climb", "metarig|WallClimb" },
                { "WallJumpBack", "metarig|WallJumpBack" },
                { "Airborne", "metarig|Airborne" },
                { "ShootGrapple", "metarig|ShootGrapple" },
                { "PullGrapple", "metarig|PullGrapple" },
                { "SwingGrapple", "metarig|SwingGrapple" },
                { "WallRunLeft", "metarig|WallRunningLeft" },
                { "WallJumpLeft", "metarig|WallJumpLeft" },
                { "WallRunRight", "metarig|WallRunningRight" },
                { "WallJumpRight", "metarig|WallJumpRight" },
            };
    }

    public void checkForPlayingAnimation()
    {
        AnimatorStateInfo stateInfo = pAnimator.GetCurrentAnimatorStateInfo(0);

        if ((stateInfo.IsName(animDict["ShootGrapple"])
            || stateInfo.IsName(animDict["Jump"])
            || stateInfo.IsName(animDict["WallJumpLeft"])
            || stateInfo.IsName(animDict["WallJumpRight"])
            || stateInfo.IsName(animDict["WallJumpBack"])
            || stateInfo.IsName(animDict["Mantle"]))
            && stateInfo.normalizedTime >= 1f)
        {
            keepPlaying = false;
        }
    }
}
