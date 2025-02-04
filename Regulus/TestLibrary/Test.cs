namespace TestLibrary
{
    public class Test
    {
        public static int Add(int a, int b)
        {
            int sum = a + b;
            for (int i = 0; i < a; i++)
            {
                sum = sum + b;
                sum = sum * a;
                sum = sum / b;
                sum = sum % b;
                
            }
            Console.WriteLine(sum);
            return sum;
        }

    }
}
