using SabreTools.IO.Compression.zlib;
using SabreTools.IO.Extensions;
using SabreTools.Serialization.Readers;
using System.IO;

namespace CdcSetup
{
    public class InstallerFileSystem(bool hasSize) : BaseBinaryReader<FileSystem>
    {
        public const int Signature = 0x49584653;

        public override FileSystem Deserialize(Stream data)
        {
            var fileSystem = new FileSystem();

            // Parse the header
            var header = ParseHeader(data);

            if (header.Signature != Signature)
            {
                return null;
            }

            fileSystem.Header = header;
            fileSystem.Entries = new FileEntry[header.NumFiles];

            // Parse the file entries
            for (var i = 0; i < header.NumFiles; i++)
            {
                fileSystem.Entries[i] = ParseFileEntry(data);
            }

            return fileSystem;
        }

        private Header ParseHeader(Stream data)
        {
            var header = new Header();

            header.Signature = data.ReadInt32LittleEndian();
            header.Size = data.ReadInt32LittleEndian();
            header.NumFiles = data.ReadInt32LittleEndian();

            return header;
        }

        private FileEntry ParseFileEntry(Stream data)
        {
            var entry = new FileEntry();

            entry.Name = data.ReadNullTerminatedAnsiString();
            data.Position += 256 - (entry.Name.Length + 1);

            // Size field only exists on installers after 2006
            if (hasSize)
            {
                entry.Size = data.ReadInt32LittleEndian();
            }

            entry.CompressedSize = data.ReadInt32LittleEndian();

            // Some of the installers contain a corrupt last file which is cut off
            if (data.Position + entry.CompressedSize > data.Length)
            {
                return entry;
            }

            // Read the file data
            var contents = data.ReadBytes(entry.CompressedSize);

            var decompressed = new MemoryStream();
            var decompressor = new ZlibInflateStream(new MemoryStream(contents));
            decompressor.CopyTo(decompressed);

            entry.Data = decompressed.ToArray();

            return entry;
        }
    }
}
