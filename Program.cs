using CdcSetup;
using SabreTools.Data.Models.PortableExecutable.Resource;
using SabreTools.IO.Extensions;
using SabreTools.Serialization.Readers;
using System;
using System.CommandLine;
using System.IO;
using System.Text;

var fileArgument = new Argument<FileInfo>("path")
{
    Description = "The path of the file to extract"
};

var outputArgument = new Argument<DirectoryInfo>("output")
{
    Description = "The path of the output folder"
};

var formatOption = new Option<Format>("--revision")
{
    Description = "The installer revision, this should be new for 2007 installers and later"
};

var command = new RootCommand("Extracts a Crystal Dynamics setup file")
{
    fileArgument,
    outputArgument,
    formatOption,
};

command.SetAction(parseResult => Execute(
    parseResult.GetValue(fileArgument),
    parseResult.GetValue(outputArgument),
    parseResult.GetValue(formatOption))
);
command.Parse(args).Invoke();

void Execute(FileInfo path, DirectoryInfo output, Format format)
{
    var stream = path.OpenRead();

    // Open the executable
    var reader = new PortableExecutable();
    var executable = reader.Deserialize(stream);

    if (executable.ResourceDirectoryTable == null)
    {
        throw new Exception("Executable has no resources");
    }

    // Export all resources
    // TODO pass less arguments to every function?
    ExportDirectory(executable.ResourceDirectoryTable, null, output, format);
}

void ExportDirectory(DirectoryTable directory, string top, DirectoryInfo output, Format format)
{
    // Recursively export all resources
    foreach (var entry in directory.Entries)
    {
        if (entry.DataEntry != null)
        {
            ExportResource(entry.DataEntry, top, output, format);
        }

        if (entry.Subdirectory != null)
        {
            if (entry.Name != null)
            {
                top = Encoding.Unicode.GetString(entry.Name.UnicodeString);
            }

            ExportDirectory(entry.Subdirectory, top, output, format);
        }
    }
}

void ExportResource(DataEntry entry, string name, DirectoryInfo output, Format format)
{
    var data = new MemoryStream(entry.Data);
    var magic = data.ReadUInt32LittleEndian();

    // Check if the resource is an installer file system
    if (magic != InstallerFileSystem.Signature)
    {
        return;
    }

    Console.WriteLine("Opening " + name);

    // Seek back to the start
    data.Position = 0;

    var reader = new InstallerFileSystem(format == Format.New);
    var fileSystem = reader.Deserialize(data);

    var path = Path.Join(output.FullName, name);

    // Export all files
    ExportFileSystem(fileSystem, path);
}

void ExportFileSystem(FileSystem fileSystem, string path)
{
    // Make sure the output directory exists
    Directory.CreateDirectory(path);

    // Extract all files
    foreach (var item in fileSystem.Entries)
    {
        Console.WriteLine("Extracting " + item.Name);

        File.WriteAllBytes(Path.Join(path, item.Name), item.Data ?? []);
    }
}

enum Format
{
    Legacy,
    New
}