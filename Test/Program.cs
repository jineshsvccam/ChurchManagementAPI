using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string projectDirectory = @"C:\Users\USER\source\repos\ChurchManagementAPI";
        PrintDirectory(projectDirectory, 0);
        Console.ReadLine();
    }

    static void PrintDirectory(string dirPath, int indentLevel)
    {
        // Exclude hidden directories, .github, and bin/obj folders
        string[] directories = Directory.GetDirectories(dirPath, "*", SearchOption.TopDirectoryOnly);
        directories = Array.FindAll(directories, d => (new DirectoryInfo(d).Attributes & FileAttributes.Hidden) == 0);
        directories = Array.FindAll(directories, d => !Path.GetFileName(d).Equals("bin")
                                                     && !Path.GetFileName(d).Equals("obj")
                                                     && !Path.GetFileName(d).Equals(".github"));

        string indent = new string(' ', indentLevel * 4);

        foreach (string directory in directories)
        {
            Console.WriteLine($"{indent}- {Path.GetFileName(directory)}");
            PrintDirectory(directory, indentLevel + 1);
        }

        // Exclude hidden files and filter project-related files (e.g., .cs, .json, .csproj)
        string[] files = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);
        files = Array.FindAll(files, f => (new FileInfo(f).Attributes & FileAttributes.Hidden) == 0);
        files = Array.FindAll(files, f => f.EndsWith(".cs") || f.EndsWith(".json") || f.EndsWith(".csproj"));

        foreach (string file in files)
        {
            Console.WriteLine($"{indent}    - {Path.GetFileName(file)}");
        }
    }
}
