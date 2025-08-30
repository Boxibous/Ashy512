using System.Security.Cryptography;
using System.Text;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Chase;

class Program
{
    static void Main(string[] str)
    {
        Console.WriteLine(Ashy512(string.Join(" ", str)));
        // Console.WriteLine(Convert.ToHexString(ashy512("ashy")));
        // Console.WriteLine(Convert.ToHexString(ashy512("bshy")));
    }
    static string Ashy512(string str) { return Convert.ToHexString(Ashy512(Encoding.UTF8.GetBytes(str))); }
    static byte[] Ashy512(byte[] str)
    {
        // im just blindly doing random stuff, dont mind, but it works
        static ulong Entropy(ulong val, ulong thresold, ulong entropy)
        {
            for (int k = 0; k < 256; k++) {
                val *= (val << 32) * (thresold ^ ((ulong)k + 0x48BCD3FFF));
                val ^= 0x487FC8AD8CBDA93B;
                val ^= (val << (int)(thresold & 63)) | (val >> (int)(thresold & 63));
                val ^= 0x000FFFFFFFFFFDCF ^ (((ulong)k + 0xF1) * 0x4C38FD4FFC);
                for (int i = 0; i < 4; i++)
                {
                    val *= ((val << (int)(thresold & 63)) | (val >> (int)(64 - (thresold & 63)))) ^ (val >> 32) * val ^ (entropy | thresold);
                }
            }
            return val;
        }
        ulong[] states = [
            0x00000FFFFFFFFFC5,
            0x487FC8AD8CBDA93B,
            0x00BCEDFE358CCB34,
            0xBBC384BBC483CBCB,
            0x19361d809cc845d5,
            0x00000FFFFFFFFDCF,
            0x85BBECBBEBC38547,
            0x284767364CEB28BB,
        ];
        long strdep = 0;
        for (int i = 0; i < 256; i++)
        {
            states[0] ^= ~Entropy(states[1], states[2], states[3]) ^ (str.Length > 0 ? str[i % str.Length] : states[4]);
            states[1] ^= ~Entropy(states[2], states[3], states[4]) ^ (str.Length > 0 ? str[i % str.Length] : states[5]);
            states[2] ^= ~Entropy(states[3], states[4], states[5]) ^ (str.Length > 0 ? str[i % str.Length] : states[6]);
            states[3] ^= ~Entropy(states[4], states[5], states[6]) ^ (str.Length > 0 ? str[i % str.Length] : states[7]);
            states[4] ^= ~Entropy(states[5], states[6], states[7]) ^ (str.Length > 0 ? str[i % str.Length] : states[0]);
            states[5] ^= ~Entropy(states[6], states[7], states[0]) ^ (str.Length > 0 ? str[i % str.Length] : states[1]);
            states[6] ^= ~Entropy(states[7], states[0], states[1]) ^ (str.Length > 0 ? str[i % str.Length] : states[2]);
            states[7] ^= ~Entropy(states[0], states[1], states[2]) ^ (str.Length > 0 ? str[i % str.Length] : states[3]);
        } // premix
        for (int i = 0; i < str.Length; i++)
        {
            strdep += (long)(str[i] + ((ulong)i * states[(i + 2) % 8]) ^ states[i % 8]);
            states[i % 8] *= (~(str[i] * (ulong)0x483BC73FFF83FB << 0x4A) << (int)(strdep & 63)) ^ states[2];
            states[i % 8] ^= Entropy(~Entropy(states[(i + 1) % 8], (ulong)strdep, states[(i + 2) % 8] ^ states[i % 8]), states[(i + 5) % 8], Entropy((ulong)strdep, states[i % 8], states[i % 8] ^ states[(i + 2) % 8]));
            states[(i + 1) % 8] *= ~(states[(i ^ 0x2) % 8] ^ states[i % 8]) ^ ~(ulong)strdep;
            states[(i + 2) % 8] ^= Entropy(((ulong)~str[i]), 1, states[i % 8]) ^ 0x27A48B;
            states[(i + 3) % 8] ^= (((ulong)str[i] << 0x1C | str[i]) ^ ((ulong)0x483BC73FFF83FB << 0x4A)) << (int)(strdep & 63);
            for (int k = 0; k < 256; k++)
            {
                states[(i + 4) % 8] ^= (states[i % 8] ^ states[(i + 2) % 8]) & Entropy(states[(i + 3) % 8] * ~states[(i + 1) % 8], (ulong)(str[i] ^ k + 0xF5), (ulong)strdep);
                states[(i + 5) % 8] ^= ~states[(i + 4) % 8] & states[(i + 2) % 8];
                states[(i + 6) % 8] ^= ~Entropy(states[(i + 5) % 8], states[(i + 4) % 8], states[(i + 3) % 8]);
                states[(i + 7) % 8] ^= ~Entropy(states[(i + 6) % 8], states[i % 8] ^ (ulong)k + 0xF5, states[(i % 8)]);
                states[(i + k) % 8] ^= (states[(i + 7) % 8] & states[(i + 5) % 8]) | states[(i + 5) % 8] ^ Entropy(states[i % 8], (ulong)strdep * ~states[k % 8], 0x483BC73FFF83FB);
            }
        }
        for (int i = 0; i < str.Length; i++)
        {
            states[0] *= (ulong)(str[i] + 2);
            states[0] ^= Entropy(states[3] ^ states[2], (~states[1] * states[2]) & 63, (ulong)strdep ^ states[0]);
            states[1] ^= Entropy(states[0] ^ states[4], (states[2] * states[1]) & 63, (ulong)strdep * ~states[1]);
            states[2] ^= Entropy(states[1] ^ states[5], (states[3] ^ states[4]) & 63, (ulong)strdep ^ states[2]);
            states[3] ^= Entropy(states[2] ^ states[6], (~states[4] ^ states[3]) & 63, (ulong)strdep * ~states[3]);
            states[4] ^= Entropy(states[3] ^ states[7], (states[5] * states[6]) & 63, (ulong)strdep ^ states[4]);
            states[5] ^= Entropy(states[4] ^ states[0], (states[6] ^ states[5]) & 63, (ulong)strdep ^ states[5]);
            states[6] ^= Entropy(states[5] ^ states[1], (~states[7] * states[0]) & 63, (ulong)strdep * ~states[6]);
            states[7] ^= Entropy(states[6] ^ states[2], (states[0] ^ states[1]) & 63, (ulong)strdep * ~states[7]);
        }
        byte[] bout = new byte[64];
        for (int i = 0; i < states.Length; i++)
        {
            byte[] bytes = BitConverter.GetBytes(states[i]);
            Array.Copy(bytes, 0, bout, i * 8, 8);
        }
        return bout;
    }
}