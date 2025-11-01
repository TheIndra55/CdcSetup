namespace CdcSetup
{
    public class FileEntry
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size of the data
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The size of the compressed data
        /// </summary>
        public int CompressedSize { get; set; }

        /// <summary>
        /// The file data
        /// </summary>
        public byte[] Data { get; set; }
    }
}
