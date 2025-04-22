using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Regulus
{
    public class ReferenceTest
    {
        public int a;
        public long b;
        public float c;
        public double d;
        
        public static int s_a;
        public string s;
        public static string s_s;
        public Dictionary<string, string> keyValuePairs;
        public ReferenceTest(int c) { a = c; s = "hello"; s_s = "hi"; keyValuePairs = new Dictionary<string, string>(); }
        ///public Dictionary<string, string> dic = new Dictionary<string, string>();

        public void Add(ref int a) { a += 1; }
        public void Out(out int a) { a = s_a; }
        public ref int getA() { return ref a; }
        public static int Add(int a, int b)
        {
            return a + b;
        }
        public static void Sub(ref int a, ref int b)
        {
            a -= 1;
            b -= 1;
        }

        public static void Sub(ref int a, int c, ref int b)
        {
            a -= c;
            b -= c;
        }

        
    }

    public class BaseTest
    {
        public string s;
        public static void Add(int a, int b)
        {
            Console.WriteLine(a + " " + b);
        }
    }

    public class Collider
    {
        public Transform transform { get; set; }
    }

    public struct RaycastHit
    {
        public Collider collider { get; set; }
    }

    public class Physics
    {
        public static bool RayCast(Ray ray, out RaycastHit hit)
        {
            hit = new RaycastHit();
            hit.collider = new Collider();
            hit.collider.transform = new Transform();
            hit.collider.transform.gameObject = new GameObject();
            hit.collider.transform.gameObject.transform = hit.collider.transform;
            hit.collider.transform.position = ray.origin;
            return true;
        }
    }

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }


    }

    public class GameObject
    {
        public Transform transform { get; set; }
    }

    public class Transform
    {
        public Vector3 position { get; set; }
        public Vector3 forward { get; set; }

        public GameObject gameObject { get; set; }

        public Transform GetChild(int index)
        {
            return this;
        }

        public int childCount { get { return 5; } }
        public bool GetVector(int n, out Vector3 vec)
        {
            
            vec = new Vector3(n, n, n);
            if (n > 0)
            {
                return true;
            }
            return false;
        }
        public void Rotate(float a, float b, float c)
        {
            //rs.a += (int)(a + b);
            //rot.a += (int)(a + c);
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        private static readonly Vector3 upVector = new Vector3(0f, 1f, 0f);
        private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 downVector = new Vector3(0f, -1f, 0f);

        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }


        public static Vector3 down
        {
            
            get
            {
                return downVector;
            }
        }

        public float magnitude
        {            
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }
        public static Vector3 up { get { return upVector; } }
        public static Vector3 one { get { return oneVector; } }
    }

    public struct ReferenceDataStruct
    {
        public int a;
        public double b;
        public ReferenceDataStruct(int c, double d) { a = c; b = d; }
    }

    public struct ReferenceStruct
    {
        public int a;
        public string s;
        private static ReferenceStruct upRef = new ReferenceStruct(5);

        public static ReferenceStruct up
        {
            get
            {
                return upRef;
            }
        }
        public ReferenceStruct(float d, float b, float c)
        {
            a = (int)(d + b + c);
            s = "hi";
        }
        public ReferenceStruct(int b)
        {
            a = b;
            s = "hello";
        }
        public int Get(int b)
        {
            return a + b;
        }

        public static ReferenceStruct operator +(ReferenceStruct a, ReferenceStruct b)
        {
            return new ReferenceStruct(a.a + b.a);
        }

        public static ReferenceStruct Cross(ReferenceStruct a, ReferenceStruct b)
        {
            return new ReferenceStruct(a.a * b.a);
        }
    }
}
