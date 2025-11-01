namespace CdcSetup
{
    public class FileSystem
    {
        /// <summary>
        /// The file system header
        /// </summary>
        public Header Header { get; set; }

        /// <summary>
        /// The file entries
        /// </summary>
        public FileEntry[] Entries { get; set; }
    }
}
