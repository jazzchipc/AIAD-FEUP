//Each Agent will have a state machine, which functions according to the next diamond to catch
public class StateMachine
{
    private bool LEFT_FROM_TARGET;
    private bool RIGHT_FROM_TARGET;
    private bool UP_FROM_TARGET;
    private bool DOWN_FROM_TARGET;

    private bool WITH_OBSTACLE_BETWEEN;

    private bool ABOVE_OTHER_AGENT;
    private bool BELOW_OTHER_AGENT;
    private bool RIGHT_FROM_OTHER_AGENT;
    private bool LEFT_FROM_OTHER_AGENT;
    private bool NEAR_OTHER_AGENT;
}