using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

using wowarmory;
using System.Diagnostics;

namespace wowarmory.SRP {
    public class ClientSRP {
        static readonly BigInteger g = 2;
        static readonly byte[] modulusBytes = new byte[] { 171, 36, 67, 99, 169, 194, 166, 195, 59, 55, 228, 97, 132, 37, 159, 139, 63, 203, 138, 133, 39, 252, 61, 135, 190, 160, 84, 210, 56, 93, 18, 183, 97, 68, 46, 131, 250, 194, 33, 217, 16, 159, 193, 159, 234, 80, 227, 9, 166, 229, 94, 35, 167, 119, 235, 0, 199, 186, 191, 248, 85, 138, 14, 128, 43, 20, 26, 162, 212, 67, 169, 212, 175, 173, 181, 225, 245, 172, 166, 19, 28, 105, 120, 100, 11, 123, 175, 156, 197, 80, 49, 138, 35, 8, 1, 161, 245, 254, 49, 50, 127, 226, 5, 130, 214, 11, 237, 77, 85, 50, 65, 148, 41, 111, 85, 125, 227, 15, 119, 25, 229, 108, 48, 235, 222, 246, 167, 134 };

        static readonly int ModulusSize = 128;
        static readonly BigInteger modulus = modulusBytes.ToPositiveBigInt();

        static readonly int SaltSize = 32;
        static readonly int HashSize = 32;
        static readonly int SessionKeySize = HashSize * 2;

        Random random = new Random();
        SHA256 hashing = SHA256.Create();

        public readonly BigInteger a;
        public readonly BigInteger A;
        public readonly BigInteger k;
        public readonly byte[] hN_xor_hg;

        public ClientSRP() {
            a = RandomCrypt();
            A = BigInteger.ModPow(g, a, modulus);

            var data = modulusBytes.Concat(g.ToByteArray()).ToArray();
            k = hashing.ComputeHash(data).ToPositiveBigInt();

            // xor H(N) and H(g)
            var nhash = hashing.ComputeHash(modulusBytes);
            var ghash = hashing.ComputeHash(g.ToByteArray());
            hN_xor_hg = new byte[nhash.Length];

            for (int i = 0; i < hN_xor_hg.Length; i++) {
                hN_xor_hg[i] = (byte)(nhash[i] ^ ghash[i]);
            }
        }

        public BigInteger RandomCrypt() {
            var size = ModulusSize * 2;
            var bytes = new byte[size];
            random.NextBytes(bytes);
            return bytes.ToPositiveBigInt() % modulus;
        }

        public byte[] GetChallengeA() {
            return A.ToByteArray().AdjustSize(ModulusSize);
        }

        public byte[] CalculateAuth1Proof(string userHash, string sessionPassword, byte[] salt, byte[] bbytes) {
            var abytes = GetChallengeA();
            var B = bbytes.ToPositiveBigInt();
            salt = salt.AdjustSize(SaltSize);

            var u = CalculateU(abytes, bbytes);
            var x = CalculateX(userHash, sessionPassword, salt);
            var S = CalculateS(B, x, u);
            var sessionKey = CalculateSessionKeyK(S);

            var userHash2 = hashing.ComputeHash(Encoding.ASCII.GetBytes(userHash));

            Debug.Assert(B < modulus);
            Debug.Assert(B != 0);

            // hash this to generate client proof, H(H(N) xor H(g) | H(userHash) | salt | A | B | K)
            var totalPayload = hN_xor_hg.Concat(userHash2).Concat(salt).Concat(abytes).Concat(bbytes).Concat(sessionKey).ToArray();
            return hashing.ComputeHash(totalPayload);
        }

        public BigInteger CalculateU(byte[] abytes, byte[] bbytes) {
            // H(A | B)
            var total = abytes.Concat(bbytes).ToArray();
            return hashing.ComputeHash(total).ToPositiveBigInt();
        }

        public BigInteger CalculateX(string userHash, string sessionPassword, byte[] salt) {
            var first_hash = hashing.ComputeHash(Encoding.ASCII.GetBytes(userHash + ":" + sessionPassword));

            // H(salt | H(userHash | : | sessionPassword))
            var total = salt.Concat(first_hash).ToArray();
            return hashing.ComputeHash(total).ToPositiveBigInt();
        }

        public BigInteger CalculateS(BigInteger B, BigInteger x, BigInteger u) {
            // (B - k * g^x) ^ (a + u * x)
            return BigInteger.ModPow(B - k * BigInteger.ModPow(g, x, modulus), a + u * x, modulus);
        }

        public byte[] CalculateSessionKeyK(BigInteger S) {
            var sessionKey = new byte[SessionKeySize];
            var bytesS = S.ToByteArray().AdjustSize(ModulusSize);
            var segmentLength = bytesS.Length;
            var offset = 0;
            var temp = new byte[ModulusSize];
            if ((segmentLength & 1) == 1) {
              offset = 1;
              segmentLength --;
            }

            segmentLength /= 2;

            if (segmentLength > ModulusSize)
              segmentLength = ModulusSize;

            temp = new byte[segmentLength];

            for (int i = 0; i < segmentLength; i++)
              temp[i] = bytesS[i * 2 + offset];

            var tempHash = hashing.ComputeHash(temp);
            for (int i = 0; i < HashSize; i++)
              sessionKey[i * 2] = tempHash[i];

            for (int i = 0; i < segmentLength; i++)
              temp[i] = bytesS[i * 2 + offset + 1];

            tempHash = hashing.ComputeHash(temp);
            for (int i = 0; i < HashSize; i++)
              sessionKey[i * 2 + 1] = tempHash[i];

            return sessionKey;
        }
    }
}
