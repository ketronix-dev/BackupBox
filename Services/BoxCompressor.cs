using System.IO;
using System.IO.Compression;

public class BoxCompressor
{
    // Метод для стиснення директорії в BOX (ZIP) файл
    public static void Compress(string sourceDirectory, string boxOutputPath)
    {
        if (Directory.Exists(sourceDirectory))
        {
            // Створюємо ZIP архів з розширенням .box
            ZipFile.CreateFromDirectory(sourceDirectory, boxOutputPath, CompressionLevel.Fastest, false);
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory '{sourceDirectory}' not found.");
        }
    }

    // Метод для розпаковування BOX (ZIP) файлу
    public static void Decompress(string boxFilePath, string destinationDirectory)
    {
        if (File.Exists(boxFilePath))
        {
            // Розпаковуємо ZIP архів з розширенням .box
            ZipFile.ExtractToDirectory(boxFilePath, destinationDirectory);
        }
        else
        {
            throw new FileNotFoundException($"File '{boxFilePath}' not found.");
        }
    }
}
