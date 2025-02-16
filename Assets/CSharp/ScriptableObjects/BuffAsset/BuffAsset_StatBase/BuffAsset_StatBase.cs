using System;

public abstract class BuffAsset_StatBase : BuffAssetBase
{
    public enum BuffApplyType
    {
        //0. Set (강제로 고정시킨다)
        Set, //이 값이 있으면 이후 값들이 무시된다

        //1. 상수값 증가
        Plus,
        Minus,

        //2. 퍼센테이지 증가
        PercentagePlus,
        PercentageMinus,

        //3. 곱증가
        Multiply,
        Devide,

        End = 2048,
    }

    [Serializable]
    public class ApplyDescBase
    {
        public BuffApplyType _applyType = BuffApplyType.End;
        public int _amout = 0;
    }
}