using Spelunx.FSM;
using UnityEngine;

[CreateAssetMenu(fileName = "Walk", menuName = "FiniteStateMachine/Demo/Walk")]
public class WalkState : State {
    [SerializeField, Tooltip("How long until transition to another state.")] private float duration = 5.0f;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private Vector3 direction = Vector3.forward;

    private float timer = 0.0f;

    public override string GetName() { return "Walk"; }

    public override void OnEnter(FiniteStateMachine fsm, GameObject target) {
        Debug.Log(target.name + " is walking!");
        timer = duration;
        direction = new Vector3(Random.value * 2.0f - 1.0f, 0.0f, Random.value * 2.0f - 1.0f).normalized;
    }

    public override void OnUpdate(FiniteStateMachine fsm, GameObject target) {
        timer -= Time.deltaTime;
        if (timer < 0.0f) {
            int randomInt = Random.Range(0, 2);
            if (randomInt == 0 && fsm.HasState("Idle")) { fsm.ChangeState("Idle"); }
            if (randomInt == 1 && fsm.HasState("Spin")) { fsm.ChangeState("Spin"); }
            return;
        }
        target.transform.Translate(direction * Time.deltaTime * walkSpeed);
    }

    public override void OnExit(FiniteStateMachine fsm, GameObject target) {
        DemoData demoData = target.GetComponent<DemoData>();
        if (demoData != null) {
            Debug.Log(target.name + " has " +  demoData.healthPoints + " health points!");
        }
    }
}