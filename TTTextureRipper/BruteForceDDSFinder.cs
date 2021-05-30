using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTextureRipper
{
    public static class BruteForceDDSFinder
    {
        static readonly byte[] ddsSignature = new byte[3] { 0x44, 0x44, 0x53 };

        public static IEnumerable<(long, long, byte[])> Find(Stream s)
        {
            using (var sr = new BinaryReader(s))
            {
                while (s.Position < s.Length)
                {
                    using (var ms = new MemoryStream())
                    using (var bw = new BinaryWriter(ms))
                    {
                        // seek for dds start
                        while (true)
                        {
                            if (IsDDSSignature(sr.ReadBytes(3)))
                            {
                                // we found the start of a dds file
                                //s.Seek(-3, SeekOrigin.Current);
                                bw.Write(ddsSignature);
                                Program.Log("[" + s.Position.ToString("X16") + "] Start of DDS file");
                                break;
                            }
                        }

                        var offset = s.Position;

                        // we are now at the start of a dds file
                        // start consuming it
                        long length = -1;
                        while (s.Position < s.Length)
                        {
                            var rbytes = sr.ReadBytes(3);
                            if (IsDDSSignature(rbytes))
                            {
                                s.Seek(-3, SeekOrigin.Current);
                                Program.Log("[" + s.Position.ToString("X16") + "] Found start of new DDS file");
                                length = s.Position - offset;
                                // we have reached the end of a dds file
                                goto END_OF_FILE;
                            }

                            bw.Write(rbytes);
                        }
                    END_OF_FILE:
                        Program.Log("End of DDS file");
                        yield return (offset, length, ms.ToArray());
                    }
                }
            }
        }

        private static bool IsDDSSignature(byte[] sig)
        {
            return sig.SequenceEqual(ddsSignature);
        }
    }
}
