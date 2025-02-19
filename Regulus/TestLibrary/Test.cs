namespace TestLibrary
{
    public class Test
    {
        public static int Add()
        {
            int a = 10;
            for (int i = 0; i < 100; i++)
            {
                if (i % 2 == 0)
                {
                    a += 1 + i;
                    if (a >= 20)
                    {
                        a /= 2;
                    }
                }
                else
                {
                    a += i + 2;
                    if (a >= 30)
                    {
                        a *= 2;
                    }
                }
            }
            return a;
        }

    }
}
