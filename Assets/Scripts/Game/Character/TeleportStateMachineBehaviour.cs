using UnityEngine;
using NaughtyAttributes;

public class TeleportStateMachineBehaviour : StateMachineBehaviour
{
    [Label("是否瞬移到攻擊點")][SerializeField] private bool _isToAttackPoint;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameplayManager.CurrentContext.CurrentTurnCharacter.TeleportToPos(_isToAttackPoint);
    }
}
