using Spelunx.FSM;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "FiniteStateMachine/Demo/Idle")]
public class IdleState : State {
    [SerializeField, Tooltip("How long until transition to another state.")] private float duration = 3.0f;

    private float timer = 0.0f;

    public override string GetName() { return "Idle"; }

    public override void OnEnter(FiniteStateMachine fsm, GameObject target) {
        Debug.Log(target.name + " is idling!");
        timer = duration;
    }
    public override void OnUpdate(FiniteStateMachine fsm, GameObject target) {
        timer -= Time.deltaTime;
        if (timer < 0.0f) {
            int randomInt = Random.Range(0, 2);
            if (randomInt == 0 && fsm.HasState("Walk")) { fsm.ChangeState("Walk"); }
            if (randomInt == 1 && fsm.HasState("Spin")) { fsm.ChangeState("Spin"); }
        }
    }
}