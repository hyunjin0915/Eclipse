using System;

public class Child : Parent
{
    public Child()
    {

    }
    
    public override void Run()
    {
        Console.WriteLine("자식 클래스 함수 호출");
    }
}
