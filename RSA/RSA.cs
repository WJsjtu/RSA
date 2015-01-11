using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using BigInteger;
using Int = BigInteger.BigInteger;
namespace RSA_T
{

    #region 使用大数类的RSA产生类，采用BigInteger.cs ，暂设最大十进制长度为100
    class RSAmaker
    {
        private readonly Int[] miniprime = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        public Int N, E, D, P, Q;

        private int bits, ac;
        private Random seed;

        //初始化
        public RSAmaker(Random seedin, int i_bits = 14, int i_ac = 20)
        {
            bits = i_bits;
            ac = i_ac;
            seed = seedin;
            RSAKey();
        }
        //加密
        public List<Int> Encrypt(ref string m)
        {
            Int check = E;
            int ebit = N.bitCount() / 8;
            int i = 0, l = m.Length;
            List<Int> encrypt = new List<Int>();
            Int temp;
            for (i = 0; i < l / ebit; i++)
            {
                temp = 0;
                for (int j = 0; j < ebit; j++)
                    temp = (temp <<= 8) | (int)m[ebit * i + j];
                encrypt.Add(temp.modPow(E, N));
            }
            if (ebit * i < l)
            {
                temp = 0;
                for (int j = 0; j < l % ebit; j++)
                    temp = (temp <<= 8) | (int)m[ebit * i + j];
                encrypt.Add(temp.modPow(E, N));
            }
            return encrypt;
        }
        //解密
        public string Decrypt(ref List<Int> m)
        {
            int l = m.Count;
            string ans = "";
            for (int i = 0; i < l; i++)
            {
                Int temp = m[i].modPow(D, N);
                string ts = "";
                while (temp != 0)
                {
                    ts = (char)(temp & 0xFF).IntValue() + ts;
                    temp >>= 8;
                }
                ans += ts;
            }
            return ans;
        }
        //加密数字以16进制显示成字符串
        public string getEncrypt(ref List<Int> m)
        {
            string res = "";
            foreach (Int i in m)
                res += i.ToHexString() + " ";
            res += "\n";
            return res;
        }
        //m_b素性检测和二次探查
        private bool MRT(Int p, Int k)
        {
            for (int i = 0; i < 25; i++)
                if ((p % miniprime[i]) == 0)
                    return false;
            for (int i = 0; i < k; i++)
            {
                Int a = RandInt(2, p - 2), x = a.modPow(p - 1, p);
                if (x == 1 || x == p - 1)
                    continue;
                else
                    return false;
            }
            return true; ;
        }
        //随机数字，设上下限
        private Int RandInt(Int lower, Int upper)
        {
            if (upper < lower)
            {
                Int temp = lower;
                lower = upper;
                upper = temp;
            }
            return seed.Next() % (upper - lower + 1) + lower;
        }
        //随机素数
        private Int RandPrime()
        {
            Int ans;
            while (true)
            {
                string move = "";
                for (int i = bits - 1; i != 0; i--)
                    move += "0";
                move = "1" + move;
                ans = RandInt(new Int(move, 2), new Int(move + "0", 2) - 1) | 1;
                if (MRT(ans, ac)) break;
            }
            return ans;
        }
        //获取广义欧几里得除法的系数
        private Int GetST(Int a, Int b, ref Int s, ref Int t)
        {
            bool reversed = false;
            if (b > a)
            {
                Int tmp = a; a = b; b = tmp;
                reversed = true;
            }
            Int ss = 1, tt = 0;
            s = 0; t = 1;
            while (b != 0)
            {
                Int temp = b, q = a / b;
                b = a % b; a = temp;
                temp = ss - q * s; ss = s;
                s = temp; temp = tt - q * t;
                tt = t; t = temp;
            }
            s = reversed ? tt : ss;
            t = reversed ? ss : tt;
            return a;
        }
        //获取D和E
        private bool RSAKey()
        {
            P = RandPrime();
            Q = RandPrime();
            N = P * Q; Int X = N - P - Q + 1;
            do E = RandInt(1, 65536) | 1;
            while (E.gcd(X) != 1);
            Int A = 0, B = 0;
            GetST(E, X, ref A, ref B);
            while (A < 0)
                A += X;
            D = A % N;
            return true;
        }
    };
    #endregion

    #region 使用long long 型存数的RSA产生类
    class S_RSAmaker
    {
        private readonly long[] miniprime = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

        public long N, E, D, P, Q;

