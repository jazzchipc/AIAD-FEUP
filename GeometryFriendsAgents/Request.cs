using System;

public abstract class Request
{
    protected enum Type { MOVE_LEFT, MOVE_RIGHT, MORPH_DOWN, MORPH_UP, ROLL_LEFT, ROLL_RIGHT, GROW};

    protected Type type;

    public abstract string getMessage();
}

public class MoveLeftRequest : Request
{
    public MoveLeftRequest()
    {
        this.type = Type.MOVE_LEFT;
    }

    public override string getMessage()
    {
        return this.type.ToString();
    }
}