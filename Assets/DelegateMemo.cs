/*
 //델리게이트 메모
 
 
델리게이트 정리
델리게이트 자체는 class이다.

delegate -> System.MulticastDelegate 받은 '클래스'임
그래서 단순 int, float 같은 값 저장이 아닌, Invoke 지원, 함수 포인터들을 저장함


delegate 를 쓸려면 반드시 typedef 가 필요하다.

public delegate void MyDelegate(int number);
               |반환|			|---인자---|


public MyDelegate _del = null; //그리고 사용할 수 있게 선언.
public int        _int = 0;    //이렇게

public event void MyDelegate(int number);
외부에서 Invoke 할 수 없게, 외부에서는 구독, 해지만 할 수 있게 하는
더 엄격한 델리게이트임




Action<int> 이것도 같은 말이다. 너무 많이 쓰여서 만들어둔거임.
근데 Action은 반환값이 있을 수 없다 -> Func 사용!

Func<int> 이것도 같은 말이다. 너무 많이 쓰여서 만들어둔거임. (마지막 타입이 반환값임)
근데 Func는 반환값이 없을 수 없다 -> Action 사용!



//람다와 델리게이트

C++에서는 람다 함수 재사용을 위해,
함수포인터레 람다함수를 저장하고 다시 쓸 수 있었다.

C#에서는 함수포인터가 없다. 반드시 람다함수를 '델리게이터에만' 저장해야한다.


'델리게이트' += () => print(myInt);
 (() => Console.WriteLine("Hello, World!"))();

근데 함수 내부에서 재사용 목적이라면
웬만하면 로컬함수 쓰세요



람다와 클로저
클로저는 **"람다가 자신을 둘러싼 환경(스코프)에서 선언된 변수들을 기억하고 사용할 수 있는 기능"**입니다.

 */