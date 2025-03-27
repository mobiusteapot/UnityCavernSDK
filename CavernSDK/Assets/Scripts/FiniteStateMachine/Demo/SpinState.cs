using Spelunx.FSM;
using UnityEngine;

[CreateAssetMenu(fileName = "Spin", menuName = "FiniteStateMachine/Demo/Spin")]
public class SpinState : State {
    [SerializeField, Tooltip("How long until transition to another state.")] private float duration = 8.0f;
    [SerializeField] private float angularVelocity = 120.0f;

    private float timer = 0.0f;

    public override string GetName() { return "Spin"; }

    public override void OnEnter(FiniteStateMachine fsm, GameObject target) {
        Debug.Log(target.name + " is spinning!");
        timer = duration;
    }

    public override void OnUpdate(FiniteStateMachine fsm, GameObject target) {
        timer -= Time.deltaTime;
        if (timer < 0.0f) {
            int randomInt = Random.Range(0, 2);
            if (randomInt == 0 && fsm.HasState("Walk")) { fsm.ChangeState("Walk"); }
            if (randomInt == 1 && fsm.HasState("Idle")) { fsm.ChangeState("Idle"); }
        }
        target.transform.Rotate(new Vector3(0.0f, angularVelocity * Time.deltaTime, 0.0f));
    }
}