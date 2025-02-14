namespace TestLibrary
{
    public class Test
    {
        public static int Add()
        {
            int i = 1;
            int j = 1;
            int k = 0;

            while (k < 10)
            {
                if (i == j)
                {
                    k = i;
                    i = k + 1;
                }
                else
                {
                    k = j;
                    j = k + 2;
                }
            }
            return i;
        }

    }
}
