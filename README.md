Liskov Substitution Principle (LSP)

📌 المبدأ بيقول:

أي كلاس ابن لازم يقدر يستبدل الأب بتاعه من غير ما يكسر الكود أو يغير النتيجة المتوقعة.

Examble :

class Animal
{
    public virtual void Speak()
    {
        Console.WriteLine("Animal sound");
    }
}

class Dog : Animal
{
    public override void Speak()
    {
        Console.WriteLine("Bark");
    }
}


الاستخدام
Animal a = new Dog();
a.Speak();
