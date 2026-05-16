using System;
using System.Collections.Generic;
using System.IO;

namespace GenericPoker
{
    public class SplitMix64
    {
        private ulong _state;

        public SplitMix64(ulong seed)
        {
            _state = seed + 0x9E3779B97f4A7C15UL;
        }

        public ulong Next()
        {
            _state += 0x9E3779B97f4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    public class Xoshiro256PlusPlus
    {
        private ulong[] s = new ulong[4];
        //private StreamWriter _logFile;
        public Xoshiro256PlusPlus(ulong seed)
        {
            var sm = new SplitMix64(seed);
            for (int i = 0; i < 4; i++)
                s[i] = sm.Next();
            
            //_logFile = new StreamWriter("C:\\LocalGameDev\\UnityProjects\\FantasyLongSong\\PokerAnalysis\\GenericPokerAnalysisAndTest\\rnd_log_cs.txt", append: false);
        }

        private static ulong RotL(ulong x, int k)
        {
            return (x << k) | (x >> (64 - k));
        }
        
        public ulong Next()
        {
            ulong result = RotL(s[0] + s[3], 23) + s[0];
            ulong t = s[1] << 17;

            s[2] ^= s[0];
            s[3] ^= s[1];
            s[1] ^= s[2];
            s[0] ^= s[3];

            s[2] ^= t;
            s[3] = RotL(s[3], 45);

            //_logFile.WriteLine(result);
            //_logFile.Flush();
            
            return result;
        }
    }

    public class XRandom
    {
        private Xoshiro256PlusPlus _rng;

        [ThreadStatic]
        private static XRandom _instance;

        public static XRandom Instance
        {
            get
            {
                if (_instance == null)
                    Init(); // Automatically initialize with default seed if accessed without Init
                return _instance;
            }
        }
        
        public XRandom(ulong seed)
        {
            _rng = new Xoshiro256PlusPlus(seed);
        }

        public static void Init(ulong seed = 1234567)
        {
            _instance = new XRandom(seed);
        }
        
/*
        public void Seed(ulong seed)
        {
            _rng = new Xoshiro256PlusPlus(seed);
        }
*/

        public ulong Next()
        {
            return _rng.Next();
        }
        
        // Get random numbers between min to max
        public int NextInt(int min, int max)
        {
            if (min > max) throw new ArgumentException("min > max");
            ulong range = (ulong)(max - min + 1);
            return (int)((ulong)min + (_rng.Next() % range));
        }


/*
        public float NextFloat()
        {
            return (float)((_rng.Next() >> 11) * (1.0 / (1UL << 53)));
        }
*/

        public void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = NextInt(0, i);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

/*
        public T Choice<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Cannot choose from an empty list");
            return list[NextInt(0, list.Count - 1)];
        }
*/
    }
}