
public class Condition
{
    public Condition(ConditionDesc descRef)
    {
        _conditionDesc = descRef;
    }
    private ConditionDesc _conditionDesc; //Copy From ScriptableObject
    public ConditionDesc GetConditionDesc() { return _conditionDesc; }
}
