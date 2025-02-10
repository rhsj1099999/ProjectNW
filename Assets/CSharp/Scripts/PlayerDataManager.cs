using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : SubManager<PlayerDataManager>
{
    /*-----------------------------------------------------
    |NOTI| Scene이 변경될 때 마다, 직전 Scene에서 들고있던 정보를
    다음 Scene에 유지시켜줘야 한다.
    -----------------------------------------------------*/

    public override void SubManagerInit()
    {
        SingletonAwake();
    }


    








    public override void SubManagerFixedUpdate() { }
    public override void SubManagerLateUpdate() {}
    public override void SubManagerStart() {}
    public override void SubManagerUpdate() {}
}
