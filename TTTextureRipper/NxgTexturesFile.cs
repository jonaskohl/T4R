using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTextureRipper.IO;

namespace TTTextureRipper
{
    public class NxgTexturesFile : IDisposable
    {
        private FileStream fileOnDisk;

        private NxgTexturesFile() { }

        private int m_Size;
        private int m_Version;

        private StructNumber1[] structNumber1s;

        private struct StructNumber1
        {
            public int unknown;
            public int numberOfFiles;
            public int numberOfEntries;
            public int numberOfCharacters;
            public byte[] names;

            public StructNumber1_Entry[] entries;
        }

        private struct StructNumber1_Entry
        {
            public short lastContentAt;
            public short prevContentAt;
            public int textOffset;
            public short parent;

            internal long __length;
            internal long __offset;
        }

        public void Dispose()
        {
            fileOnDisk.Dispose();
        }

        public static NxgTexturesFile Open(string filename)
        {
            var nxg = new NxgTexturesFile();

            nxg.fileOnDisk = File.OpenRead(filename);
            nxg.ReadEntries();
            nxg.AbstractEntries();

            return nxg;
        }

        public byte[] GetEntryContents(NxgTexturesEntry entry)
        {
            var prevPos = fileOnDisk.Position;
            fileOnDisk.Seek(entry.OffsetInFile, SeekOrigin.Begin);

            byte[] data = new byte[entry.LengthInFile];

            using (var br = new BinaryReader(fileOnDisk))
                for (var i = 0; i < entry.LengthInFile; ++i)
                    data[i] = br.ReadByte();

            fileOnDisk.Seek(prevPos, SeekOrigin.Begin);

            return data;
        }

        private void AbstractEntries()
        {
            foreach (var e in structNumber1s)
            {
                var abstractEntry = new NxgEntry();

                abstractEntry.Name = e.GetHashCode().ToString("X8");

                foreach (var c in e.entries)
                {
                    var texEntry = new NxgTexturesEntry();
                    texEntry.Name = c.ToString();
                    texEntry.OffsetInFile = c.__offset;
                    texEntry.LengthInFile = c.__length;
                }
            }
        }

        private void ReadEntries()
        {
            int[] SUPPORTED_VERSIONS = new[] { 0x06 };

            using (var br = new BinaryReaderBE(fileOnDisk))
            {
                var f_Size = br.ReadInt32();    // 0x00
                var f__4CC = br.ReadBytes(4);   // 0x04
                var f_HSER_1 = br.ReadBytes(4); // 0x08
                var f_HSER_2 = br.ReadBytes(4); // 0x0C
                var f_Version = br.ReadInt32(); // 0x10
                var f_Number1 = br.ReadInt32(); // 0x14

                if (!SUPPORTED_VERSIONS.Contains(f_Version))
                    throw new UnsupportedFormatException("Does not support version " + f_Version);

                var l_Number1Items = new List<StructNumber1>();

                // "structnumber1"
                //for (var i = 0; i < f_Number1; ++i)
                for (var i = 0; i < f_Size; ++i)
                {
                    var s = new StructNumber1();

                    var f_Unknown = br.ReadInt32();
                    var f_NumberOfFiles = br.ReadInt32();
                    var f_NumberOfEntries = br.ReadInt32();
                    var f_NumberOfCharacters = br.ReadInt32();
                    var f_Names = br.ReadBytes(f_NumberOfCharacters);

                    var l_Entries = new List<StructNumber1_Entry>();

                    for (var j = 0; j < f_NumberOfEntries; ++j)
                    {
                        var e = new StructNumber1_Entry();

                        var i_currentOffset = fileOnDisk.Position;

                        var f_LastContentAt = br.ReadInt16();
                        var f_PrevContentAt = br.ReadInt16();
                        var f_TextOffset = br.ReadInt32();
                        var f_Parent = br.ReadInt16();

                        var i_deltaOffset = fileOnDisk.Position - i_currentOffset;

                        e.lastContentAt = f_LastContentAt;
                        e.prevContentAt = f_PrevContentAt;
                        e.textOffset = f_TextOffset;
                        e.parent = f_Parent;

                        e.__offset = i_currentOffset;
                        e.__length = i_deltaOffset;

                        l_Entries.Add(e);
                    }

                    s.unknown = f_Unknown;
                    s.numberOfFiles = f_NumberOfFiles;
                    s.numberOfEntries = f_NumberOfEntries;
                    s.numberOfCharacters = f_NumberOfCharacters;
                    s.names = f_Names;
                    s.entries = l_Entries.ToArray();

                    l_Number1Items.Add(s);
                }

                m_Size = f_Size;
                m_Version = f_Version;
                structNumber1s = l_Number1Items.ToArray();
            }
        }
    }

    public struct NxgEntry
    {
        public string Name;

        public NxgTexturesEntry[] entries;
    }

    public struct NxgTexturesEntry
    {
        public string Name;
        internal long OffsetInFile;
        internal long LengthInFile;
    }
}
