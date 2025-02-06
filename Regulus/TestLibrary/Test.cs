namespace TestLibrary
{
    public class Test
    {
        public static int Add()
        {
            int i = 1;
            int j = 1;
            int k = 0;
            while (k < 100)
            {
                if (j < 20)
                {
                    j = k;
                    k = k + 2;
                }
                else
                {
                    j = i;
                    k = k + 1;
                }
            }
            return j;
        }

    }
}
