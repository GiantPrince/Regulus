namespace TestLibrary
{
    public class Test
    {
        public static int Add(int a, int b)
        {
            int c = a + b;
            for (int i = 0; i < a; i++)
            {
                c += a;
                for (int j = 0; j < b; j++)
                {
                    c += b;
                }
            }
            return c;
        }

    }
}
