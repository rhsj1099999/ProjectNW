using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : SubManager<PlayerDataManager>
{
    /*-----------------------------------------------------
    |NOTI| Scene�� ����� �� ����, ���� Scene���� ����ִ� ������
    ���� Scene�� ����������� �Ѵ�.
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
