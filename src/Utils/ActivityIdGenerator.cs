using System;
using System.IO;
using System.Text;

namespace Stream.Utils
{
    /// <summary>Utility class to generate a unique activity id.</summary>
    public static class ActivityIdGenerator
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long EpochTicks = Epoch.Ticks;

        // Difference in 100-nanosecond intervals between
        // UUID epoch (October 15, 1582) and Unix epoch (January 1, 1970)
        private static long UuidEpochDifference = 122192928000000000;

        /// <summary>Generates an Activity ID for the given epoch timestamp and foreign ID.</summary>
        public static Guid GenerateId(string foreignId, int epoch)
        {
            return GenerateId(foreignId, Epoch.AddSeconds(epoch));
        }

        /// <summary>
        /// Generates an Activity ID for the given timestamp and foreign ID.
        /// <paramref name="timestamp"/> must be UTC.
        /// </summary>
        /// <exception cref="ArgumentException">Raised if the timestamp kind if not UTC.</exception>
        public static Guid GenerateId(string foreignId, DateTime timestamp)
        {
            // The backend doesn't care about milliseconds, so we truncate the date here.
            var truncatedDate = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second, DateTimeKind.Utc);

            var unixNano = truncatedDate.Ticks - EpochTicks;
            var t = (ulong)(UuidEpochDifference + unixNano);

            long signedDigest;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(foreignId)))
            {
                var hashAsInt = Murmur3.Hash(stream);

                if (hashAsInt > int.MaxValue)
                    signedDigest = (long)hashAsInt - 4294967296;
                else
                    signedDigest = hashAsInt;
            }

            long signMask;
            if (signedDigest > 0)
            {
                signMask = 0x100000000;
            }
            else
            {
                signedDigest *= -1;
                signMask = 0x000000000;
            }

            signedDigest = (signedDigest | signMask) | 0x800000000000;

            var nodeBytes = PutUint64((ulong)signedDigest);

            var finalBytes = new byte[16];
            Array.Copy(PutUint32(TrimUlongToUint(t)), 0, finalBytes, 0, 4);
            Array.Copy(PutUint16(TrimUlongToUshort(t >> 32)), 0, finalBytes, 4, 2);
            Array.Copy(PutUint16((ushort)(0x1000 | TrimUlongToUshort(t >> 48))), 0, finalBytes, 6, 2);
            Array.Copy(PutUint16(TrimUlongToUshort(0x8080)), 0, finalBytes, 8, 2);

            // Now to the final
            Array.Copy(nodeBytes, 2, finalBytes, 10, 6);

            return new Guid(BytesToGuidString(finalBytes));
        }

        private static byte[] PutUint64(ulong v)
        {
            var b = new byte[8];
            b[0] = (byte)((v >> 56) & 0xFF);
            b[1] = (byte)((v >> 48) & 0xFF);
            b[2] = (byte)((v >> 40) & 0xFF);
            b[3] = (byte)((v >> 32) & 0xFF);
            b[4] = (byte)((v >> 24) & 0xFF);
            b[5] = (byte)((v >> 16) & 0xFF);
            b[6] = (byte)((v >> 8) & 0xFF);
            b[7] = (byte)(v & 0xFF);

            return b;
        }

        private static byte[] PutUint32(uint v)
        {
            var b = new byte[4];
            b[0] = (byte)((v >> 24) & 0xFF);
            b[1] = (byte)((v >> 16) & 0xFF);
            b[2] = (byte)((v >> 8) & 0xFF);
            b[3] = (byte)(v & 0xFF);

            return b;
        }

        private static byte[] PutUint16(ushort v)
        {
            var b = new byte[2];
            b[0] = (byte)((v >> 8) & 0xFF);
            b[1] = (byte)(v & 0xFF);

            return b;
        }

        private static uint TrimUlongToUint(ulong l)
        {
            return (uint)(l & 0xFFFFFFFF);
        }

        private static ushort TrimUlongToUshort(ulong l)
        {
            return (ushort)(l & 0xFFFF);
        }

        private static string BytesToGuidString(byte[] b)
        {
            var offsets = new int[16] { 0, 2, 4, 6, 9, 11, 14, 16, 19, 21, 24, 26, 28, 30, 32, 34 };
            var hexString = "0123456789abcdef";
            var retVal = new byte[36];
            for (var i = 0; i < b.Length; i++)
            {
                var value = b[i];
                retVal[offsets[i]] = (byte)hexString[value >> 4];
                retVal[offsets[i] + 1] = (byte)hexString[value & 0xF];
            }

            const int dash = 45; // The dash character '-'
            retVal[8] = retVal[13] = retVal[18] = retVal[23] = dash;

            return Encoding.UTF8.GetString(retVal);
        }
    }
}