        private Random seed;
        //随机数字，设上下限
        private long RandInt(long lower, long upper)
        {
            if (upper < lower)
            {
                long temp = lower;
                lower = upper;
                upper = temp;
            }
            return seed.Next() % (upper - lower + 1) + lower;
        }
        //初始化
        public S_RSAmaker(Random seedin)
        {
            seed = seedin;
            RSAKey(14, 20);
        }
        //重复模平方法
        private long ModPow(long a, long n, long p)
        {
            if (n == 0)
                return 1;
            long s = 1, t = a % p;
            while (n != 0)
            {
                if ((n & 1) == 1) s = (s * t) % p;
                t = (t * t) % p; n >>= 1;
            }
            return s;
        }
        //m_b素性检测和二次探查
        private bool MRT(long p, long k)
        {
            for (int i = 0; i < 25; i++)
                if ((p % miniprime[i]) == 0)
                    return false;
            for (int i = 0; i < k; i++)
            {
                long a = RandInt(2, p - 2), x = ModPow(a, p - 1, p);
                if (x == 1 || x == p - 1)
                    continue;
                else
                    return false;
            }
            return true; ;
        }
        //随机素数
        private long RandPrime(int bits, int ac = 20)
        {
            long ans;
            while (true)
            {
                ans = RandInt(((long)1) << (bits - 1), (long)((((long)1) << bits) - 1)) | 1;
                if (MRT(ans, ac)) break;
            }
            return ans;
        }
        //获取广义欧几里得除法的系数
        private long GetST(long a, long b, ref long s, ref long t)
        {
            bool reversed = false;
            if (b > a)
            {
                long tmp = a; a = b; b = tmp;
                reversed = true;
            }
            long ss = 1, tt = 0;
            s = 0; t = 1;
            while (b != 0)
            {
                long temp = b, q = a / b;
                b = a % b; a = temp;
                temp = ss - q * s; ss = s;
                s = temp; temp = tt - q * t;
                tt = t; t = temp;
            }
            s = reversed ? tt : ss;
            t = reversed ? ss : tt;
            return a;
        }
        //最大公因数
        private long Gcd(long a, long b)
        {
            if (a < b)
            {
                long temp = b; b = a; a = temp;
            }
            long r = a % b;
            while (r != 0)
            {
                a = b; b = r; r = a % b;
            }
            return b;
        }
        //获取D和E
        private bool RSAKey(int bits = 14, int ac = 20)
        {
            P = RandPrime(bits, ac);
            Q = RandPrime(bits, ac);
            N = P * Q; long X = N - P - Q + 1;
            do E = RandInt(1, 65536) | 1;
            while (Gcd(E, X) != 1);
            long A = 0, B = 0;
            GetST(E, X, ref A, ref B);
            while (A < 0)
                A += X;
            D = A % N;
            return true;
        }
        //加密
        public List<long> Encrypt(ref string m, int bit = 3)
        {
            int i = 0, l = m.Length;
            List<long> encrypt = new List<long>();
            long temp;
            for (i = 0; i < l / bit; i++)
            {
                temp = 0;
                for (int j = 0; j < bit; j++)
                    temp = (temp <<= 8) | m[bit * i + j];
                encrypt.Add(ModPow(temp, E, N));
            }
            if (bit * i < l)
            {
                temp = 0;
                for (int j = 0; j < l % bit; j++)
                    temp = (temp <<= 8) | m[bit * i + j];
                encrypt.Add(ModPow(temp, E, N));
            }
            return encrypt;
        }
        //解密
        public string Decrypt(ref List<long> m)
        {
            int l = m.Count;
            string ans = "";
            for (int i = 0; i < l; i++)
            {
                long temp = ModPow(m[i], D, N);
                string ts = "";
                while (temp != 0)
                {
                    ts = (char)(temp & 0xFF) + ts;
                    temp >>= 8;
                }
                ans += ts;
            }
            return ans;
        }
        //加密数字以16进制显示成字符串
        public string getEncrypt(ref List<long> m)
        {
            string res = "";
            string compare = "0123456789ABCDEF";
            for (int i = 0; i < m.Count; i++)
            {
                string strtmp = "";
                long tmp = m[i];
                for (int j = 0; j < 8; j++)
                {
                    strtmp = compare[(int)(tmp & 0xF)] + strtmp;
                    tmp >>= 4;
                }
                res += strtmp + "  ";
            }
            res += "\n";
            return res;
        }
    #endregion
    };
};