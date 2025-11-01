namespace CdcSetup
{
    public class Header
    {
        /// <summary>
        /// "IXFS"
        /// </summary>
        public int Signature { get; set; }

        /// <summary>
        /// The size of the file system
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The number of files in the file system
        /// </summary>
        public int NumFiles { get; set; }
    }
}
