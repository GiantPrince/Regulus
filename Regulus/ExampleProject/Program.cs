using TestLibrary;

namespace Example
{
    
    public class Example
    {
        public static void Main(string[] args)
        {            
            Dog.Init();
            Dog dog = new Dog();
            dog.Bark();
            Console.WriteLine(dog.barkCount);
            Console.ReadKey();
        }
    }
}