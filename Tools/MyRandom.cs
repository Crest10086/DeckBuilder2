using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace MyTools
{
    public class MyRandom
    {
        Random rand = null;

        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public MyRandom()
        {
            rand = new Random(GetRandomSeed());
        }

        public int Next()
        {
            return rand.Next();
        }

        public int Next(int max)
        {
            return rand.Next(max);
        }

        public int Next(int min, int max)
        {
            return rand.Next(min, max);
        }
    }
    /*
    public class MyRandom
    {
        private RNGCryptoServiceProvider rngp = null;
        private byte[] rb = null;

        public MyRandom()
        {
            rngp = new RNGCryptoServiceProvider();
            rb = new byte[4];
        }

        /// <summary>
        /// 产生一个非负数的乱数
        /// </summary>
        public int Next()
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            if (value < 0) value = -value;
            return value;
        }
        /// <summary>
        /// 产生一个非负数且最大值在 max 以下的乱数
        /// </summary>
        /// <param name="max">最大值</param>
        public int Next(int max)
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            value = value % (max + 1);
            if (value < 0) value = -value;
            return value;
        }
        /// <summary>
        /// 产生一个非负数且最小值在 min 以上最大值在 max 以下的乱数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public int Next(int min, int max)
        {
            int value = Next(max - min) + min;
            return value;
        }

     
    }
     * */
}
